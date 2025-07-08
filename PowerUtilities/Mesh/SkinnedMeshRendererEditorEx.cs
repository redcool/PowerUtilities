#if UNITY_EDITOR
using GameUtilsFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

namespace PowerUtilities
{
    [CustomEditor(typeof(SkinnedMeshRenderer))]
    [CanEditMultipleObjects]
    public class SkinnedMeshRendererEditorEx : Editor
    {
        Type skinnedMeshRendererEditorType;
        Editor skinnedMeshEditor;

        bool isShowSkeleton;
        bool isShowWeights;
        private Vector2 scrollPosition;
        // diff with this
        GUIContent diffSkinnedGUI = new GUIContent("Diff Skinned", "sync show original skinned skeleton");
        SkinnedMeshRenderer diffSkinned;

        MethodInfo onSceneGUIMethod, onEnableMethod;

        public void OnEnable()
        {
            EditorTools.GetOrCreateUnityEditor(ref skinnedMeshEditor, targets, ref skinnedMeshRendererEditorType, "UnityEditor.SkinnedMeshRendererEditor");

            skinnedMeshRendererEditorType.GetMethod(ref onEnableMethod, nameof(OnEnable), ReflectionTools.instanceBindings);
            skinnedMeshRendererEditorType.GetMethod(ref onSceneGUIMethod, nameof(OnSceneGUI), ReflectionTools.instanceBindings);

            DelegateEx.GetOrCreate<Action>(skinnedMeshEditor, onEnableMethod).Invoke();
        }



        public override void OnInspectorGUI()
        {
            // show default
            skinnedMeshEditor.OnInspectorGUI();

            DrawInspectorGUI();
        }

        private void DrawInspectorGUI()
        {
            EditorGUITools.DrawColorLine(1);
            var skinned = target as SkinnedMeshRenderer;
            // show skeleton foldout
            isShowSkeleton = EditorGUILayout.BeginFoldoutHeaderGroup(isShowSkeleton, "Show Skeleton");
            if (isShowSkeleton)
            {
                // show diff
                diffSkinned = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(diffSkinnedGUI, diffSkinned, typeof(SkinnedMeshRenderer), true);

                // show cur skinned skeleton
                SkinnedMeshRendererInfoWin.InitBoneInfos(skinned, out var bonePaths, out var boneDepths);
                SkinnedMeshRendererInfoWin.DrawBoneInfos(skinned, bonePaths, boneDepths, ref scrollPosition, diffSkinned);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            //------ show bone weight
            isShowWeights = EditorGUILayout.BeginFoldoutHeaderGroup(isShowWeights, "Show BoneWeights");
            if (isShowWeights)
            {
                SkinnedMeshRendererInfoWin.DrawBoneWeigth1Info(skinned);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public void OnSceneGUI()
        {
            //onSceneGUIMethod.CreateDelegate(defaultEditor).DynamicInvoke();
            DelegateEx.GetOrCreate<Action>(skinnedMeshEditor, onSceneGUIMethod).Invoke();
        }
    }
}
#endif