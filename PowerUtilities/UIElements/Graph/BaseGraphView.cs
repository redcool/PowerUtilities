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
            public string name;
            public Type type;
        }

        /// <summary>
        /// contextual menu show this list
        /// subClass override this list
        /// </summary>
        public List<NodeViewInfo> appendNodeList = new List<NodeViewInfo>()
        {
            new NodeViewInfo{name = "TestNode",type = typeof(BaseNodeView) }
        };

        public List<BaseNodeView> nodeViewList = new List<BaseNodeView>();

        public BaseGraphView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            LoadDefaultStyles();
        }

        public virtual void LoadDefaultStyles()
        {
            var styleSheet = AssetDatabaseTools.FindAssetPathAndLoad<StyleSheet>(out _, "BaseGraphView", "uss");
            styleSheets.Add(styleSheet);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            appendNodeList.ForEach(node =>
            {
                evt.menu.AppendAction(node.name,
                    (action) => AddElement((GraphElement)Activator.CreateInstance(node.type))
                );
            });
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(p => p.direction != startPort.direction).ToList();
            //return base.GetCompatiblePorts(startPort, nodeAdapter);
        }

        public void ShowNodes(List<BaseNodeInfo> nodeInfoList)
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

        public GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
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