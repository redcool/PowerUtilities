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
        Editor defaultEditor;

        bool isShowSkeleton;
        private Vector2 scrollPosition;
        private Dictionary<(object inst, string methodName), Delegate> methodNameDelegateDict = new();

        public void OnEnable()
        {
            if (defaultEditor == null)
            {
                var defaultEditorType = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.SkinnedMeshRendererEditor");
                defaultEditor = CreateEditor(targets, defaultEditorType);
            }
            InvokeDefaultEditorMethod(nameof(OnEnable));
        }

        public override void OnInspectorGUI()
        {
            // show default
            defaultEditor.OnInspectorGUI();

            EditorGUITools.DrawColorLine(1);
            // show skeleton
            isShowSkeleton = EditorGUILayout.BeginFoldoutHeaderGroup(isShowSkeleton, "Show Skeleton");
            if (isShowSkeleton)
            {
                var skinned = target as SkinnedMeshRenderer;
                SkinnedMeshRendererInfoWin.InitBoneInfos(skinned, out var bonePaths, out var boneDepths);
                SkinnedMeshRendererInfoWin.DrawBoneInfos(skinned, bonePaths, boneDepths, ref scrollPosition);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }


        public void InvokeDefaultEditorMethod(string methodName)
        {
            //defaultEditor.GetType().InvokeMethod(methodName, Type.EmptyTypes, defaultEditor, default);

            //defaultEditor.GetType().GetDelegate(defaultEditor,methodName, ReflectionTools.instanceBindings, Type.EmptyTypes).DynamicInvoke();
            DelegateEx.GetOrCreate<Action>(
                defaultEditor, 
                defaultEditor.GetType().GetMethod(methodName, ReflectionTools.instanceBindings)
                )?.DynamicInvoke();
        }

        public void OnSceneGUI()
        {
            InvokeDefaultEditorMethod(nameof(OnSceneGUI));
        }
    }
}
#endif