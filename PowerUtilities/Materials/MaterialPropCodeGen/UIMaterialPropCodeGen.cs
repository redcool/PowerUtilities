#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PowerUtilities;
using UnityEngine.UI;
using System.Text;
using UnityEngine.Rendering;
using System.IO;
using System.Linq;
using System;

public class UIMaterialPropCodeGen
{
    const string MENU_ROOT = "PowerUtilities/CodeGenerator";
    const string PATH = "Assets/CodeGens/";

    static HashSet<string> propNameSet = new HashSet<string>();

    [MenuItem(MENU_ROOT+"/CreateUIMaterialCode(from hierarchy ui)")]
    static void Init()
    {
        var graphs = SelectionTools.GetSelectedComponents<MaskableGraphic>();

        foreach (var item in graphs)
        {
            var mat = item.material;
            Analysis(mat.shader);
        }
    }

    [MenuItem(MENU_ROOT+"/CreateUIMaterialCode(from Asset Shader)")]
    static void Init2()
    {
        var shader = Selection.GetFiltered<Shader>(SelectionMode.Assets);

        foreach (var item in shader)
        {
            Analysis(item);
        }
    }

    static void Analysis(Shader shader)
    {
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

            AnalysisProp(propFlags, propType, propName, propDefaultValue,
                fieldSb, updateMatMethodSb, updateBlockMethodSb, readMatSb);
        }

        PathTools.CreateAbsFolderPath(PATH);

        var className = shader.name.Replace('/', '_');
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
    }

    private static bool IsValidFlags(ShaderPropertyFlags flags)
    {
        return (ShaderPropertyFlags.HideInInspector & flags) == 0;
    }

    public static string FormatVector(Vector4 v)
    {
        return $"({v.x}f,{v.y}f,{v.z}f,{v.w}f)";
    }

    public static string GetDefaultValue(int id, ShaderPropertyType type, Shader shader) => type switch
    {
        //ShaderPropertyType.Texture => $"Texture2D.{shader.GetPropertyTextureDefaultName(id)}Texture",
        ShaderPropertyType.Texture => $"null",
        ShaderPropertyType.Color => $"new Color{FormatVector(shader.GetPropertyDefaultVectorValue(id))}",
        ShaderPropertyType.Vector => $"new Vector4{FormatVector(shader.GetPropertyDefaultVectorValue(id))}",
        _ => $"{shader.GetPropertyDefaultFloatValue(id)}f"
    };

    public static void AnalysisProp(ShaderPropertyFlags flags, ShaderPropertyType type, string propName, string propValue,
        StringBuilder fieldSb,
        StringBuilder updateMatSb,
        StringBuilder updateBlockSB,
        StringBuilder readMatSb
        )
    {
        //Check repeatted propName
        if (propNameSet.Contains(propName))
        {
            return;
        }
        propNameSet.Add(propName);

        var varTypeName = GetVarableType(type);
        var setMethodName = GetSetMethodName(type);

        var varDecorator = GetVarDecorator(propName, flags);
        //demo : [ColorUsage(true,true)] public Color color = shaderDefaultValue
        fieldSb.AppendLine($"{varDecorator} public {varTypeName} {propName} = {propValue};");

        var textureCondition = "";
        if (type == ShaderPropertyType.Texture)
        {
            textureCondition = $"if({propName}!=null)";
        }

        //demo : mat.SetColor("_Color",color);
        updateMatSb.AppendLine($"{textureCondition} mat.Set{setMethodName}(\"{propName}\", {propName});");
        updateBlockSB.AppendLine($"{textureCondition} block.Set{setMethodName}(\"{propName}\", {propName});");

        // demo: color = mat.GetColor("_Color");
        readMatSb.AppendLine($"{propName} = mat.Get{setMethodName}(\"{propName}\");");
    }

    private static string GetVarDecorator(string propName, ShaderPropertyFlags flags)
    {
        var sb = new StringBuilder();
        if ((flags & ShaderPropertyFlags.HDR) > 0)
        {
            sb.Append("[ColorUsage(true,true)]");
        }

        return sb.ToString();
    }

    public static string GetVarableType(ShaderPropertyType type) => type switch
    {
#if UNITY_2021
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
    [ExecuteInEditMode]
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
#endif