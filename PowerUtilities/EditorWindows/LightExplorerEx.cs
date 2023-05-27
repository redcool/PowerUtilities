#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// LightExplorer is urp's feature
    /// USE LightExplorerEx need use UniversalRenderPipelineAssetEx
    /// 
    /// 1 change UniversalRenderPipelineAsset's script to UniversalRenderPipelineAssetEx
    /// </summary>
    [LightingExplorerExtension(typeof(UniversalRenderPipelineAssetEx))]
    public class LightExplorerEx : LightExplorer
    {

        static class Styles
        {
            public static readonly GUIContent BakeryLight = new GUIContent(nameof(BakeryLight));
        }


        public bool SyncLightInfo(Light light)
        {
            if (!light)
                return false;

            return BakeryEditorTools.SyncBakeryLight(light);
        }

        public override LightingExplorerTab[] GetContentTabs()
        {
            return new List<LightingExplorerTab>(base.GetContentTabs())
            {

            }.ToArray();
        }

        protected override LightingExplorerTableColumn[] GetLightColumns()
        {
            return new List<LightingExplorerTableColumn>(base.GetLightColumns())
            { 
                new LightingExplorerTableColumn(
                    LightingExplorerTableColumn.DataType.Custom
                    ,Styles.BakeryLight
                    ,"m_Intensity"
                    ,onGUIDelegate:DrawBakeryLight
                )
            }.ToArray();
        }

        public void DrawBakeryLight(Rect rect, SerializedProperty prop, SerializedProperty[] dependencies)
        {
            if (prop == null)
                return;

            var pos = new Rect(rect.x, rect.y, 50, 18);
            var light = prop.serializedObject.targetObject as Light;

            var isSynced = SyncLightInfo(light);
            // status
            EditorGUI.LabelField(pos, isSynced ? "sync" : "no");
            // buttons
            pos.x += 30;
            if (!isSynced)
            {
                if (GUI.Button(pos, "Add"))
                    BakeryEditorTools.AddBakeryLight(light);
            }
            else
            {
                if (GUI.Button(pos, "Remove"))
                    BakeryEditorTools.RemoveBakeryLight(light);
            }

        }
    }
}
#endif