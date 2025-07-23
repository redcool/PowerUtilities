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
    /// Extends UnityEditor.Texture3D
    /// </summary>
    [CustomEditor(typeof(Texture3D))]
    [CanEditMultipleObjects]
    public class Texture3DInspectorEx : BaseEditorEx
    {

        public override string GetDefaultInspectorTypeName() => "UnityEditor.Texture3DInspector";

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
            //GUILayout.Button("test3d");
        }

    }
}
#endif