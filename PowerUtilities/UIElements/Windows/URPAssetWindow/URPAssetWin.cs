#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    public class URPAssetWin : BaseUXMLEditorWindow, IUIElementEvent
    {
        ListView assetListView;
        ListView assetDataListView;
        UniversalRenderPipelineAsset[] urpAssets;

        VisualTreeAsset assetRowUxml;

        Func<VisualElement> MakeAssetRowFunc => () => assetRowUxml?.CloneTree();

        [MenuItem(ROOT_MENU+"/URP/"+nameof(URPAssetWin))]
        static void Init()
        {
            var win = GetWindow<URPAssetWin>();
            win.titleContent = new GUIContent("URPAssetView");
            
            //win.position = new Rect(0,0,400,300);
        }

        public override void CreateGUI()
        {
            var assetRowPath = AssetDatabaseTools.FindAssetsPath("URPAssetRow", "uxml").FirstOrDefault();
            assetRowUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetRowPath);

            var uxmlPath = AssetDatabaseTools.FindAssetsPath("URPAssetWin", "uxml").FirstOrDefault();
            treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            eventInstance = this;

            treeStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabaseTools.FindAssetPath("DefaultStyles", "uss"));

            base.CreateGUI();
        }

        public void OnProjectChange()
        {
            SetupURPAssetListView();
        }

        private void SetupURPAssetListView()
        {
            if (assetListView == null)
                return;

            urpAssets = AssetDatabaseTools.FindAssetsInProject<UniversalRenderPipelineAsset>();

            assetListView.itemsSource = urpAssets;
            assetListView.makeItem = MakeAssetRowFunc;
            assetListView.bindItem = (e,i) =>
            {
                e.Q<Label>("AssetName").text = urpAssets[i].name;
            };
            assetListView.onSelectionChange +=AssetListView_onSelectionChange;
            assetListView.SetSelection(0);
        }

        private void AssetListView_onSelectionChange(IEnumerable<object> assets)
        {
            var asset = assets.First();
            Selection.activeObject = (Object)asset;
        }

        public void AddEvent(VisualElement root)
        {
            assetListView = root.Q<ListView>("AssetListView");
            assetDataListView = root.Q<ListView>("AssetDataListView");
            SetupURPAssetListView();
        }
    }
}
#endif