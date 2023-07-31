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

namespace PowerUtilities.UIElements
{
    public class BaseGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<BaseGraphView, UxmlTraits> { };

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
            evt.menu.AppendAction("Add TestNode", (action) =>
            {
                AddElement(new BaseNodeView());
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