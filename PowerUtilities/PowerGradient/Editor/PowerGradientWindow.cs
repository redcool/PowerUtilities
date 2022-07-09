#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using Random = UnityEngine.Random;

    public class PowerGradientWindow : EditorWindow
    {
        SerializedProperty property;
        PowerGradient gradient;
        const int borderSize = 10;
        const float keyWidth = 10;
        const float keyHeight = 20;

        Rect gradientPreviewRect;
        Rect[] keyRects;
        bool mouseIsDownOverKey;
        bool needsRepaint;

        int selectedKeyIndex;

        Rect keyInteractiveRect;

        private void Update()
        {
            TryClose();
        }

        private void TryClose()
        {
            var type = EditorWindow.focusedWindow.GetType();
            var isSelfWindow = (type.Name.Contains("ColorPicker") || type.Name.Contains(nameof(PowerGradientWindow)));
            if (!isSelfWindow)
            {
                Close();
            }
        }

        private void OnGUI()
        {
            if(property == null || !Selection.activeGameObject)
            {
                Close();
                return;
            }
            var e = Event.current;

            property.serializedObject.Update();

            Draw();

            HandleInput(e);

            property.serializedObject.ApplyModifiedProperties();

            if (needsRepaint)
            {
                needsRepaint = false;

                selectedKeyIndex = gradient.SortKeys(selectedKeyIndex);
                Repaint();
            }
        }

        void Draw()
        {
            EditorGUIUtility.labelWidth = 100;

            gradientPreviewRect = new Rect(borderSize, borderSize, position.width - borderSize * 2, 25);
            keyInteractiveRect = new Rect(borderSize, gradientPreviewRect.yMax + borderSize, gradientPreviewRect.width, borderSize + keyHeight);

            GUI.DrawTexture(gradientPreviewRect, gradient.GetTexture((int)gradientPreviewRect.width));
            var keyRectHeight = DrawKeys();

            Rect settingsRect = new Rect(borderSize, keyRectHeight + borderSize, position.width - borderSize * 2, position.height + 100);
            DrawSettings(settingsRect);
        }
        float DrawKeys()
        {
            keyRects = new Rect[gradient.NumKeys];
            for (int i = 0; i < gradient.NumKeys; i++)
            {
                var key = gradient.GetKey(i);
                var keyRect = new Rect(gradientPreviewRect.x + gradientPreviewRect.width * key.Time - keyWidth / 2f, gradientPreviewRect.yMax + borderSize, keyWidth, keyHeight);

                if (i == selectedKeyIndex)
                {
                    var c = Color.white == key.Color ? Color.black : Color.white;
                    EditorGUI.DrawRect(new Rect(keyRect.x - 2, keyRect.y - 2, keyRect.width + 4, keyRect.height + 4), c);
                }
                //vertical line
                DrawVerticalLine(key.Color, ref keyRect);

                EditorGUI.DrawRect(keyRect, key.Color);
                keyRects[i] = keyRect;
            }
            return keyRects[0].yMax;
        }

        private void DrawVerticalLine(Color keyColor, ref Rect keyRect)
        {
            var lineSize = new Vector2(1, -keyRect.height);
            var linePos = new Vector2(keyRect.center.x- lineSize.x *0.5f, keyRect.position.y);
            EditorGUI.DrawRect(new Rect(linePos, lineSize), keyColor);
        }

        private void DrawSettings(Rect settingsRect)
        {
            GUILayout.BeginArea(settingsRect);
            {
                DrawSelectedKey();
                //gradient.blendMode = (PowerGradient.BlendMode)EditorGUILayout.EnumPopup("Blend mode", gradient.blendMode);
                //gradient.randomizeColour = EditorGUILayout.Toggle("Randomize colour", gradient.randomizeColour);
                //gradient.range = EditorGUILayout.Vector2Field("Range", gradient.range);

                EditorGUILayout.PropertyField(property.FindPropertyRelative("blendMode"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("randomizeColour"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("range"));

            }
            GUILayout.EndArea();
        }

        private void DrawSelectedKey()
        {
            if (selectedKeyIndex >= gradient.NumKeys)
                return;

            GUILayout.BeginHorizontal("box");
            {
                var key = GetKeyProp(selectedKeyIndex);

                //key colour
                EditorGUILayout.PropertyField(key.FindPropertyRelative("colour"));

                //key time 
                DrawKeyTime(key.FindPropertyRelative("time"));

            }
            GUILayout.EndHorizontal();
        }

        private void DrawKeyTime(SerializedProperty keyTime)
        {
            //EditorGUILayout.PropertyField(keyTime);
            EditorGUI.BeginChangeCheck();

            var time = keyTime.floatValue;
            time = EditorGUILayout.FloatField("Location", time * gradient.range.y, GUILayout.Width(200));
            time = Mathf.Clamp(time, gradient.range.x, gradient.range.y);
            time /= gradient.range.y;
            keyTime.floatValue = time;

            if (EditorGUI.EndChangeCheck())
            {
                needsRepaint = true;
            }
        }


        SerializedProperty GetKeyProp(int id)
        {
            return property.FindPropertyRelative("keys").GetArrayElementAtIndex(id);
        }


        void HandleInput(Event guiEvent)
        {
            HandleMouse(guiEvent);
            HandleKeyboard(guiEvent);
        }

        private void HandleKeyboard(Event guiEvent)
        {
            if (guiEvent.keyCode == KeyCode.Delete && guiEvent.type == EventType.KeyDown)
            {
                selectedKeyIndex = DeleteKey(selectedKeyIndex);
                needsRepaint = true;
            }
        }

        int DeleteKey(int selectedKeyIndex)
        {
            gradient.RemoveKey(selectedKeyIndex);
            if (selectedKeyIndex >= gradient.NumKeys)
            {
                selectedKeyIndex--;
            }
            return selectedKeyIndex;
        }

        private void HandleMouse(Event guiEvent)
        {
            if (guiEvent.button != 0)
                return;

            if (guiEvent.type == EventType.MouseDown)
            {
                for (int i = 0; i < keyRects.Length; i++)
                {
                    var rect = keyRects[i];
                    rect.position += new Vector2(-2, -2);
                    rect.size += new Vector2(4, 4);

                    if (rect.Contains(guiEvent.mousePosition))
                    {
                        selectedKeyIndex = i;
                        mouseIsDownOverKey = true;
                        needsRepaint = true;
                        break;
                    }
                }

                if (keyInteractiveRect.Contains(guiEvent.mousePosition) && !mouseIsDownOverKey)
                {
                    float keyTime = Mathf.InverseLerp(gradientPreviewRect.x, gradientPreviewRect.xMax, guiEvent.mousePosition.x);
                    Color interpolatedColour = gradient.Evaluate(keyTime);
                    Color randomColour = new Color(Random.value, Random.value, Random.value);

                    selectedKeyIndex = gradient.AddKey((gradient.randomizeColour) ? randomColour : interpolatedColour, keyTime);
                    mouseIsDownOverKey = true;
                    needsRepaint = true;
                }
            }

            if (guiEvent.type == EventType.MouseUp)
            {
                if (mouseIsDownOverKey && gradientPreviewRect.Contains(guiEvent.mousePosition))
                {
                    selectedKeyIndex = DeleteKey(selectedKeyIndex);
                    needsRepaint = true;
                }

                mouseIsDownOverKey = false;
            }

            if (mouseIsDownOverKey && guiEvent.type == EventType.MouseDrag)
            {
                float keyTime = Mathf.InverseLerp(gradientPreviewRect.x, gradientPreviewRect.xMax, guiEvent.mousePosition.x);
                //selectedKeyIndex = gradient.UpdateKeyTime(selectedKeyIndex, keyTime);

                var key = GetKeyProp(selectedKeyIndex).FindPropertyRelative("time");
                key.floatValue = keyTime;

                needsRepaint = true;
            }
        }

        public void SetGradient(PowerGradient gradient, SerializedProperty sp)
        {
            this.gradient = gradient;
            this.property = sp;
        }

        private void OnEnable()
        {
            titleContent.text = "Power Gradient Window";
            minSize = new Vector2(400, 200);
            maxSize = new Vector2(1920, 200);
            position.Set(position.x, position.y, minSize.x, minSize.y);
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}
#endif