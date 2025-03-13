#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;
    using Random = UnityEngine.Random;

    [CustomEditor(typeof(AssetBundleEditorTool))]
    public class AssetBundleExEditor : PowerEditor<AssetBundleEditorTool>
    {
        TextField abNameTextField;
        ListView abAssetView, abNameView;
        Foldout dependencyFoldout;

        AssetBundleEditorTool inst;


        public override VisualElement CreateInspectorGUI()
        {
            inst = target as AssetBundleEditorTool;

            if (inst.infoList.Count == 0)
            {
                FindABNames(ref inst.infoList);
            }

            // adjust root container
            var root = new VisualElement();
            root.style.marginTop = 10;

            // load uxml
            var nameExt = inst.uxmlName.SplitFileNameExt();
            var layoutAsset = AssetDatabaseTools.FindAssetPathAndLoad<VisualTreeAsset>(out var _, nameExt.name, nameExt.ext, true);
            layoutAsset.CloneTree(root);

            // setup list view
            abAssetView = root.Q<ListView>("AB_Asset_ListView");
            abNameView = root.Q<ListView>("AB_Name_ListView");

            abNameTextField = root.Q<TextField>("ABName");
            dependencyFoldout = root.Q<Foldout>("DependencyFoldout");

            var renameBt = root.Q<Button>("ABRename");
            renameBt.clicked += () =>
            {
                RenameABName();

                RefreshABNames(ref inst.infoList, abNameView);
            };

            SetupABNameListView(inst, abAssetView, abNameView);

            // assetListView,double cliick to select asset
            SetupAssetListView(inst, abAssetView, abNameView);

            ////================= buttons
            var refreshButton = root.Q<Button>("Refresh");
            refreshButton.clicked += () =>
            {
                RefreshABNames(ref inst.infoList, abNameView);
            };

            var removeUnusedButton = root.Q<Button>("RemoveUnused");
            removeUnusedButton.clicked += () =>
            {
                AssetDatabase.RemoveUnusedAssetBundleNames();
                RefreshABNames(ref inst.infoList, abNameView);
            };

            var addABNameBt = root.Q<Button>("AddABName");
            addABNameBt.clicked += () =>
            {
                AddBundleName();
            };
            var removeAbNameBt = root.Q<Button>("RemoveABName");
            removeAbNameBt.clicked += () =>
            {
                RemoveBundleName();
            };

            return root;
        }

        void RefreshABNames(ref List<AssetBundleInfo> infoList,ListView abNameView)
        {
            FindABNames(ref infoList);
            abNameView.Rebuild();
        }

        void ShowABDependencies(AssetBundleInfo info)
        {
            if(info.dependencyList.Count == 0)
                info.UpdateDependencies();

            dependencyFoldout.Clear();

            for (int i = 0; i < info.dependencyList.Count; i++)
            {
                var intentLevel = i;
                var depList = info.dependencyList[i];
                foreach (var depABName in depList)
                {
                    var label = new Label();
                    label.text = depABName;
                    label.style.marginLeft = intentLevel * 10;

                    dependencyFoldout.Add(label);
                }
            }

        }

        void ShowABName(string abName)
        {
            abNameTextField.value = abName;
            abNameTextField.userData = abName;
        }

        void RenameABName()
        {
            string oldABName = (string)abNameTextField.userData;
            var newABName = abNameTextField.value;
            AssetDatabaseTools.RenameAssetBundleName(oldABName, newABName);
        }
        void AddBundleName()
        {
            AssetDatabaseTools.AddAssetBundleName();
            RefreshABNames(ref inst.infoList, abNameView);
        }
        void RemoveBundleName()
        {
            if (!EditorUtility.DisplayDialog("Remove AssetBundle", "Are you sure to remove selected AssetBundle?", "Yes", "No"))
                return;

            var id = abNameView.selectedIndex;
            var item = inst.infoList[id];
            AssetDatabase.RemoveAssetBundleName(item.abName, true);

            RefreshABNames(ref inst.infoList, abNameView);
        }

        private void SetupABNameListView(AssetBundleEditorTool inst, ListView abAssetView, ListView abNameView)
        {
            abNameView.itemsSource = inst.infoList;
            abNameView.showAddRemoveFooter = false;

            abNameView.makeItem = () => new Label();
            abNameView.bindItem = (e, i) =>
            {
                var item = inst.infoList[i];

                if(item.dependencyList.Count == 0)
                    item.UpdateDependencies();

                var label = (e as Label);
                label.text = item.abName;
                // apply intent with dependency depth
                label.style.paddingLeft = item.DependencyDepth * 10;
            };

            abNameView.selectionChanged -= OnSelectionChanged;
            abNameView.selectionChanged += OnSelectionChanged;

            abNameView.itemsChosen += (e) =>
            {
                var item = inst.infoList[abNameView.selectedIndex];
                item.UpdateDependencies();
                foreach (var dep in item.dependencyList)
                {
                    Debug.Log("dep: " + string.Join(",", dep));
                }
            };

            //---------- events
            void OnSelectionChanged(IEnumerable<object> e)
            {
                var index = abNameView.selectedIndex;
                if (index < 0 || index >= inst.infoList.Count)
                    return;

                var item = inst.infoList[index];
                item.UpdateBundleAssets();
                abAssetView.itemsSource = item.assetPathList;

                //
                ShowABName(item.abName);

                item.UpdateDependencies();
                ShowABDependencies(item);
            }
        }

        private static void SetupAssetListView(AssetBundleEditorTool inst, ListView abAssetView, ListView abNameView)
        {
            abAssetView.itemsChosen += (e) =>
            {
                var abNameIndex = abNameView.selectedIndex;
                var assetIndex = abAssetView.selectedIndex;
                if (assetIndex < 0)
                    return;

                var item = inst.infoList[abNameIndex];
                var path = item.assetPathList[assetIndex];
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                Selection.activeObject = obj;
            };
        }

        public override bool NeedDrawDefaultUI() => false;
        public override void DrawInspectorUI(AssetBundleEditorTool inst) { }

        public static void FindABNames(ref List<AssetBundleInfo> infoList)
        {
            infoList.Clear();

            var names = AssetDatabase.GetAllAssetBundleNames();
            var list = infoList;
            names.ForEach(n =>
            {
                var info = new AssetBundleInfo();
                list.Add(info);
                info.abName = n;
                info.assetPathList.AddRange(AssetDatabase.GetAssetPathsFromAssetBundle(n));
            });
        }
    }

    /// <summary>
    /// Unity AssetBundleEditorTools 
    /// </summary>
    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Project/AssetBundleEditorTool",isUseUIElment =true)]
    [SOAssetPath("Assets/PowerUtilities/AssetBundleEditorTool.asset")]
    public class AssetBundleEditorTool : ScriptableObject
    {
        public string uxmlName = "AssetBundleEditorTool_Layout.uxml";
        public List<AssetBundleInfo> infoList = new();
    }
}
#endif