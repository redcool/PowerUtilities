#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.UI;
    using System.Text;
    using UnityEngine.Rendering;
    using System.IO;
    using System.Linq;
    using System;
    using System.Text.RegularExpressions;
    using System.Net;

    /// <summary>
    /// UIMaterial code generate
    /// can k framework this generated script,that control ui material
    /// </summary>
    public class UIMaterialPropCodeGen
    {
        const string MENU_ROOT = "PowerUtilities/CodeGenerator";
        const string PATH = "Assets/CodeGens/";

        static HashSet<string> propNameSet = new HashSet<string>();
        /// <summary>
        /// {
        ///     propName -> groupName
        /// }
        /// when propName == groupName, is header
        /// </summary>
        static Dictionary<string, (string groupName, bool isHeader)> propLayoutDict = new Dictionary<string, (string groupName, bool isHeder)>();
        static Shader currentShader;

        [MenuItem(MENU_ROOT + "/CreateUIMaterialCode(from hierarchy ui)")]
        static void Init()
        {
            var graphs = SelectionTools.GetSelectedComponents<MaskableGraphic>();

            foreach (var item in graphs)
            {
                var mat = item.material;
                Analysis(mat.shader);
            }
        }

        [MenuItem(MENU_ROOT + "/CreateUIMaterialCode(from Asset Shader)")]
        static void Init2()
        {
            var shaders = Selection.GetFiltered<Shader>(SelectionMode.Assets);

            foreach (var item in shaders)
            {
                Analysis(item);
            }
        }

        /// <summary>
        /// analysis layout file
        /// {
        ///     prop -> (groupName, isHeader)    
        /// }
        /// </summary>
        /// <param name="shader"></param>
        static void SetupPropLayoutDict(Shader shader)
        {
            propLayoutDict.Clear();

            var shaderPath = AssetDatabase.GetAssetPath(shader);
            var layoutFilePath = shaderPath.Substring(0, shaderPath.Length - Path.GetExtension(shaderPath).Length) + ".txt";
            //var layoutText = AssetDatabase.LoadAssetAtPath<TextAsset>(layoutFilePath);

            var dict = ConfigTool.ReadKeyValueConfig(layoutFilePath);

            dict.ForEach(kv =>
            {
                if (kv.Key != "tabNames")
                {
                    var propNames = ConfigTool.SplitBy(kv.Value);
                    for (int i = 0; i < propNames.Length; i++)
                    {
                        var prop = propNames[i];
                        propLayoutDict[prop] = (kv.Key, i == 0);
                    }
                }
            });

        }

        static void Analysis(Shader shader)
        {
            currentShader = shader;
            // setup layout
            SetupPropLayoutDict(shader);

            // setup props

            var fieldSb = new StringBuilder();
            var updateMatMethodSb = new StringBuilder();
            var updateBlockMethodSb = new StringBuilder();
            var readMatSb = new StringBuilder();

            var propCount = shader.GetPropertyCount();
            for (int i = 0; i < propCount; i++)
            {
                var propFlags = shader.GetPropertyFlags(i);

                if (!IsValidFlags(propFlags))
                {
                    continue;
                }

                var propName = shader.GetPropertyName(i);
                var propType = shader.GetPropertyType(i);
                var propDefaultValue = GetDefaultValue(i, propType, shader);

                AnalysisProp(i, propFlags, propType, propName, propDefaultValue,
                    fieldSb, updateMatMethodSb, updateBlockMethodSb, readMatSb);
            }

            PathTools.CreateAbsFolderPath(PATH);

            var className = Regex.Replace(shader.name, @"[/ ]", "_");
            var codeStr = string.Format(CODE_TEMPLATE,
                className,
                fieldSb.ToString(),
                updateMatMethodSb.ToString(),
                updateBlockMethodSb.ToString(),
                readMatSb.ToString()
                );

            var path = $"{PathTools.GetAssetAbsPath(PATH)}/{className}.cs";
            File.WriteAllText(path, codeStr);

            propNameSet.Clear();
            fieldSb.Clear();
            updateMatMethodSb.Clear();
            updateBlockMethodSb.Clear();

            AssetDatabase.Refresh();
            currentShader = null;
        }
        /// <summary>
        /// filter material property by flags
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        private static bool IsValidFlags(ShaderPropertyFlags flags) =>
            (ShaderPropertyFlags.HideInInspector & flags) == 0
            && (ShaderPropertyFlags.PerRendererData & flags) == 0
        ;

        public static string FormatVector(Vector4 v)
        {
            return $"({v.x}f,{v.y}f,{v.z}f,{v.w}f)";
        }

        public static string GetDefaultValue(int id, ShaderPropertyType type, Shader shader) => type switch
        {
            //ShaderPropertyType.Texture => $"Texture2D.{shaders.GetPropertyTextureDefaultName(id)}Texture",
            ShaderPropertyType.Texture => $"null",
            ShaderPropertyType.Color => $"new Color{FormatVector(shader.GetPropertyDefaultVectorValue(id))}",
            ShaderPropertyType.Vector => $"new Vector4{FormatVector(shader.GetPropertyDefaultVectorValue(id))}",
            _ => $"{shader.GetPropertyDefaultFloatValue(id)}f"
        };

        static bool IsPropExisted(string propName)
        {
            var isExisted = propNameSet.Contains(propName);
            if (!isExisted)
            {
                propNameSet.Add(propName);
            }
            return isExisted;
        }

        public static void AnalysisProp(int propId, ShaderPropertyFlags flags, ShaderPropertyType type, string propName, string propValue,
            StringBuilder fieldSb,
            StringBuilder updateMatSb,
            StringBuilder updateBlockSB,
            StringBuilder readMatSb
            )
        {
            //Check repeatted propName
            if (IsPropExisted(propName))
                return;

            var varTypeName = GetVarableType(type);
            var setMethodName = GetSetMethodName(type);

            var varDecorator = GetVarDecorator(propId, propName, flags, type);
            /** 
             * demo : 
             *      [ColorUsage(true,true)] public Color color = shaderDefaultValue
             *      [EditorGroup(groupName,isHeader)]
             */
            fieldSb.AppendLine($"{varDecorator} public {varTypeName} {propName} = {propValue};");

            var textureCondition = "";
            if (type == ShaderPropertyType.Texture)
            {
                textureCondition = $"if({propName}!=null)";
            }
            // demo: if(mat.HasFloat("_Value"))
            var matPropCondition = GetPropCondition(setMethodName, propName);
            var blockPropCondition = GetPropCondition(setMethodName, propName,"block");

            //demo : mat.SetColor("_Color",color);
            updateMatSb.AppendLine($"{matPropCondition} {textureCondition} mat.Set{setMethodName}(\"{propName}\", {propName});");
            updateBlockSB.AppendLine($"{blockPropCondition} {textureCondition} block.Set{setMethodName}(\"{propName}\", {propName});");

            // demo: color = mat.GetColor("_Color");
            readMatSb.AppendLine($"{matPropCondition} {propName} = mat.Get{setMethodName}(\"{propName}\");");

            string GetPropCondition(string setMethodName,string propName,string invokerName = "mat")
                => $"if ({invokerName}.Has{setMethodName} (\"{propName}\"))";
        }

        /// <summary>
        /// add variables decorator
        /// 
        /// demo : 
        /// [ColorUsage(true, true)]
        /// [EditorGroup(groupName, isHeader)]
        /// public Color color = shaderDefaultValue
        ///      
        ///
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private static string GetVarDecorator(int propId, string propName, ShaderPropertyFlags flags, ShaderPropertyType type)
        {
            var sb = new StringBuilder();
            // texturePreview
            if (type == ShaderPropertyType.Texture)
            {
                sb.AppendLine("[TexturePreview]");
            }
            // add group
            if (propLayoutDict.ContainsKey(propName))
            {
                var groupInfo = propLayoutDict[propName];
                sb.AppendFormat("[EditorGroup(\"{0}\",{1})]", groupInfo.groupName, groupInfo.isHeader ? "true" : "false");
            }
            // add ColorUsage
            if ((flags & ShaderPropertyFlags.HDR) > 0)
            {
                sb.Append("[ColorUsage(true,true)]");
            }
            // add Range
            if (type == ShaderPropertyType.Range)
            {
                var range = currentShader.GetPropertyRangeLimits(propId);
                sb.AppendFormat(@"[Range({0},{1})]", range.x, range.y);
            }

            return sb.ToString();
        }

        public static string GetVarableType(ShaderPropertyType type) => type switch
        {
#if UNITY_2021_1_OR_NEWER
            ShaderPropertyType.Int => "int",
#endif
            ShaderPropertyType.Range => "float",
            ShaderPropertyType.Float => "float",
            ShaderPropertyType.Texture => "Texture",
            ShaderPropertyType.Vector => "Vector4",
            _ => Enum.GetName(typeof(ShaderPropertyType), type),
        };

        public static string GetSetMethodName(ShaderPropertyType type) => type switch
        {
            ShaderPropertyType.Range => "Float",
            _ => Enum.GetName(typeof(ShaderPropertyType), type)
        };

        public const string CODE_TEMPLATE = @"
using UnityEngine;

namespace PowerUtilities
{{
    /// <summary>
    /// generated code by UIMaterialPropCodeGen
    /// </summary>
    [ExecuteAlways]
    public class {0} : BaseUIMaterialPropUpdater
    {{
        // props
        {1}

        public override void ReadFirstMaterial(Material mat)
        {{
            if (!mat)
                return;
            {4}
        }}

        public override void UpdateMaterial(Material mat)
        {{
            if(!mat) 
                return;
            {2}
        }}

        public override void UpdateBlock(MaterialPropertyBlock block)
        {{
            if (block == null)
                return;
            {3}
        }}
    }}
}}
";
    }
}
#endif