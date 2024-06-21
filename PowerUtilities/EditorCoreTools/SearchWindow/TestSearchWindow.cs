namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
#endif
    using UnityEngine;

#if UNITY_EDITOR
    [CustomEditor(typeof(TestSearchWindow))]
    public class TestSearchWindowEditor : Editor
    {
        public string selectedItem = "no";
        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Selected Item:");
            var isClicked = GUILayout.Button(selectedItem, EditorStyles.popup);
            GUILayout.EndHorizontal();
            if (isClicked)
            {
                SearchWindowTools.OpenSearchWindow( new StringListSearchProvider<object>()
                {
                    windowTitle = "stringList",
                    itemList = new()
                    {
                        new (){name="a/b/1",userData= 1 },
                        new (){name="a/b/2",userData= 2 },
                        new() { name = "a/1", userData = 3 },
                    },
                    
                    onSelectedChanged = ((string name,object userData)infoItem) =>
                    {
                        selectedItem = infoItem.name +":"+ infoItem.userData;
                        Debug.Log(selectedItem);
                    }
                });
            }


        }
    }
#endif

    public class TestSearchWindow : MonoBehaviour
    {

    }
}
