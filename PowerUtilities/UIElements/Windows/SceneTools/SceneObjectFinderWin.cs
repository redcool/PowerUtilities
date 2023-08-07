#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        }

        private void FindObjects()
        {
            var layer = rootVisualElement.Q<LayerField>("LayerFind").value;
            var tag = rootVisualElement.Q<TagField>("TagFind").value;
            var includeInactive = rootVisualElement.Q<Toggle>("IncludeInactive").value;
            var componentTypeNames = rootVisualElement.Q<TextField>("ComponentTypeNames").value.SplitBy();
            var objNames = rootVisualElement.Q<TextField>("ObjectNames").value.SplitBy();

            var parentGo = Selection.activeGameObject;

            var trs =
                parentGo ? parentGo.GetComponentsInChildren<Transform>(includeInactive) :
                FindObjectsByType<Transform>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            var gos = trs.Where(tr => tr.gameObject.layer == layer)
                .Where(tr => tr.CompareTag(tag))
                .Where(tr => (componentTypeNames.Length>0 ? componentTypeNames.Where(comName => tr.GetComponent(comName)).Count() > 0 : true))
                .Where(tr => (objNames.Length > 0 ? objNames.Where(objName => tr.gameObject.name.Contains(objName)).Count() > 0 : true))
                .Select(tr => tr.gameObject)
                .ToArray();

            Selection.objects = gos;
        }
    }
}
#endif