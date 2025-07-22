#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// UnityEditor.Texture2DArrayInspector extends
    /// </summary>
    [CustomEditor(typeof(Texture2DArray))]
    [CanEditMultipleObjects]
    public class Texture2DArrayInspectorEx : BaseEditorEx
    {

        public override string GetDefaultInspectorTypeName() => "UnityEditor.Texture2DArrayInspector";

        public override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // draw 
            GUILayout.Button("test");
        }

    }
}
#endif