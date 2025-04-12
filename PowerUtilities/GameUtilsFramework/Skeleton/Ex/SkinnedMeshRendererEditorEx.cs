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
        Type defaultEditorType;
        Editor defaultEditor;

        bool isShowSkeleton;
        private Vector2 scrollPosition;
        // diff with this
        GUIContent diffSkinnedGUI = new GUIContent("Diff Skinned", "sync show original skinned skeleton");
        SkinnedMeshRenderer diffSkinned;

        MethodInfo onSceneGUIMethod, onEnableMethod;

        public void OnEnable()
        {
            if (defaultEditorType == null)
                defaultEditorType = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.SkinnedMeshRendererEditor");
            if (defaultEditor == null)
            {
                defaultEditor = CreateEditor(targets, defaultEditorType);
            }

            if (onSceneGUIMethod == null)
                onSceneGUIMethod = defaultEditorType.GetMethod("OnSceneGUI", ReflectionTools.instanceBindings);
            if (onEnableMethod == null)
                onEnableMethod = defaultEditorType.GetMethod("OnEnable", ReflectionTools.instanceBindings);

            DelegateEx.GetOrCreate<Action>(defaultEditor, onEnableMethod).Invoke();
        }

        public override void OnInspectorGUI()
        {
            // show default
            defaultEditor.OnInspectorGUI();

            EditorGUITools.DrawColorLine(1);
            // show skeleton foldout
            isShowSkeleton = EditorGUILayout.BeginFoldoutHeaderGroup(isShowSkeleton, "Show Skeleton");
            if (isShowSkeleton)
            {
                // show diff
                diffSkinned = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(diffSkinnedGUI, diffSkinned, typeof(SkinnedMeshRenderer), true);

                // show cur skinned skeleton
                var skinned = target as SkinnedMeshRenderer;
                SkinnedMeshRendererInfoWin.InitBoneInfos(skinned, out var bonePaths, out var boneDepths);
                SkinnedMeshRendererInfoWin.DrawBoneInfos(skinned, bonePaths, boneDepths, ref scrollPosition, diffSkinned);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public void OnSceneGUI()
        {
            //onSceneGUIMethod.CreateDelegate(defaultEditor).DynamicInvoke();
            DelegateEx.GetOrCreate<Action>(defaultEditor, onSceneGUIMethod).Invoke();
        }
    }
}
#endif