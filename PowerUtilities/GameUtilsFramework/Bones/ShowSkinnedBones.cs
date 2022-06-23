namespace GameUtilsFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(ShowSkinnedBones))]
    public class ShowSkinnedBonesEditor : Editor
    {
        private Vector2 scrollPosition;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var inst = (ShowSkinnedBones)target;
            if (!inst.skinnedMeshRenderer)
            {
                inst.skinnedMeshRenderer = inst.GetComponent<SkinnedMeshRenderer>();

                if (!inst.skinnedMeshRenderer)
                    return;
            }
            EditorGUILayout.LabelField(inst.skinnedMeshRenderer.name + " bones :");

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var item in inst.skinnedMeshRenderer.bones)
            {
                EditorGUILayout.ObjectField(item, typeof(Transform), true);
            }

            EditorGUILayout.EndScrollView();
        }
    }
#endif

    public class ShowSkinnedBones : MonoBehaviour
    {
        public SkinnedMeshRenderer skinnedMeshRenderer;
    }
}