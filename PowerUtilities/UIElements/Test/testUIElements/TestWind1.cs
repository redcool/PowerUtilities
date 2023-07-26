#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using PowerUtilities;
using System.Linq;
using System.IO;

public class TestWind1 : EditorWindow
{
    [MenuItem("PowerUtilities/UI Toolkit/TestWind1")]
    public static void ShowExample()
    {
        TestWind1 wnd = GetWindow<TestWind1>();
        wnd.titleContent = new GUIContent("TestWind1");
    }

    public void CreateGUI()
    {
        var fileGUID = AssetDatabase.FindAssets("TestWind1").FirstOrDefault();
        var folder = Path.GetDirectoryName( AssetDatabase.GUIDToAssetPath(fileGUID)); // Assets/testUIElements

        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(folder+"/TestWind1.uxml");
        visualTree.CloneTree(root);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(folder+"/TestWind1.uss");
        //root.styleSheets.Add(styleSheet);
        
    }
}
#endif