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

    [CustomEditor(typeof(AssetBundleTools))]
    public class AssetBundleToolsEditor : PowerEditor<AssetBundleTools>
    {

        public override VisualElement CreateInspectorGUI()
        {
            var inst = target as AssetBundleTools;

            if (inst.infoList.Count ==0)
            {
                FindABNames(ref inst.infoList);
            }


            var items = inst.infoList;

            Func<VisualElement> makeItem = () => new Label();
            Action<VisualElement,int> bindItem = (e,i) => (e as Label).text = items[i].abName;


            var listView = new ListView(inst.infoList, 16, makeItem, bindItem)
            {
                selectionType = SelectionType.Multiple,
                showAddRemoveFooter = true,
            };

            var root = new VisualElement();
            root.Add(listView);

            return root;
        }

        public override bool NeedDrawDefaultUI() => false;
        public override void DrawInspectorUI(AssetBundleTools inst)
        {
            EditorGUILayout.HelpBox("AssetBundle Tools", MessageType.Info);

            if (GUILayout.Button("Find AssetBundle"))
            {
                FindABNames(ref inst.infoList);
            }
        }

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


    [Serializable]
    public class AssetBundleInfo
    {
        public string abName;
        [ListItemDraw("","")]
        public List<string> assetPathList = new();
    }

    /// <summary>
    /// Unity AssetBundleTools 
    /// </summary>
    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Project/AssetBundleTools",isUseUIElment =true)]
    [SOAssetPath("Assets/PowerUtilities/AssetBundleTools.asset")]
    public class AssetBundleTools : ScriptableObject
    {
        //[ListItemDraw("name:,abName,assets:,assetPathList", "50,200,50,",rowCountArrayPropName = "assetPathList")]
        public List<AssetBundleInfo> infoList = new();
    }
}
#endif