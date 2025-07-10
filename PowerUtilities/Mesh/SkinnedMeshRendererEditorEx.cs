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
        //==================
        //SkinnedMeshRenderer
        //==================
        Type skinnedMeshRendererEditorType;
        Editor skinnedMeshEditor;

        bool isShowSkeleton;
        bool isShowWeights;
        private Vector2 scrollPosition;
        // diff with this
        GUIContent diffSkinnedGUI = new GUIContent("Diff Skinned", "sync show original skinned skeleton");
        SkinnedMeshRenderer diffSkinned;

        MethodInfo onSceneGUIMethod, onEnableMethod;

        //==================
        //SortingLayerEditorUtility
        //==================
        // 
        public Type sortingLayerEditorUtilityType;
        MethodInfo renderSortingLayerFields;

        public bool isShowSortingLayerSetting;
        GUIContent sortingLayerSettingGUI = new GUIContent("SortingLayer", "SortingLayer,order");

        public void OnEnable()
        {
            EditorTools.GetOrCreateUnityEditor(ref skinnedMeshEditor, targets, ref skinnedMeshRendererEditorType, "UnityEditor.SkinnedMeshRendererEditor");

            skinnedMeshRendererEditorType.GetMethod(ref onEnableMethod, nameof(OnEnable), ReflectionTools.instanceBindings);
            skinnedMeshRendererEditorType.GetMethod(ref onSceneGUIMethod, nameof(OnSceneGUI), ReflectionTools.instanceBindings);

            DelegateEx.GetOrCreate<Action>(skinnedMeshEditor, onEnableMethod).Invoke();

            // get SortingLayerEditorUtility
            RendererEditorTools.GetSortingLayerEditorUtility(ref sortingLayerEditorUtilityType, ref renderSortingLayerFields);
        }



        public override void OnInspectorGUI()
        {
            // show default
            skinnedMeshEditor.OnInspectorGUI();

            EditorGUITools.DrawColorLine(1);

            serializedObject.Update();
            RendererEditorTools.DrawRenderSortingLayerFields(serializedObject, renderSortingLayerFields, ref isShowSortingLayerSetting, sortingLayerSettingGUI);

            serializedObject.ApplyModifiedProperties();
            DrawSkinnedBoneInfoGUI();
        }

        private void DrawSkinnedBoneInfoGUI()
        {
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