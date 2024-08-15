#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public static class LightmapSettingTools
    {

        public static bool FindProperty(MeshRenderer mr, string propName, out SerializedObject so, out SerializedProperty sp)
        {
            so = null;
            sp = null;
            if (mr)
            {
                so = new SerializedObject(mr);
                sp = so.FindProperty(propName);
                return true;
            }
            return false;
        }
        public static void SetProperty(MeshRenderer mr, string propName, Action<SerializedObject, SerializedProperty> action)
        {
            if (!mr)
                return;

            SerializedObject so;
            SerializedProperty sp;
            if (FindProperty(mr, propName, out so, out sp))
                action(so, sp);
        }

        public static float GetScaleInLightmap(this MeshRenderer mr)
        {
            if (!mr)
                return 1;

            SerializedObject so;
            SerializedProperty sp;
            if (FindProperty(mr, "m_ScaleInLightmap", out so, out sp))
                return sp.floatValue;
            return 1;
        }

        public static Rect GetLightmapRect(this Renderer r)
        {
            if (!r)
                return Rect.zero;


            var v4 = r.lightmapScaleOffset;
            return new Rect(v4.z, v4.w, v4.x , v4.y );
        }

        public static void SetLightmapStatic(this MeshRenderer mr)
        {
            if (!mr)
                return;

            EditorTools.AddStaticFlags(mr.gameObject, StaticEditorFlags.ContributeGI);
        }

        public static void SetScaleInLightmap(this MeshRenderer mr, float value, bool setLightmapStatic = true)
        {
            if (!mr)
                return;

            if (setLightmapStatic)
                SetLightmapStatic(mr);

            SerializedObject so;
            SerializedProperty sp;
            if (FindProperty(mr, "m_ScaleInLightmap", out so, out sp))
                sp.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetStitchSeam(this MeshRenderer mr, bool value)
        {
            if (!mr)
                return;

            SerializedObject so;
            SerializedProperty sp;
            if (FindProperty(mr, "m_StitchLightmapSeams", out so, out sp))
                sp.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static bool GetStitchSeam(this MeshRenderer mr)
        {
            if (!mr)
                return false;

            SerializedObject so;
            SerializedProperty sp;
            if (FindProperty(mr, "m_StitchLightmapSeams", out so, out sp))
                return sp.boolValue;
            return false;
        }
    }
}
#endif