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
        public static ItemPlacementWindow current;
        [MenuItem(ROOT_MENU+"/Scene/"+nameof(ItemPlacementWindow))]
        static void Init()
        {
            var win = GetWindow<ItemPlacementWindow>();
            var uxmlPath = AssetDatabaseTools.FindAssetsPath("ItemPlacementWindow", "uxml").FirstOrDefault();

            win.treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            win.isShowCommonHeader = false;
            win.CreateGUI();

            win.eventInstance = win;
            win.AddTreeEvents();
            current = win;
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui -= SceneView_duringSceneGui;
            SceneView.duringSceneGui += SceneView_duringSceneGui;

            if (!current)
                Init();
        }

        private void SceneView_duringSceneGui(SceneView obj)
        {
            var e = Event.current;
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (Physics.Raycast(ray, out var hit, float.MaxValue, -1))
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

                DrawGuideLine(e.mousePosition, hit.point,hit.normal);
            }
        }

        private static void DrawGuideLine(Vector2 mousePos,Vector3 point,Vector3 normal)
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
            var endPos = current.normalAlign.value ? normal : Vector3.up;
            Handles.color = Color.cyan;
            Handles.DrawPolyLine(new[] {
                    point,
                    point + endPos
                });
            Handles.color = lastColor;
        }

        Object prefab;
        Toggle normalAlign;

        public void AddEvent(VisualElement root)
        {
            root.Q<ObjectField>("Prefab").RegisterValueChangedCallback(e => prefab = e.newValue);
            normalAlign = root.Q<Toggle>("NormalAlign");
            Debug.Log(normalAlign.value);
        }
    }
}
#endif