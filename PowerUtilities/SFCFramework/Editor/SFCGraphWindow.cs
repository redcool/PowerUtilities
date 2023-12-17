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
    public class SFCGraphWindow : BaseUXMLEditorWindow, IUIElementEvent
    {
        static SRPFeatureListSO srpFeatureList;

        BaseGraphView graphView;
        IMGUIContainer inspectorView;

        Editor featureSOEditor;
        IEnumerable<Type> srpFeatureTypes;

        [OnOpenAsset]
        static bool OnOpenSFC(int instanceId, int line)
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
            ShowSelectedFeatureListSO();
        }

        private void ShowSelectedFeatureListSO()
        {
            if (Selection.activeObject is SRPFeatureListSO featurelistSO)
            {
                srpFeatureList = featurelistSO;
                ShowFeatureList(featurelistSO);
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

            graphView.ShowNodes(infos.ToList(), inspectorView);

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
            srpFeatureTypes = ReflectionTools.GetTypesDerivedFrom<SRPFeature>();

            base.treeAsset = AssetDatabaseTools.FindAssetPathAndLoad<VisualTreeAsset>(out _, "SFCGraphWindow", "uxml");
            base.CreateGUI();
            ShowSelectedFeatureListSO();
        }

        public void AddEvent(VisualElement root)
        {
            graphView = root.Q<BaseGraphView>();
            //graphView.appendNodeList = new List<BaseGraphView.NodeViewInfo>
            //{
            //    new BaseGraphView.NodeViewInfo {name = "SFCNode1",nodeViewType = typeof(BaseNodeView)},
            //    new BaseGraphView.NodeViewInfo {name = "SFCNode2",nodeViewType = typeof(BaseNodeView)}
            //};

            graphView.appendNodeList = srpFeatureTypes.Select(sfcFeatureType =>
                new BaseGraphView.NodeViewInfo
                {
                    name = sfcFeatureType.Name,
                    nodeViewType = typeof(BaseNodeView),
                    onSetupView = (nodeView) =>
                    {
                        nodeView.graphView = graphView;
                        nodeView.title = sfcFeatureType.Name;
                    }
                }
            ).ToList();


            inspectorView = root.Q<IMGUIContainer>("Inspector");
            inspectorView.onGUIHandler = OnShowInspector;
        }

        void OnShowInspector()
        {
            if (!srpFeatureList || graphView.selectedNodeIndex == -1)
            {
                return;
            }
            var featureSO = srpFeatureList.featureList[graphView.selectedNodeIndex];
            if(featureSOEditor == null || featureSOEditor.target != featureSO)
            {
                featureSOEditor = Editor.CreateEditor(featureSO);
            }

            featureSOEditor.OnInspectorGUI();
        }
    }
}
#endif