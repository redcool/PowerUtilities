#if UNITY_EDITOR
using PowerUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PowerEditorWindow : EditorWindow
{
    bool isFold;
    public VisualTreeAsset treeAsset;
    VisualTreeAsset lastTreeAsset;

    VisualElement treeVInstance;

    public const string ROOT_MENU = "PowerUtilities/Window";
    [MenuItem(ROOT_MENU+"/UxmlWindow")]
    public static void InitWindow()
    {
        var w = GetWindow<PowerEditorWindow>();
        w.titleContent = EditorGUITools.TempContent("PowerEditorWin");
    }

    private void OnGUI()
    {
        //if (treeAsset)
        //    return;

        isFold = EditorGUILayout.Foldout(isFold, EditorGUITools.TempContent("TreeAsset"), true);
        if (isFold)
        {
            EditorGUILayout.BeginVertical(EditorStylesEx.box);
            treeAsset = EditorGUILayout.ObjectField(treeAsset, typeof(VisualTreeAsset), true) as VisualTreeAsset;
            EditorGUILayout.EndVertical();

            if (treeAsset != null && lastTreeAsset != treeAsset)
            {
                CreateGUI();
            }
        }
        MoveTreeView();
    }

    private void CreateGUI()
    {
        if (!treeAsset)
            return;

        lastTreeAsset = treeAsset;

        rootVisualElement.Clear();

        treeVInstance = treeAsset.CloneTree();
        rootVisualElement.Add(treeVInstance);

        MoveTreeView();
    }

    private void MoveTreeView()
    {
        if (treeVInstance == null)
            return;

        var pos = Vector2.zero;
        pos.y = 20;
        if (isFold)
        {
            pos.y += 20;
        }
        treeVInstance.transform.position = pos;
    }
}
#endif