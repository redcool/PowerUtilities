#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PowerUtilities.UIElements
{
    public class TestNodeView : Node
    {
        [Serializable]
        public class PortInfo
        {
            public Orientation orientation = Orientation.Vertical;
            public Port.Capacity capacity = Port.Capacity.Single;
            public Type portType = typeof(bool);
            public string portName;
        }

        public static Lazy<string> lazyNodeViewUssPath = new Lazy<string>(()=> AssetDatabaseTools.FindAssetPath("TestNodeView","uxml"));

        public PortInfo
            inputPortInfo = new PortInfo(),
            outputPortInfo = new PortInfo()
        ;

        public Port input, output;
        const string PATH = "Assets/PowerXXX/PowerUtilities/PowerUtilities/UIElements/Test/testUIElements/TestNodeView.uxml";
        public TestNodeView() : base(lazyNodeViewUssPath.Value)
        {
            this.title = "test node";
            this.viewDataKey = GUID.Generate().ToString();

            CreatePorts();

            SetupClasses();
        }

        private void SetupClasses()
        {
            AddToClassList("action");
        }

        /// <summary>
        /// create input and output port
        /// </summary>
        public void CreatePorts()
        {
            input = InstantiatePort(inputPortInfo.orientation, Direction.Input, inputPortInfo.capacity, inputPortInfo.portType);
            inputContainer.Clear();
            inputContainer.Add(input);
            input.portName = inputPortInfo.portName;
            input.style.flexDirection = FlexDirection.Column;

            output = InstantiatePort(outputPortInfo.orientation, Direction.Output, outputPortInfo.capacity, outputPortInfo.portType);
            outputContainer.Clear();
            outputContainer.Add(output);
            output.portName = outputPortInfo.portName;
            output.style.flexDirection = FlexDirection.ColumnReverse;
        }
    }
}
#endif