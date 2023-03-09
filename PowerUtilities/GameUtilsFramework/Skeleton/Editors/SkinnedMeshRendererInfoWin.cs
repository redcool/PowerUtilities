#if UNITY_EDITOR
using PowerUtilities;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameUtilsFramework
{
    public class SkinnedMeshRendererInfoWin : EditorWindow
    {
        SkinnedMeshRenderer skinned;
        private Vector2 scrollPosition;
        string[] bonePaths;
        int[] boneDepths;


        [MenuItem(SaveSkinnedBonesWindow.MENU_PATH+"/Skinned Mesh Renderer Bones Info")]
        static void ShowWin()
        {
            GetWindow<SkinnedMeshRendererInfoWin>().Show();
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            skinned = EditorGUITools.BeginHorizontalBox(() =>
            {
                GUILayout.Label("SkinnedMesh : ");
                return (SkinnedMeshRenderer)EditorGUILayout.ObjectField(skinned, typeof(SkinnedMeshRenderer), true);
            });
            if (EditorGUI.EndChangeCheck() || bonePaths == null)
            {
                InitBoneInfos(skinned,out bonePaths,out boneDepths);
            }

            if (!skinned)
                return;

            DrawBoneInfos(skinned, bonePaths, boneDepths, ref scrollPosition);
        }

        public static void InitBoneInfos(SkinnedMeshRenderer skinned, out string[] bonePaths,out int[] boneDepths)
        {
            bonePaths = null;
            boneDepths = null;

            if (!skinned)
                return;

            bonePaths = new string[skinned.bones.Length];
            boneDepths = new int[skinned.bones.Length];

            for (int i = 0; i < skinned.bones.Length; i++)
            {
                var path = bonePaths[i] = skinned.bones[i].GetHierarchyPath(skinned.rootBone.name);
                boneDepths[i] = path.Count(c => c == '/');
            }
        }

        public static void DrawBoneInfos(SkinnedMeshRenderer skinned,string[] bonePaths,int[] boneDepths,ref Vector2 scrollPosition)
        {
            EditorGUILayout.HelpBox($"{skinned} bones {skinned.bones.Length}", MessageType.Info);

            var bones = skinned.bones;

            var indent = EditorGUI.indentLevel;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            Undo.RecordObject(skinned, "Before change "+skinned.name);
            for (int i = 0; i < skinned.bones.Length; i++)
            {
                var boneTr = bones[i];

                EditorGUI.indentLevel = boneDepths[i];

                EditorGUITools.BeginHorizontalBox(() =>
                {
                    GUILayout.Label(i.ToString(), GUILayout.Width(20));
                    bones[i] = (Transform)EditorGUILayout.ObjectField(boneTr, typeof(Transform), true);
                });
            }
            EditorGUILayout.EndScrollView();
            EditorGUI.indentLevel = indent;
            //reaasign again
            skinned.bones = bones;
        }
    }
}
#endif