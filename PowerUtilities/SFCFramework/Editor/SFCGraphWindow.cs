#if UNITY_EDITOR
using PowerUtilities.RenderFeatures;
using PowerUtilities.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace PowerUtilities
{
    /// <summary>
    /// SFC's graph view
    /// </summary>
    public class SFCGraphWindow : BaseUXMLEditorWindow,IUIElementEvent
    {
        static SRPFeatureListSO srpFeatureList;

        BaseGraphView graphView;

        [OnOpenAsset]
        static bool OnOpenSFC(int instanceId,int line)
        {
            var srpFeatureList = Selection.activeObject as SRPFeatureListSO;
            if (srpFeatureList != null)
            {
                OpenWindow();
            }
            return srpFeatureList;
        }

        public static void OpenWindow()
        {
            var win = GetWindow<SFCGraphWindow>();
            if (win.position.width < 1000)
                win.position = new Rect(100, 100, 1000, 800);
        }

        public SFCGraphWindow()
        {
            base.IsReplaceRootContainer = true;
        }

        private void OnSelectionChange()
        {
            if(Selection.activeObject is SRPFeatureListSO listSO)
            {
                ShowFeatureList(listSO);
            }
        }

        private void ShowFeatureList(SRPFeatureListSO listSO)
        {
            var infos = listSO.featureList.Select((feature, id) =>
            {
                //update y
                var pos = feature.nodeInfo.pos;
                if (pos.y == 0)
                {
                    pos.y = id * pos.height;
                    feature.nodeInfo.pos= pos;
                }
                feature.nodeInfo.title = feature.name;
                return feature.nodeInfo;
            });

            graphView.ShowNodes(infos.ToList());

            var edgeInfos = listSO.featureList
                .Select((feature, id) =>
            {
                if (id==0)
                    return default;
                return (id-1, id);
            }).Where(edgeInfo => edgeInfo != default);

            graphView.ShowEdges(edgeInfos.ToList());
        }

        public override void CreateGUI()
        {
            base.treeAsset = AssetDatabaseTools.FindAssetPathAndLoad<VisualTreeAsset>(out _, "SFCGraphWindow", "uxml");
            base.CreateGUI();
        }

        public void AddEvent(VisualElement root)
        {
            graphView = root.Q<BaseGraphView>();
            graphView.appendNodeList = new List<BaseGraphView.NodeViewInfo>
            {
                new BaseGraphView.NodeViewInfo {name = "SFCNode1",type = typeof(BaseNodeView)},
                new BaseGraphView.NodeViewInfo {name = "SFCNode2",type = typeof(BaseNodeView)}
            };
        }
    }
}
#endif