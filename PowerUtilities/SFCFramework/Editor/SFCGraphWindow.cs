#if UNITY_EDITOR
using PowerUtilities.RenderFeatures;
using PowerUtilities.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace PowerUtilities
{
    /// <summary>
    /// SFC's graph view
    /// </summary>
    public class SFCGraphWindow : BaseUXMLEditorWindow, IUIElementEvent
    {
        /// <summary>
        /// current selected srf feature list
        /// </summary>
        static SRPFeatureListSO srpFeatureListSO, lastSrpFeatureListSO;

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
                srpFeatureListSO = featurelistSO;
                ShowFeatureList(featurelistSO);
            }
        }

        private void ShowFeatureList(SRPFeatureListSO listSO)
        {
            var featureList = listSO.featureList.Where(item => item);

            var nodeInfos = featureList.Select((feature, id) =>
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

            graphView.ShowNodes(nodeInfos.ToList(), inspectorView);

            var edgeInfos = featureList.Select((feature, id) =>
            {
                if (id==0)
                    return default;
                return (id-1, id);
            }).Where(edgeInfo => edgeInfo != default);

            graphView.ShowEdges(edgeInfos.ToList());


            graphView.graphViewChanged -= OnGraphViewChanged;
            graphView.graphViewChanged += OnGraphViewChanged;

            graphView.onDeleteNode += OnDeleteNode;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.movedElements != null)
            {
                OnMoved(graphViewChange);
            }
            if (graphViewChange.elementsToRemove != null)
            {
                //OnRemoved(graphViewChange);
            }
            if (graphViewChange.edgesToCreate != null)
            {
                OnCreateEdges(graphViewChange.edgesToCreate);
            }
            return graphViewChange;

            //-------------- local functions
            static void OnMoved(GraphViewChange graphViewChange)
            {
                foreach (var item in graphViewChange.movedElements)
                {
                    if (item is BaseNodeView nodeView)
                    {
                        var feature = srpFeatureListSO.featureList[nodeView.nodeId];
                        feature.nodeInfo.pos.position += graphViewChange.moveDelta;
                    }
                }
            }

            //static void OnRemoved(GraphViewChange graphViewChange)
            //{
            //    foreach (var item in graphViewChange.elementsToRemove)
            //    {
            //        RemoveSFCPass(item);
            //    }
            //}

            void OnCreateEdges(List<Edge> edgesToCreate)
            {
                foreach (var edge in edgesToCreate)
                {
                    if (edge.output.node is BaseNodeView node1)
                    {
                        var node2 = edge.input.node as BaseNodeView;
                        var targetId = node1.nodeId + 1;

                        var insertFeature = srpFeatureListSO.featureList[node2.nodeId];
                        srpFeatureListSO.featureList.Remove(insertFeature);
                        graphView.nodeViewList.Remove(node2);

                        if (targetId < srpFeatureListSO.featureList.Count)
                        {
                            srpFeatureListSO.featureList.Insert(targetId, insertFeature);
                            graphView.nodeViewList.Insert(targetId, node2);
                        }
                        else
                        {
                            srpFeatureListSO.featureList.Add(insertFeature);
                            graphView.nodeViewList.Add(node2);
                        }

                        // update nodeViews's nodeId
                        for (int i = 0; i< graphView.nodeViewList.Count; i++)
                        {
                            graphView.nodeViewList[i].nodeId = i;
                        }
                    }
                }
            }
        }

        private void OnDeleteNode(GraphElement item)
        {
            if (item is BaseNodeView nodeView)
            {
                if (nodeView.nodeId >= srpFeatureListSO.featureList.Count)
                    return;

                var feature = srpFeatureListSO.featureList[nodeView.nodeId];
                srpFeatureListSO.featureList.Remove(feature);
                //feature.Destroy(true);
            }
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

                        SRPFeatureListEditor.CreateSFCPassAsset(sfcFeatureType, srpFeatureListSO);
                    }
                }
            ).ToList();


            inspectorView = root.Q<IMGUIContainer>("Inspector");
            inspectorView.onGUIHandler = OnShowInspector;
        }

        void OnShowInspector()
        {
            if (!srpFeatureListSO || graphView.selectedNodeIndex < 0 || graphView.selectedNodeIndex >= srpFeatureListSO.featureList.Count)
            {
                return;
            }
            var featureSO = srpFeatureListSO.featureList[graphView.selectedNodeIndex];
            if(featureSOEditor == null || featureSOEditor.target != featureSO)
            {
                featureSOEditor = Editor.CreateEditor(featureSO);
            }

            featureSOEditor.OnInspectorGUI();
        }
    }
}
#endif