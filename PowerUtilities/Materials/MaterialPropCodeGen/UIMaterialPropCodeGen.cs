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
            var propName = shader.GetPropertyName(i);
            var propType = shader.GetPropertyType(i);
            var propDefaultValue = GetDefaultValue(i, propType, shader);

            AnalysisProp(propType, propName,propDefaultValue,
                fieldSb, updateMatMethodSb, updateBlockMethodSb,readMatSb);
        }

        PathTools.CreateAbsFolderPath(PATH);

        var className = shader.name.Replace('/','_');
        var codeStr = string.Format(CodeTemplate.codeString,
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

    public static void AnalysisProp(ShaderPropertyType type, string propName, string propValue,
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

        //demo : public Color color = shaderDefaultValue
        fieldSb.Append($"public {varTypeName} {propName} = {propValue};\n");

        var textureCondition = "";
        if (type == ShaderPropertyType.Texture)
        {
            textureCondition = $"if({propName}!=null)";
        }

        //demo : mat.SetColor("_Color",color);
        updateMatSb.Append($"{textureCondition} mat.Set{setMethodName}(\"{propName}\", {propName});\n");
        updateBlockSB.Append($"{textureCondition} block.Set{setMethodName}(\"{propName}\", {propName});\n");

        // demo: color = mat.GetColor("_Color");
        readMatSb.Append($"{propName} = mat.Get{setMethodName}(\"{propName}\");\n");
    }

    public static string GetVarableType(ShaderPropertyType type) => type switch
    {
        ShaderPropertyType.Int => "int",
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

}

static class CodeTemplate
{
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
        // props
        {1}

        [Header(""UI Graphs"")]
        public Graphic[] graphs;
        public bool useGraphMaterialInstance;

        [Header(""Renderers"")]
        public Renderer[] renderers;
        
        static MaterialPropertyBlock rendererBlock;

        private void Start()
        {{
            if (graphs == null || graphs.Length == 0)
            {{
                if (gameObject.TryGetComponent<Graphic>(out var comp))
                    graphs = new []{{comp}};
            }}

            if(renderers == null || renderers.Length == 0)
            {{
                if (gameObject.TryGetComponent<Renderer>(out var comp))
                    renderers = new []{{comp}};
            }}

            var isRenderersValid = (renderers != null && renderers.Length>0);
            var isGraphsValid = (graphs != null && graphs.Length>0);
            enabled = isRenderersValid || isGraphsValid;

            if (useGraphMaterialInstance)
                graphs.ForEach(graph => graph.material = Instantiate(graph.material));

            if (enabled)
            {{
                var firstMat = isRenderersValid ? renderers[0].sharedMaterial : graphs[0].material;
                if (firstMat)
                    ReadFirstMaterial(firstMat);
            }}
        }}

        private void Update()
        {{
            if(rendererBlock == null)
                rendererBlock= new MaterialPropertyBlock();

            MaterialPropCodeGenTools.UpdateComponentsMaterial(graphs, (graph,id) => {{
                UpdateMaterial(graph.material);
                graph.SetMaterialDirty();
            }});
            MaterialPropCodeGenTools.UpdateComponentsMaterial(renderers, (render,id) => {{
                rendererBlock.Clear();
                render.GetPropertyBlock(rendererBlock);
                UpdateBlock(rendererBlock);
                render.SetPropertyBlock(rendererBlock);
            }});
        }}

        private void OnDestroy()
        {{
            rendererBlock =null;
        }}
        
        private void Reset()
        {{
            if(Application.isEditor)
                Start();
        }}

        void ReadFirstMaterial(Material mat)
        {{
            {4}
        }}

        void UpdateMaterial(Material mat)
        {{
            if(!mat) 
                return;
            {2}
        }}

        void UpdateBlock(MaterialPropertyBlock block)
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