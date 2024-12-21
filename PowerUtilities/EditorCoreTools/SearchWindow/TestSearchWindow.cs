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
    using UnityEngine.Experimental.Rendering;

#if UNITY_EDITOR
    [CustomEditor(typeof(TestSearchWindow))]
    public class TestSearchWindowEditor : Editor
    {
        public string selectedItem = "no";
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Selected Item:",GUILayout.Width(EditorGUIUtility.labelWidth-4));
            var isClicked = GUILayout.Button(selectedItem, EditorStyles.popup);
            GUILayout.EndHorizontal();

            if (isClicked)
            {
                var provider = SearchWindowTools.CreateProvider<StringListSearchProvider>();

                provider.windowTitle = "stringList";
                provider.itemList = new()
                {
                    new (){name="a/b/1",userData= 1 },
                    new (){name="a/b/2",userData= 2 },
                    new() { name = "a/1", userData = 3 },
                };

                provider.onSelectedChanged = ((string name, object userData) infoItem) =>
                {
                    selectedItem = infoItem.name + ":" + infoItem.userData;
                    Debug.Log(selectedItem);
                };
                SearchWindowTools.OpenSearchWindow(provider);
            }

        }
    }
#endif

    public class TestSearchWindow : MonoBehaviour
    {
        [EnumSearchable(typeof(GraphicsFormat), textFileName = "")]
        public GraphicsFormat gFormat;

        [StringListSearchable(names ="a,b,c")]
        public string textName;
    }
}
