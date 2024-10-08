#if UNITY_EDITOR
using PowerUtilities.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
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
        BaseSplitView rootSplitView,leftSplitView,rightSplitView;

        ScrollView pipelineAssetDetailScrollView;
        ScrollView dataDetailScrollView;

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

        // last pipeline asset
        RenderPipelineAsset lastPipeline;
        static Dictionary<Object, Editor> editorDict = new Dictionary<Object, Editor>();

        [MenuItem(ROOT_MENU+"/Pipeline/"+nameof(SRPPipelineAssetWin))]
        static void Init()
        {
            var win = GetWindow<SRPPipelineAssetWin>();
            if (!win)
                return;

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

            rootSplitView = root.Q<BaseSplitView>("Root");
            leftSplitView = root.Q<BaseSplitView>("LeftSplitView");
            rightSplitView = root.Q<BaseSplitView>("RightSplitView");

            pipelineAssetDetailScrollView = root.Q<ScrollView>("PipelineAssetDetailScrollView");
            dataDetailScrollView = root.Q<ScrollView>("DataDetailScrollView");


            SetupSRPPipelineAssetListView();

            // add global size changed
            rootSplitView.RegisterCallback<GeometryChangedEvent>(OnSizeChanged);
        }

        private void OnSizeChanged(GeometryChangedEvent evt)
        {
            pipelineAssetListView.parent.style.height = evt.newRect.height;
            //update, listView'parent(PipelineAssetGroup) and parent's parent(left BaseSplitView)
            rendererDataListView.parent.style.height = evt.newRect.height;
            leftSplitView.style.height = evt.newRect.height;

            pipelineAssetDetailScrollView.parent.style.height = evt.newRect.height;
            //update 
            dataDetailScrollView.parent.style.height = evt.newRect.height;
            rightSplitView.style.height = evt.newRect.height;
        }

        public void OnProjectChange()
        {
            SetupSRPPipelineAssetListView();
        }

        public void OnInspectorUpdate()
        {
            if(QualitySettings.renderPipeline != lastPipeline)
            {
                lastPipeline = QualitySettings.renderPipeline;
                SetupSRPPipelineAssetListView();
            }
        }

        
        private void SetupSRPPipelineAssetListView()
        {
            if (pipelineAssetListView == null)
                return;

            var selectedId = ScriptableRenderPipelineEditorTools.GetDefaultPipelineIndex(out urpAssets, out _);

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

                    // get override used or default pipeline
                    var isUsedPipeline = i == selectedId;
                    ve.EnableInClassList("listview-row-selected", isUsedPipeline);
                }
                , (assets) =>
                {
                    SetupSRPRendererDataListView();

                    ShowDetails(pipelineAssetDetailImgui, (Object)pipelineAssetListView.selectedItem);
                }
                , selectedId
            );
        }

        void ShowDetails(IMGUIContainer imgui,Object item)
        {
            if (imgui == null)
                return;

            imgui.pickingMode = PickingMode.Position;

            var editor = DictionaryTools.Get(editorDict, item, (item) => Editor.CreateEditor(item));
            
            //var editor = Editor.CreateEditor(item);
            imgui.onGUIHandler = editor.OnInspectorGUI;
        }

        public static void SetupListView(ListView listView, IList itemsSource, Func<VisualElement> makeItem
            , Action<VisualElement, int> bindItem, Action<IEnumerable<object>> onSelectionChange,int selectedId)
        {
            listView.Clear();

            listView.itemsSource = itemsSource;
            listView.makeItem = makeItem;
            listView.bindItem = bindItem;
#if UNITY_2022_3_OR_NEWER
            listView.selectionChanged -= onSelectionChange;
            listView.selectionChanged += onSelectionChange;
#else
            listView.onSelectionChange -= onSelectionChange;
            listView.onSelectionChange += onSelectionChange;
#endif
            listView.selectedIndex = selectedId;
        }

        private void SetupSRPRendererDataListView()
        {
            var urpAsset = urpAssets[pipelineAssetListView.selectedIndex];
            if (!urpAsset || rendererDataListView == null)
            {
                return;
            }
            var datas = urpAsset.GetRendererDatas();
            var selectedId = urpAsset.GetDefaultRendererIndex();

            SetupListView(rendererDataListView
                , datas
                , () => (assetDataRowUxml.Value?.CloneTree())
                , (ve, i) =>
                {
                    if (i >= datas.Length || datas[i] == null)
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

                    // show tag when i is defaut renderer
                    if (urpAsset)
                    {
                        var isDefaultRenderer = selectedId == i;
                        ve.EnableInClassList("listview-row-selected", isDefaultRenderer);
                    }
                }
                , (IEnumerable<object> obj) =>
                {
                    //ShowRendererDataDetails();
                    ShowDetails(rendererDataDetailImgui, (Object)rendererDataListView.selectedItem);
                }
                ,selectedId
            );
            // call once positive
            ShowDetails(rendererDataDetailImgui, (Object)rendererDataListView.selectedItem);
        }
    }
}
#endif