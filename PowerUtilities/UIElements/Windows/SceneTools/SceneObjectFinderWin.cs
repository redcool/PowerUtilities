#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PowerUtilities
{
    public class SceneObjectFinderWin : BaseUXMLEditorWindow, IUIElementEvent
    {
        [MenuItem(ROOT_MENU+"/Scene/"+nameof(SceneObjectFinderWin))]
        static void OpenWindow()
        {
            EditorWindowTools.OpenWindow<SceneObjectFinderWin>("ObjectFinder");
        }
        public override void CreateGUI()
        {
            LoadUxmlUss("SceneObjectFinderWin", "");
            base.IsReplaceRootContainer = true;
            base.CreateGUI();
        }

        public void AddEvent(VisualElement root)
        {
            root.Q<Button>("bt_find").clicked -= FindObjects;
            root.Q<Button>("bt_find").clicked += FindObjects;

            root.Q<Button>("bt_clear").clicked -= Clear ;
            root.Q<Button>("bt_clear").clicked += Clear;
        }

        void Clear()
        {
            Selection.activeGameObject = null;
        }

        private void FindObjects()
        {
            var layer = rootVisualElement.Q<LayerField>("LayerFind").value;
            var tag = rootVisualElement.Q<TagField>("TagFind").value;
            var includeInactive = rootVisualElement.Q<Toggle>("IncludeInactive").value;
            var componentTypeNames = rootVisualElement.Q<TextField>("ComponentTypeNames").value.SplitBy();
            var objNames = rootVisualElement.Q<TextField>("ObjectNames").value.SplitBy();

            var isLayerFind = rootVisualElement.Q<Toggle>("IsLayerFind").value;
            var isTagFind = rootVisualElement.Q<Toggle>("IsTagFind").value;
            var isComponentTypeNames = rootVisualElement.Q<Toggle>("IsComponentTypeNames").value;
            var isObjectNames = rootVisualElement.Q<Toggle>("IsObjectNames").value;

            var parentGo = Selection.activeGameObject;

            var trs =
                parentGo ? parentGo.GetComponentsInChildren<Transform>(includeInactive) :
                //FindObjectsByType<Transform>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                FindObjectsOfType<Transform>(includeInactive);

            var gos = trs.Where(tr => isLayerFind ? tr.gameObject.layer == layer : true)
                .Where(tr => isTagFind ? tr.CompareTag(tag) : true)
                .Where(tr => isComponentTypeNames ?
                    (componentTypeNames.Length > 0 ? componentTypeNames.Where(comName => tr.GetComponent(comName)).Count() > 0 : true) : true
                    )
                .Where(tr => isObjectNames ?
                (objNames.Length > 0 ? objNames.Where(objName => tr.gameObject.name.Contains(objName)).Count() > 0 : true) : true
                )
                .Select(tr => tr.gameObject)
                .ToArray();

            Selection.objects = gos;
        }
    }
}
#endif