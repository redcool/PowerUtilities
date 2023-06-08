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
        static void Init()
        {
            var win = GetWindow<ItemPlacementWindow>();
            var uxmlPath = AssetDatabaseTools.FindAssetsPath("ItemPlacementWindow", "uxml").FirstOrDefault();

            win.treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            win.isShowCommonHeader = false;
            win.CreateGUI();

            win.eventInstance = win;
            win.AddTreeEvents();

        }

        private void OnEnable()
        {
            SceneView.duringSceneGui -= SceneView_duringSceneGui;
            SceneView.duringSceneGui += SceneView_duringSceneGui;
        }

        private void SceneView_duringSceneGui(SceneView obj)
        {
            var e = Event.current;
            if (prefab && e.IsMouseLeftDown())
            {
                var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray,out var hit, float.MaxValue, -1))
                {
                    var inst = (GameObject) PrefabUtility.InstantiatePrefab(prefab);
                    inst.transform.position = hit.point;
                    inst.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                }
                
            }
        }

        Object prefab;
        Toggle normalAlign;

        public void AddEvent(VisualElement root)
        {
            Debug.Log("add event");
            root.Q<ObjectField>("Prefab").RegisterValueChangedCallback(e => prefab = e.newValue);
            normalAlign = root.Q<Toggle>("NormalAlign");
        }
    }
}
#endif