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
        ListView pipelineAssetListView;
        ListView rendererDataListView;
        IMGUIContainer details;

        UniversalRenderPipelineAsset[] urpAssets;

        Lazy<VisualTreeAsset> assetRowUxml = new Lazy<VisualTreeAsset>(
            () => AssetDatabaseTools.FindAssetPathAndLoad<VisualTreeAsset>(out var _,"URPAssetRowItem", "uxml")
        );

        Lazy<VisualTreeAsset> assetDataRowUxml = new Lazy<VisualTreeAsset>(
            () => AssetDatabaseTools.FindAssetPathAndLoad<VisualTreeAsset>(out var _, "URPRendererDataRowItem", "uxml")
        );

        Lazy<VisualTreeAsset> lazyTreeAsset = new Lazy<VisualTreeAsset>(
            () => AssetDatabaseTools.FindAssetPathAndLoad<VisualTreeAsset>(out var _,"URPAssetWin", "uxml")
        );

        
        

        [MenuItem(ROOT_MENU+"/URP/"+nameof(URPAssetWin))]
        static void Init()
        {
            var win = GetWindow<URPAssetWin>();
            win.titleContent = new GUIContent("URPAssetView");
            
            //win.position = new Rect(0,0,400,300);
        }

        public override void CreateGUI()
        {
            treeAsset = lazyTreeAsset.Value;
            eventInstance = this;

            base.CreateGUI();
        }

        public void AddEvent(VisualElement root)
        {
            pipelineAssetListView = root.Q<ListView>("PipelineAssetListView");
            rendererDataListView = root.Q<ListView>("RendererDataListView");
            details = root.Q<IMGUIContainer>();

            SetupURPAssetListView();
        }

        public void OnProjectChange()
        {
            SetupURPAssetListView();
        }

        private void SetupURPAssetListView()
        {
            if (pipelineAssetListView == null)
                return;

            urpAssets = AssetDatabaseTools.FindAssetsInProject<UniversalRenderPipelineAsset>();
            Action OnSelect = () => {
                Selection.activeObject = (Object)pipelineAssetListView.selectedItem;
            };


            SetupListView(pipelineAssetListView
                , urpAssets
                , () => assetRowUxml.Value?.CloneTree()
                , (e, i) =>
                {
                    e.Q<Label>("PipelineAssetName").text = urpAssets[i].name;
                }
                , (assets) =>
                {
                    var asset = assets.First();

                    SetupURPRendererDataListView();
                }
            );
        }

        public static void SetupListView(ListView listView, IList itemsSource, Func<VisualElement> makeItem
            , Action<VisualElement, int> bindItem, Action<IEnumerable<object>> onSelectionChange)
        {
            listView.Clear();
            listView.itemsSource = itemsSource;
            listView.makeItem = makeItem;
            listView.bindItem = bindItem;
            listView.onSelectionChange -= onSelectionChange;
            listView.onSelectionChange += onSelectionChange;
            listView.selectedIndex = 0;
            listView.SetSelection(0);
        }

        private void SetupURPRendererDataListView()
        {
            var urpAsset = urpAssets[pipelineAssetListView.selectedIndex];
            if (!urpAsset || rendererDataListView == null)
            {
                return;
            }
            var datas = urpAsset.GetRendererDatas();
            Action onSelect = () => {
                Selection.activeObject = (Object)rendererDataListView.selectedItem;
            };

            SetupListView(rendererDataListView
                , datas
                , () => assetDataRowUxml.Value?.CloneTree()
                , (e, i) =>
                {
                    if (i >= datas.Length)
                    {
                        return;
                    }
                    var dataNameLabel = e.Q<Label>("RendererDataName");
                    dataNameLabel.text = datas[i].name;
                }
                ,(IEnumerable<object> obj) =>
                {
                    var data = (UniversalRendererData)obj.First();
                    ShowRendererDataDetails(data);
                }
            );
        }

        private void ShowRendererDataDetails(UniversalRendererData data)
        {
            if (details == null)
                return;

            details.onGUIHandler = () =>
            {
                Editor.CreateEditor(data).DrawDefaultInspector();
            };
        }
    }
}
#endif