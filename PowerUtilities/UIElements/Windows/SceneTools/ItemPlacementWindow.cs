#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PowerUtilities
{
    /// <summary>
    /// Simple spawn object view tools
    /// </summary>
    public class ItemPlacementWindow : BaseUXMLEditorWindow, IUIElementEvent
    {
        [MenuItem(ROOT_MENU+"/Scene/"+nameof(ItemPlacementWindow))]
        static void OpenWindow()
        {
            var win = GetWindow<ItemPlacementWindow>();
            win.titleContent = new GUIContent("ItemPlaceWin");
        }
        public override void CreateGUI()
        {
            LoadUxmlUss("ItemPlacementWindow", "");
            base.CreateGUI();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui -= SceneView_duringSceneGui;
            SceneView.duringSceneGui += SceneView_duringSceneGui;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= SceneView_duringSceneGui;
        }


        private void SceneView_duringSceneGui(SceneView obj)
        {
            if (!activeField.value)
                return;
            
            var e = Event.current;
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            var layerMask = this.layerMaskField.value;

            if (Physics.Raycast(ray, out var hit, float.MaxValue, layerMask))
            {
                // show guide line
                var endPos = normalAlignField.value ? hit.normal : Vector3.up;
                DrawGuideLine(e.mousePosition, hit.point, endPos);

                // try create
                var prefab = prefabField.value;
                if (!e.IsMouseLeftDown())
                    return;

                if (prefab && prefab is GameObject go)
                {
                    var inst = Instantiate(go);
                    Undo.RegisterCreatedObjectUndo(inst, $"{inst.name} created");

                    inst.transform.parent = go.transform.parent;

                    inst.transform.position = hit.point;

                    if (normalAlignField.value)
                        inst.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                }

            }
        }

        public static void DrawGuideLine(Vector2 mousePos, Vector3 point, Vector3 endPos)
        {
            var lastColor = Handles.color;

            Handles.BeginGUI();
            GUI.color = Color.cyan;
            GUILayout.BeginArea(new Rect(mousePos, new Vector2(100, 50)));
            GUILayout.Label(point.ToString());
            GUILayout.EndArea();
            GUI.color = lastColor;
            Handles.EndGUI();

            //draw guide line
            Handles.color = Color.cyan;
            Handles.DrawPolyLine(new[] {
                    point,
                    point + endPos
                });
            Handles.color = lastColor;
        }

        ObjectField prefabField;
        Toggle normalAlignField;
        LayerMaskField layerMaskField;
        Toggle activeField;
        VisualElement rootField;

        public void AddEvent(VisualElement root)
        {
            prefabField = root.Q<ObjectField>("Prefab");
            normalAlignField = root.Q<Toggle>("NormalAlign");
            layerMaskField = root.Q<LayerMaskField>("LayerMask");
            activeField = root.Q<Toggle>("Active");
            rootField = root.Q<VisualElement>("Root");
        }
    }
}
#endif