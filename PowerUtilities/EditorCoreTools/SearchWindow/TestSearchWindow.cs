namespace PowerUtilities
{
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
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), new StringListSearchProvider()
                {
                    onSetIndex = (name) =>
                    {
                        selectedItem = name;
                        Debug.Log(name);
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
