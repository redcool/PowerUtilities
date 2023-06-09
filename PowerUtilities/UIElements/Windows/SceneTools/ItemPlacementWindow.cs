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
            var layerMask = this.layerMask.value;

            if (Physics.Raycast(ray, out var hit, float.MaxValue, layerMask))
            {
                if (prefab && e.IsMouseLeftDown())
                {
                    var inst = (GameObject)(PrefabUtility.IsPartOfAnyPrefab(prefab) ?
                        PrefabUtility.InstantiatePrefab(prefab) : Instantiate(prefab));
                    ;

                    inst.transform.position = hit.point;

                    if (normalAlign.value)
                        inst.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                }

                var endPos = normalAlign.value ? hit.normal : Vector3.up;
                DrawGuideLine(e.mousePosition, hit.point, endPos);
            }
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

        Object prefab;
        Toggle normalAlign;
        LayerMaskField layerMask;

        public void AddEvent(VisualElement root)
        {
            root.Q<ObjectField>("Prefab").RegisterValueChangedCallback(e => prefab = e.newValue);
            normalAlign = root.Q<Toggle>("NormalAlign");
            layerMask = root.Q<LayerMaskField>("LayerMask");
        }
    }
}
#endif