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
            if (!inst.skinned)
            {
                inst.skinned = inst.GetComponent<SkinnedMeshRenderer>();

                if (!inst.skinned)
                {
                    EditorGUILayout.LabelField("SkinnedMeshRenderer not found.");
                    return;
                }
            }

            SkinnedMeshRendererInfoWin.InitBoneInfos(inst.skinned,out var bonePaths,out var boneDepths);
            SkinnedMeshRendererInfoWin.DrawBoneInfos(inst.skinned,bonePaths,boneDepths,ref scrollPosition);
        }
    }
#endif
    [AddComponentMenu("")]
    public class ShowSkinnedBones : MonoBehaviour
    {
        public SkinnedMeshRenderer skinned;
    }
}