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
            //evt.menu.AppendAction("Add TestNode", (action) =>
            //{
            //    AddElement(new BaseNodeView());
            //});
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
    }
}
#endif