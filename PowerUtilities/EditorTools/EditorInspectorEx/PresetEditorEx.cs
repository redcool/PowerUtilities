#if UNITY_EDITOR
using PowerUtilities.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UIElements;

namespace PowerUtilities
{

    [CustomEditor(typeof(Preset))]
    [CanEditMultipleObjects]
    public class PresetEditorEx : BaseEditorEx
    {
        public override string GetDefaultInspectorTypeName() => "UnityEditor.Presets.PresetEditor";
        public List<string> excludePropertiesList = new();
        ListView excludePropertiesListView;

        public override void OnEnable()
        {
            base.OnEnable();

            var preset = target as Preset;

            excludePropertiesList.Clear();
            excludePropertiesList.AddRange(preset.excludedProperties);
        }

        public override VisualElement CreateInspectorGUI()
        {
            var rootUI = base.CreateInspectorGUI();

            SetupListView(ref excludePropertiesListView);
            rootUI.Add(excludePropertiesListView);

            return rootUI;
        }

        ListView SetupListView(ref ListView listView)
        {
            Func<VisualElement> makeItem = () =>
            {
                var tf = new TextField();
                tf.style.flexGrow = 1;
                return tf;
            };
            Action<VisualElement, int> bindItem = (element, index) =>
            {
                var tf = (element as TextField);
                if (tf == null)
                    return;

                //tf.value = excludePropertiesList[index];
                tf.SetValueWithoutNotify(excludePropertiesList[index]);

                tf.RegisterValueChangedCallback(OnTextFieldValueChanged); 
            };

            listView = new ListView(excludePropertiesList,18,makeItem,bindItem);
            listView.showAddRemoveFooter = true;
            listView.showFoldoutHeader = true;
            listView.headerTitle = "ExcludeProperpies";
            listView.style.flexGrow = 1f;

            // use this jinstead listView.itemsRemoved,(has bug)
            listView.viewController.itemsSourceSizeChanged += ViewController_itemsSourceSizeChanged;
            return listView;
        }

        private void ViewController_itemsSourceSizeChanged()
        {
            SaveExcludeProperties();
        }

        private void OnTextFieldValueChanged(ChangeEvent<string> evt)
        {
            var curId = excludePropertiesListView.selectedIndex;
            excludePropertiesList[curId] = evt.newValue;

            SaveExcludeProperties();
        }

        public void SaveExcludeProperties()
        {
            var preset = target as Preset;
            if (!preset)
                return;

            preset.excludedProperties = excludePropertiesList.ToArray();
        }

        private void ListView_itemsRemoved(IEnumerable<int> results)
        {

            foreach (var item in results)
            {
                if (item >= 0 && item < excludePropertiesList.Count)
                    excludePropertiesList?.RemoveAt(item);
            }
            excludePropertiesListView.ClearSelection();

            excludePropertiesListView.RefreshItems();

            
            Debug.Log("done");
        }

    }
}
#endif