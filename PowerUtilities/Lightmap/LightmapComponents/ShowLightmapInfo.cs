namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(ShowLightmapInfo))]
    public class ShowLightmapInfoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var inst = target as ShowLightmapInfo;
            var r = inst.GetComponent<MeshRenderer>();

            EditorGUILayout.LabelField($"lightmapIdex: {r.lightmapIndex}");
            EditorGUILayout.LabelField($"lightmap uv offset: { r.lightmapScaleOffset}");
        }
    }
#endif

    public class ShowLightmapInfo : MonoBehaviour
    {

    }
}