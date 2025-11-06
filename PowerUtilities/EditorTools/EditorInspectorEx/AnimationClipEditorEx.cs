#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static PowerUtilities.StringEx;

namespace PowerUtilities
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AnimationClip))]
    public class AnimationClipEditorEx : BaseEditorEx
    {
        public static readonly GUIContent GUI_ANIMATION_WIN = new GUIContent("Animation","Show Animation Window");
        public static readonly GUIContent GUI_ANIMATOR_WIN = new GUIContent("Animator", "Show Animator Window");

        Vector2 scrollPos;
        bool isFolded = true;
        public override string GetDefaultInspectorTypeName() => "UnityEditor.AnimationClipEditor";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var clip = target as AnimationClip;

            DrawButtons();

            isFolded = EditorGUILayout.Foldout(isFolded, "Curves", true);
            if (isFolded)
            {
                DrawCurvesInfo(clip, ref scrollPos);
            }
        }

        private static void DrawButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(GUI_ANIMATION_WIN))
                EditorWindow.GetWindow<AnimationWindow>().Show();
            if (GUILayout.Button(GUI_ANIMATOR_WIN))
                EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
            GUILayout.EndHorizontal();
        }

        public static void DrawCurvesInfo(AnimationClip clip, ref Vector2 scrollPos)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);
            var guiX = new GUIContent("X");
            var labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.richText = true;

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            
            for (int i = 0; i < bindings.Length; i++) 
            {
                var binding = bindings[i];

                var info = ($"{i} <b>{binding.path}</b>.<color=#008800ff>{binding.propertyName}</color> ({binding.type})");
                guiX.tooltip = $"delete : {info}";

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(guiX, GUILayout.Width(30)))
                {
                    if (EditorUtility.DisplayDialog("Warning", $"delete curve: {binding.path}.{binding.propertyName}", "ok"))
                        RemoveCurvesByProperty(clip, binding.type, binding.path, binding.propertyName);
                }
                EditorGUILayout.LabelField(info, labelStyle);

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        public static void RemoveCurvesByProperty(AnimationClip clip, Type targetType, string path, string propertyName, NameMatchMode matchMode = NameMatchMode.Full)
        {
            if (!clip)
                return;

            var bindings = AnimationUtility.GetCurveBindings(clip);
            var removedCount = 0;
            foreach (var binding in bindings)
            {

                var isTypeValid = binding.type == targetType;
                var isPropValid = binding.propertyName.IsMatch(propertyName, matchMode);
                var isPathValid = binding.path == path;

                if (isTypeValid && isPropValid && isPathValid)
                {
                    Undo.RecordObject(clip, $"delete curve {binding.path}");
                    AnimationUtility.SetEditorCurve(clip, binding, null);
                    removedCount++;

                    //Debug.Log($"removed curve : path{binding.path} , name:{binding.propertyName}");
                }
            }
            if (removedCount > 0)
            {
                EditorUtility.SetDirty(clip);
                AssetDatabaseTools.SaveRefresh();
            }

        }
    }
}
#endif