#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;
using System.Linq;
using System.IO;
using System;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace PowerUtilities.UIElements
{
    public class BaseGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<BaseGraphView, UxmlTraits> { };

        public class NodeViewInfo
        {
            /// <summary>
            /// Node 's name
            /// </summary>
            public string name;

            /// <summary>
            /// NodeView's nodeViewType,will auto create a instance
            /// </summary>
            public Type nodeViewType;

            /// <summary>
            /// copy info to nodeView
            /// 
            /// setup NodeView(inner GraphView) will call this
            /// </summary>
            public Action<BaseNodeView> onSetupView;
        }

        /// <summary>
        /// contextual menu show this list
        /// subClass override this list
        /// </summary>
        public List<NodeViewInfo> appendNodeList = new List<NodeViewInfo>()
        {
            new NodeViewInfo{name = "TestNode",nodeViewType = typeof(BaseNodeView) }
        };

        /// <summary>
        /// current nodeViews
        /// </summary>
        public List<BaseNodeView> nodeViewList = new List<BaseNodeView>();

        /// <summary>
        /// current selected nodeViewInfo id
        /// </summary>
        public int selectedNodeIndex = -1;

        /// <summary>
        /// when delete node will call
        /// </summary>
        public Action<GraphElement> onDeleteNode;

        public BaseGraphView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            LoadDefaultStyles();

            deleteSelection -= OnDeleteGraphNode;
            deleteSelection += OnDeleteGraphNode;
        }

        /// <summary>
        /// Default remove graph's node
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ask"></param>
        void OnDeleteGraphNode(string name,AskUser ask)
        {
            var nodes = selection.Select(item => item as GraphElement).ToList();

            for (int i = 0;i<nodes.Count;i++)
            {
                var item = nodes[i];
                onDeleteNode?.Invoke(item);
                RemoveElement(item);
            }
        }

        public virtual void LoadDefaultStyles()
        {
            var styleSheet = AssetDatabaseTools.FindAssetPathAndLoad<StyleSheet>(out _, "BaseGraphView", "uss");
            styleSheets.Add(styleSheet);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var mousePos = evt.localMousePosition;

            appendNodeList.ForEach(nodeViewInfo =>
            {
                evt.menu.AppendAction(nodeViewInfo.name,
                    (action) =>
                    {
                        var guiItem = (GraphElement)Activator.CreateInstance(nodeViewInfo.nodeViewType);
                        if (guiItem is BaseNodeView nodeView)
                        {
                            nodeViewInfo.onSetupView?.Invoke(nodeView);
                        }

                        SetupNodePosition( guiItem, mousePos);
                        AddElement(guiItem);
                    }
                );
            });

            //--------- local methods
            static void SetupNodePosition( GraphElement item,Vector2 pos)
            {
                var itemPos = item.GetPosition();
                itemPos.position = pos;

                item.SetPosition(itemPos);
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(p => p.direction != startPort.direction).ToList();
            //return base.GetCompatiblePorts(startPort, nodeAdapter);
        }

        public void ShowNodes(List<BaseNodeInfo> nodeInfoList,IMGUIContainer inspectorView)
        {
            nodeViewList.Clear();
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            // nodes
            nodeInfoList.ForEach((nodeInfo, id) =>
            {
                var nodeView = new BaseNodeView
                {
                    nodeId = id,
                    graphView = this,

                    title = nodeInfo.title,
                };
                nodeViewList.Add(nodeView);

                nodeView.SetPosition(nodeInfo.pos);
                AddElement(nodeView);
            });
        }

        public void ShowEdges(List<(int parentId,int childId)> edgeInfos)
        {
            edgeInfos.ForEach(edgeInfo =>
            {
                var edge = nodeViewList[edgeInfo.parentId].output.ConnectTo(nodeViewList[edgeInfo.childId].input);
                AddElement(edge);
            });
        }

        public virtual GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if(graphViewChange.elementsToRemove != null)
            {

            }

            if(graphViewChange.edgesToCreate != null)
            {

            }

            if(graphViewChange.movedElements != null)
            {

            }

            return graphViewChange;
        }

    }


}
#endif