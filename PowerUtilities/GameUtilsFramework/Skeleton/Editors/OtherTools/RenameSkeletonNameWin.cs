#if UNITY_EDITOR
using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace GameUtilsFramework {
    public class RenameSkeletonNameWin : EditorWindow
    {
        GameObject go;
        string findStr;
        string replaceStr;

        (string, bool) renameInfo = ("Replace Names:",true);

        [MenuItem(SaveSkinnedBonesWindow.MENU_PATH+"/Tools/Rename Skeleton Name Win")]
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
            if (string.IsNullOrEmpty(findStr))
                return;

            var gos = go.GetComponentsInChildren<Transform>()
                .Select(tr => tr.gameObject)
                .ToArray();

            Undo.RecordObjects(gos, "Before Rename "+replaceStr);
            gos.ForEach(t => { 
                t.name = t.name.Replace(findStr, replaceStr);
            });
        }
    }
}
#endif