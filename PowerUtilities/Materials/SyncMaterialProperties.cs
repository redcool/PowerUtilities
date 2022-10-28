using PowerUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(SyncMaterialProperties))]
public class SyncMaterialPropertiesEditor : PowerEditor<SyncMaterialProperties>
{
    string helpStr = "同步当前renderer的材质属性给其他的renderer";
    public override void DrawInspectorUI(SyncMaterialProperties inst)
    {

        EditorGUILayout.HelpBox(helpStr,MessageType.Info,true);
        DrawDefaultGUI();
    }
}
#endif

[ExecuteInEditMode]
public class SyncMaterialProperties : MonoBehaviour
{
    public Renderer[] otherRenderers; 
    Renderer render;
    static MaterialPropertyBlock block;
    // Start is called before the first frame update
    void Start()
    {
        render = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!render)
        {
            enabled=false;
            return;
        }

        if(block ==null)
            block = new MaterialPropertyBlock();

        render.GetPropertyBlock(block);

        foreach (var item in otherRenderers)
        {
            item.SetPropertyBlock(block);
        }
    }
}
