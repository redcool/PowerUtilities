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
    VisualElement treeInstance;

    public UIElementEventSO treeEventSO;
    UIElementEventSO lastTreeEventSO;


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

        DrawHeader();
    }

    private void DrawHeader()
    {
        var lastIsFold = isFold;
        isFold = EditorGUILayout.Foldout(lastIsFold, EditorGUITools.TempContent("TreeAsset"), true);
        if (isFold)
        {
            EditorGUILayout.BeginHorizontal(EditorStylesEx.box);

            EditorGUILayout.LabelField("Uxml",GUILayout.Width(60));
            treeAsset = EditorGUILayout.ObjectField(treeAsset, typeof(VisualTreeAsset), true) as VisualTreeAsset;

            EditorGUILayout.LabelField("Event SO",GUILayout.Width(60));
            treeEventSO = EditorGUILayout.ObjectField(treeEventSO, typeof(UIElementEventSO), true) as UIElementEventSO;

            EditorGUILayout.EndHorizontal();

            if (lastTreeAsset != treeAsset || lastIsFold != isFold)
            {
                CreateGUI();
            }

            if (lastIsFold != isFold || lastTreeAsset != treeAsset)
            {
                AddTreeEvents();
            }
        }
    }

    private void Update()
    {
        MoveTreeView();
        
    }

    private void CreateGUI()
    {
        if (!treeAsset)
            return;

        lastTreeAsset = treeAsset;

        rootVisualElement.Clear();

        treeInstance = treeAsset.CloneTree();
        rootVisualElement.Add(treeInstance);

        MoveTreeView();
    }

    private void MoveTreeView()
    {
        if (treeInstance == null)
            return;

        var pos = Vector2.zero;
        pos.y = 20;
        if (isFold)
        {
            pos.y += 20;
        }
        treeInstance.transform.position = pos;
    }

    void AddTreeEvents()
    {
        if (treeInstance == null || !treeEventSO)
            return;

        //treeInstance.Query<Button>().Build().ForEach(button => {
        //    treeEventSO.TestButton(button);
        //});

        treeEventSO.AddEvent(treeInstance);

        Debug.Log("ADD Events");
    }
}
#endif