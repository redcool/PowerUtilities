#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PowerUtilities
{
    public class ItemPlacementWindow : PowerEditorWindow
    {

        [MenuItem(ROOT_MENU+"/Scene/"+nameof(ItemPlacementWindow))]
        static void Init()
        {
            var win = GetWindow<ItemPlacementWindow>();
            var uxmlPath = AssetDatabaseTools.FindAssetsPath("ItemPlacementWindow", "uxml").FirstOrDefault();

            win.treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            win.CreateGUI();
            win.isHiddenCommonHeader = true;
        }


    }
}
#endif