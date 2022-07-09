#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using UnityEngine;
    using UnityEditor;
    using System.Reflection;
    using System;
    using System.Linq;

    [CustomPropertyDrawer(typeof(PowerGradient))]
    public class PowerGradientDrawer : PropertyDrawer
    {
        public const string NAME_COPY = "C";
        public const string NAME_PASTE = "P";

        static PowerGradient copyTarget;

        PowerGradient gradient;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (gradient == null)
            {
                var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
                gradient = obj as PowerGradient;
                //cannot get instance
                if (gradient == null)
                {
                    EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                    return;
                }
            }

            Event guiEvent = Event.current;

            EditorGUI.BeginProperty(position, label, property);
            {
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                var textureRect = new Rect(position.x, position.y, position.width - 40, position.height);
                var copyButtonRect = new Rect(textureRect.xMax + 1, textureRect.y, 20, textureRect.height);
                var pasteButtonRect = new Rect(copyButtonRect.xMax, textureRect.y, 20, textureRect.height);

                if (guiEvent.type == EventType.Repaint)
                {
                    GUIStyle gradientStyle = new GUIStyle();
                    gradientStyle.normal.background = gradient.GetTexture((int)position.width);

                    GUI.Label(textureRect, GUIContent.none, gradientStyle);
                    GUI.Button(copyButtonRect, NAME_COPY);
                    GUI.Button(pasteButtonRect, NAME_PASTE);
                }
                else
                {
                    HandleAction(guiEvent, gradient, property, ref textureRect, ref copyButtonRect, ref pasteButtonRect);

                }
            }
            EditorGUI.EndProperty();
        }


        private void HandleAction(Event guiEvent, PowerGradient gradient, SerializedProperty sp, ref Rect textureRect, ref Rect copyButtonRect, ref Rect pasteButtonRect)
        {
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
            {
                if (textureRect.Contains(guiEvent.mousePosition))
                {
                    PowerGradientWindow window = EditorWindow.GetWindow<PowerGradientWindow>();
                    window.SetGradient(gradient, sp);
                }
                else if (copyButtonRect.Contains(guiEvent.mousePosition))
                {
                    Copy(gradient);
                }
                else if (pasteButtonRect.Contains(guiEvent.mousePosition))
                {
                    Paste(copyTarget, gradient);
                }
            }
        }

        void Copy(PowerGradient g)
        {
            copyTarget = g;
        }

        void Paste(PowerGradient from, PowerGradient to)
        {
            if (from != null && to != null)
            {
                to.CopyFrom(from);
            }
        }
    }
}
#endif