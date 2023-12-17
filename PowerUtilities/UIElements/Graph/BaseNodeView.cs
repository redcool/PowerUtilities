#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PowerUtilities.UIElements
{

    /// <summary>
    /// GraphView's node info
    /// </summary>
    [Serializable]
    public class BaseNodeInfo
    {
        public Rect pos = new Rect(0, 0, 200, 100);
        public string guid;
        public string title;
    }

    /// <summary>
    /// GraphView's nodeView
    /// </summary>
    public class BaseNodeView : Node
    {

        [Serializable]
        public class PortInfo
        {
            public Orientation orientation = Orientation.Vertical;
            public Port.Capacity capacity = Port.Capacity.Single;
            public Direction direction = Direction.Input;
            public Type portType = typeof(bool);
            public string portName;

            /// <summary>
            /// Style Sheet
            /// </summary>
            public FlexDirection flexDirection = FlexDirection.Column;
        }

        public static Lazy<string> lazyNodeViewUssPath = new Lazy<string>(() => AssetDatabaseTools.FindAssetPath("BaseNodeView", "uxml"));

        public PortInfo
            inputPortInfo = new PortInfo(),
            outputPortInfo = new PortInfo { direction = Direction.Output,flexDirection = FlexDirection.ColumnReverse}
        ;

        public Port input, output;


        /// <summary>
        /// node index
        /// </summary>
        public int nodeId;
        /// <summary>
        /// current GraphView
        /// </summary>
        public BaseGraphView graphView;

        public BaseNodeView() : base(lazyNodeViewUssPath.Value)
        {
            this.title = "test node";
            this.viewDataKey = GUID.Generate().ToString();

            CreatePortsAndAdd();

            SetupClasses();
        }

        private void SetupClasses()
        {
            AddToClassList("action");
        }



        /// <summary>
        /// create input and output port
        /// </summary>
        public void CreatePortsAndAdd()
        {   input = CreatePort(inputPortInfo);
            inputContainer.Add(true, input);
         

            output = CreatePort(outputPortInfo);
            outputContainer.Add(true, output);
        }

        public static Port CreatePort(string portName, Orientation orientation, Direction direction, Port.Capacity capacity, Type portType, FlexDirection flexDirection)
        {
            var port = Port.Create<Edge>(orientation, direction, capacity, portType);
            port.portName = portName;
            port.style.flexDirection = flexDirection;
            return port;
        }

        public static Port CreatePort(PortInfo info)
        {
            return CreatePort(info.portName, info.orientation, info.direction, info.capacity, info.portType, info.flexDirection);
        }

        /// <summary>
        /// GraphView select
        /// </summary>
        /// <exception cref="Exception"></exception>
        public override void OnSelected()
        {
            if (graphView == null)
                throw new Exception("Graph View is null,need set instance");

            graphView.selectedNodeIndex = nodeId;
        }
    }
}
#endif