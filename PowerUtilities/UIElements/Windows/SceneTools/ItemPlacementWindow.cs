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
    public class ItemPlacementWindow : PowerEditorWindow,IUIElementEvent
    {
        [MenuItem(ROOT_MENU+"/Scene/"+nameof(ItemPlacementWindow))]
        new static void OpenWindow()
        {
            var win = GetWindow<ItemPlacementWindow>();
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

        new public void CreateGUI()
        {
            var uxmlPath = AssetDatabaseTools.FindAssetsPath("ItemPlacementWindow", "uxml").FirstOrDefault();

            treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            isShowCommonHeader = false;
            base.CreateGUI();

            eventInstance = this;
            AddTreeEvents();
        }

        private void SceneView_duringSceneGui(SceneView obj)
        {
            var e = Event.current;
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            var layerMask = this.layerMaskField.value;

            if (Physics.Raycast(ray, out var hit, float.MaxValue, layerMask))
            {
                var prefab = prefabField.value;
                if (activeField.value 
                    && prefab 
                    && e.IsMouseLeftDown())
                {
                    var inst = (GameObject)(PrefabUtility.IsPartOfAnyPrefab(prefab) ?
                        PrefabUtility.InstantiatePrefab(prefab) : Instantiate(prefab));
                    ;

                    inst.transform.position = hit.point;

                    if (normalAlignField.value)
                        inst.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                }

                var endPos = normalAlignField.value ? hit.normal : Vector3.up;
                DrawGuideLine(e.mousePosition, hit.point, endPos);
            }
            rootField.SetEnabled(activeField.value);
        }

        public static void DrawGuideLine(Vector2 mousePos,Vector3 point,Vector3 endPos)
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