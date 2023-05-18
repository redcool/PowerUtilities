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
            public static readonly GUIContent BakerySync = new GUIContent(nameof(BakerySync));
        }


        public bool SyncLightInfo(Light light)
        {
            if (!light)
                return false;

#if BAKERY_INCLUDED
            return BakeryEditorTools.SyncBakeryLight(light);
#else
            return false;
#endif
        }

        protected override LightingExplorerTableColumn[] GetLightColumns()
        {
            var columns = new List<LightingExplorerTableColumn>(base.GetLightColumns()) { 
                new LightingExplorerTableColumn(LightingExplorerTableColumn.DataType.Custom,Styles.BakerySync,"m_Intensity"
                ,onGUIDelegate:(rect,prop,propDeps)=>{
                    if(prop == null)
                        return;
                    var light = prop.serializedObject.targetObject as Light;

                    var isSynced = SyncLightInfo(light);
                    EditorGUI.LabelField(rect, isSynced? "sync" : "no");
                }
                )
            };

            return columns.ToArray();
        }


    }
}
#endif