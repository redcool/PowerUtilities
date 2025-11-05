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
        Vector2 scrollPos;
        public override string GetDefaultInspectorTypeName() => "UnityEditor.AnimationClipEditor";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var clip = target as AnimationClip;
            DrawCurvesInfo(clip,ref scrollPos);
        }

        public static void DrawCurvesInfo(AnimationClip clip,ref Vector2 scrollPos)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);
            var guiX = new GUIContent("X");
            var labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.richText = true;

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            foreach (var binding in bindings)
            {
                var info = ($"<b>{binding.path}</b>.<color=#008800ff>{binding.propertyName}</color> ({binding.type})");
                guiX.tooltip = $"delete : {info}";

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(guiX,GUILayout.Width(30)))
                {
                    if(EditorUtility.DisplayDialog("Warning",$"delete curve: {binding.path}.{binding.propertyName}","ok"))
                        RemoveCurvesByProperty(clip, binding.type, binding.propertyName);
                }
                EditorGUILayout.LabelField(info, labelStyle);

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        public static void RemoveCurvesByProperty(AnimationClip clip, Type targetType, string propertyName, NameMatchMode matchMode = NameMatchMode.Full)
        {
            if (!clip)
                return;

            var bindings = AnimationUtility.GetCurveBindings(clip);
            var removedCount = 0;
            foreach (var binding in bindings)
            {
                var isTypeValid = binding.type == targetType;
                var isPropValid = binding.propertyName.IsMatch(propertyName, matchMode);

                if (isTypeValid && isPropValid)
                {
                    Undo.RecordObject(clip, $"delete curve {binding.path}");
                    AnimationUtility.SetEditorCurve(clip, binding, null);
                    removedCount++;

                    //Debug.Log($"removed curve : path{binding.path} , name:{binding.propertyName}");
                }
            }
            if (removedCount > 0) {
                EditorUtility.SetDirty(clip);
                AssetDatabaseTools.SaveRefresh();
            }

        }
    }
}
#endif