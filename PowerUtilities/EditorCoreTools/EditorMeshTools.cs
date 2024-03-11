#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public static class EditorMeshTools
    {
        public static void SetIsReadable(this Mesh mesh, bool isReadable)
        {
            var so = new SerializedObject(mesh);
            so.Update();
            var prop = so.FindProperty("m_IsReadable");
            prop.boolValue = isReadable;
            so.ApplyModifiedProperties();
        }
    }
}
#endif