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
    public class SRPPipelineAssetWin : BaseUXMLEditorWindow, IUIElementEvent
    {
        ListView pipelineAssetListView;
        ListView rendererDataListView;
        IMGUIContainer rendererDataDetailImgui;
        IMGUIContainer pipelineAssetDetailImgui;

        UniversalRenderPipelineAsset[] urpAssets;

        Lazy<VisualTreeAsset> assetRowUxml = new Lazy<VisualTreeAsset>(
            () => AssetDatabaseTools.FindAssetPathAndLoad<VisualTreeAsset>(out var _, "URPAssetRowItem", "uxml")
        );

        Lazy<VisualTreeAsset> assetDataRowUxml = new Lazy<VisualTreeAsset>(
            () => AssetDatabaseTools.FindAssetPathAndLoad<VisualTreeAsset>(out var _, "URPRendererDataRowItem", "uxml")
        );

        Lazy<VisualTreeAsset> lazyTreeAsset = new Lazy<VisualTreeAsset>(
            () => AssetDatabaseTools.FindAssetPathAndLoad<VisualTreeAsset>(out var _, "URPAssetWin", "uxml")
        );




        [MenuItem(ROOT_MENU+"/Pipeline/"+nameof(SRPPipelineAssetWin))]
        static void Init()
        {
            var win = GetWindow<SRPPipelineAssetWin>();
            win.titleContent = new GUIContent("SRPPipelineAssetWin");
            if (win.position.width < 1400)
            {
                win.position = new Rect(100, 100, 1440, 900);
            }
            win.ShowPopup();
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
            rendererDataDetailImgui = root.Q<IMGUIContainer>("RendererDataDetailIMGUI");
            pipelineAssetDetailImgui = root.Q<IMGUIContainer>("PipelineAssetDetailIMGUI");

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

            SetupListView(pipelineAssetListView
                , urpAssets
                , () => (assetRowUxml.Value?.CloneTree())
                , (ve, i) =>
                {
                    Label label = ve.Q<Label>("PipelineAssetName");
                    label.text = urpAssets[i].name;
                    label.RegisterCallback((ContextClickEvent e) => {
                        var curId = i;
                        pipelineAssetListView.SetSelection(curId);
                        Selection.activeObject = (Object)pipelineAssetListView.selectedItem;
                    });
                }
                , (assets) =>
                {
                    SetupURPRendererDataListView();

                    ShowPipelineAssetDetails();
                }
            );
        }

        void ShowPipelineAssetDetails()
        {
            if (pipelineAssetDetailImgui == null)
                return;

            pipelineAssetDetailImgui.onGUIHandler =() =>
            {
                Editor.CreateEditor((Object)pipelineAssetListView.selectedItem).OnInspectorGUI();
            };
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

            SetupListView(rendererDataListView
                , datas
                , () => (assetDataRowUxml.Value?.CloneTree())
                , (ve, i) =>
                {
                    if (i >= datas.Length)
                    {
                        return;
                    }
                    var dataNameLabel = ve.Q<Label>("RendererDataName");
                    dataNameLabel.text = datas[i].name;

                    dataNameLabel.RegisterCallback((ContextClickEvent e) =>
                    {
                        var curId = i;
                        rendererDataListView.SetSelection(curId);
                        Selection.activeObject = (Object)rendererDataListView.selectedItem;
                    });
                }
                , (IEnumerable<object> obj) =>
                {
                    ShowRendererDataDetails();

                }
            );
            void ShowRendererDataDetails()
            {
                if (rendererDataDetailImgui == null)
                    return;

                rendererDataDetailImgui.onGUIHandler = () =>
                {
                    Editor.CreateEditor((Object)rendererDataListView.selectedItem).OnInspectorGUI();
                };
            }
        }
    }
}
#endif