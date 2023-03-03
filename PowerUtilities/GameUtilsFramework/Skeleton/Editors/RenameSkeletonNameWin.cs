#if UNITY_EDITOR
using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace GameUtilsFramework {
    public class RenameSkeletonNameWin : EditorWindow
    {
        GameObject go;
        string findStr;
        string replaceStr;

        (string, bool) renameInfo = ("Replace Names:",true);

        [MenuItem(SaveSkinnedBonesWindow.MENU_PATH+"/RenameSkeletonNameWin")]
        static void OpenWindow()
        {
            var win = GetWindow<RenameSkeletonNameWin>();
            win.name = nameof(RenameSkeletonNameWin);
            win.Show();
        }

        private void OnGUI()
        {
            EditorGUITools.BeginHorizontalBox(() => { 
                EditorGUILayout.LabelField("Skeleton : ");
                go = (GameObject)EditorGUILayout.ObjectField(go,typeof(GameObject),true);
            });

            EditorGUITools.BeginDisableGroup(!go, () => {
                EditorGUITools.DrawFoldContent(ref renameInfo,() => {
                    findStr = EditorGUITools.LabelTextField(EditorGUITools.TempContent("Find"), findStr);
                    replaceStr = EditorGUITools.LabelTextField(EditorGUITools.TempContent("Replace"), replaceStr);
                    if (GUILayout.Button("Rename"))
                    {
                        RenameChildren();
                    }
                });
            });
        }

        void RenameChildren()
        {
            if (string.IsNullOrEmpty(findStr) || string.IsNullOrEmpty(replaceStr))
                return;

            var trs = go.GetComponentsInChildren<Transform>();
            trs.ForEach(t => { 
                t.name = t.name.Replace(findStr, replaceStr);
            });
        }
    }
}
#endif