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
    /// <summary>
    /// Extends Unity SkinnedMeshRenderer Editor
    /// </summary>
    [CustomEditor(typeof(SkinnedMeshRenderer))]
    [CanEditMultipleObjects]
    public class SkinnedMeshRendererEditorEx : BaseEditorEx
    {
        //==================
        //SkinnedMeshRenderer
        //==================
        bool isShowSkeleton;
        bool isShowWeights;
        private Vector2 scrollPosition;
        // diff with this
        GUIContent diffSkinnedGUI = new GUIContent("Diff Skinned", "sync show original skinned skeleton");
        SkinnedMeshRenderer diffSkinned;

        MethodInfo onSceneGUI;

        //==================
        //SortingLayerEditorUtility
        //==================
        // 
        public Type sortingLayerEditorUtilityType;
        MethodInfo renderSortingLayerFields;

        public bool isShowSortingLayerSetting;
        GUIContent sortingLayerSettingGUI = new GUIContent("SortingLayer", "SortingLayer,order");

        public override string GetDefaultInspectorTypeName() => "UnityEditor.SkinnedMeshRendererEditor";

        public override void OnEnable()
        {
            base.OnEnable();

            defaultEditorType.GetMethod(ref onSceneGUI, name);

            // get SortingLayerEditorUtility
            RendererEditorTools.GetSortingLayerEditorUtility(ref sortingLayerEditorUtilityType, ref renderSortingLayerFields);
        }



        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

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
                SkinnedMeshRendererInfoWin.DrawBoneWeigth1Info(skinned, 0, 1000);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public void OnSceneGUI()
        {
            defaultEditor.InvokeDelegate<Action>(onSceneGUI);
        }


    }
}
#endif