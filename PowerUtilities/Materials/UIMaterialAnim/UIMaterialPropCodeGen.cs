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

    [MenuItem(MENU_ROOT+"/CreateUIMaterialCode")]
    static void Init()
    {
        var graphs = SelectionTools.GetSelectedComponents<MaskableGraphic>();
            
        foreach (var item in graphs)
        {
            var mat = item.material;
            Analysis(mat.shader);
        }
    }

    static void Analysis(Shader shader)
    {
        var fieldSb = new StringBuilder();
        var methodSb = new StringBuilder();

        var propCount = shader.GetPropertyCount();
        for (int i = 0; i < propCount; i++)
        {
            var propName = shader.GetPropertyName(i);
            var propType = shader.GetPropertyType(i);
            var propDefaultValue = GetDefaultValue(i, propType, shader);

            AnalysisProp(propType, propName,propDefaultValue, fieldSb, methodSb);
        }

        PathTools.CreateAbsFolderPath(PATH);

        var className = shader.name.Replace('/','_');
        var codeStr = string.Format(CodeTemplate.codeString, className, fieldSb.ToString(), methodSb.ToString());

        var path = $"{PathTools.GetAssetAbsPath(PATH)}/{className}.cs";
        File.WriteAllText(path, codeStr);

        propNameSet.Clear();
        AssetDatabase.Refresh();
    }

    public static string FormatVector(Vector4 v)
    {
        return $"({v.x}f,{v.y}f,{v.z}f,{v.w}f)";
    }

    public static string GetDefaultValue(int id, ShaderPropertyType type,Shader shader) => type switch
    {
        //ShaderPropertyType.Texture => $"Texture2D.{shader.GetPropertyTextureDefaultName(id)}Texture",
        ShaderPropertyType.Texture => $"null",
        ShaderPropertyType.Color => $"new Color{FormatVector(shader.GetPropertyDefaultVectorValue(id))}",
        ShaderPropertyType.Vector => $"new Vector4{FormatVector(shader.GetPropertyDefaultVectorValue(id))}",
        _ => $"{shader.GetPropertyDefaultFloatValue(id)}f"
    };

    public static void AnalysisProp(ShaderPropertyType type, string propName,string propValue, StringBuilder fieldSB, StringBuilder methodSB)
    {
        //Check repeatted propName
        if (propNameSet.Contains(propName))
        {
            return;
        }
        propNameSet.Add(propName);

        var varTypeName = GetVarableType(type);
        var setMethodName = GetSetMethodName(type);

        fieldSB.Append($"public {varTypeName} {propName} = {propValue};\n");
        methodSB.Append($"mat.Set{setMethodName}(\"{propName}\", {propName});\n");


    }

    public static string GetVarableType(ShaderPropertyType type) => type switch
    {
        ShaderPropertyType.Int => "int",
        ShaderPropertyType.Range => "float",
        ShaderPropertyType.Float => "float",
        ShaderPropertyType.Texture => "Texture2D",
        ShaderPropertyType.Vector => "Vector4",
        _ => Enum.GetName(typeof(ShaderPropertyType), type),
    };

    public static string GetSetMethodName(ShaderPropertyType type) => type switch
    {
        ShaderPropertyType.Range => "Float",
        _ => Enum.GetName(typeof(ShaderPropertyType), type)
    };

}

static class CodeTemplate
{
    public const string fieldStatement = @"public {0} {1}";
    public const string setMethodStatment = @"mat.Set{0}(""{1}"",{2})";
    public const string codeString = @"
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PowerUtilities
{{
    [ExecuteInEditMode]
    public class {0} : MonoBehaviour
    {{
        {1}

        Material mat;
        MaskableGraphic graph;

        private void Start()
        {{
            graph = GetComponent<MaskableGraphic>();
            if(graph)
                mat = graph.material;
        }}

        private void Update()
        {{
            if(!mat)
            {{
                enabled=false;
                return;
            }}

            UpdateMaterial();
            graph.SetMaterialDirty();
        }}

        void UpdateMaterial()
        {{
            {2}
        }}
    }}
}}
";
}
#endif