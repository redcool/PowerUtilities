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


        [MenuItem(SaveSkinnedBonesWindow.MENU_PATH+"/Tools/Skinned Mesh Renderer Bones Info")]
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
        /// <summary>
        /// Draw skinned mesh bones hierarchy,
        /// </summary>
        /// <param name="skinned"></param>
        /// <param name="bonePaths"></param>
        /// <param name="boneDepths"></param>
        /// <param name="scrollPosition"></param>
        /// <param name="diffSkinned"> show diff hierarchy</param>
        public static void DrawBoneInfos(SkinnedMeshRenderer skinned,string[] bonePaths,int[] boneDepths,ref Vector2 scrollPosition,SkinnedMeshRenderer diffSkinned = null)
        {
            EditorGUILayout.HelpBox($"{skinned} bones {skinned.bones.Length}", MessageType.Info);

            var bones = skinned.bones;

            var indent = EditorGUI.indentLevel;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            Undo.RecordObject(skinned, "Before change "+skinned.name);
            for (int i = 0; i < skinned.bones.Length; i++)
            {
                var bonePath = bonePaths[i];
                var boneTr = bones[i];

                EditorGUI.indentLevel = boneDepths[i];

                GUI.color = boneTr ? Color.white : Color.red;
                EditorGUITools.BeginHorizontalBox(() =>
                {
                    GUILayout.Label(i.ToString(), GUILayout.Width(20));
                    bones[i] = (Transform)EditorGUILayout.ObjectField(boneTr, typeof(Transform), true);

                    // show diff
                    if (diffSkinned)
                    {
                        var diffBone = diffSkinned.bones[i];
                        if (diffBone)
                        {
                            EditorGUILayout.SelectableLabel($"{diffBone.name}",GUILayout.Height(18));
                        }
                    }
                });
            }
            EditorGUILayout.EndScrollView();
            EditorGUI.indentLevel = indent;
            //reaasign again
            skinned.bones = bones;


            GUI.color = Color.white;
        }

        public static void DrawBoneWeigth1Info(SkinnedMeshRenderer skinned)
        {
            var mesh = skinned.sharedMesh;
            var boneWeights = mesh.GetAllBoneWeights();
            var boneWeightInfos = mesh.GetBoneWeight1_InfoPerVertex();
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                var info = boneWeightInfos[i];
                EditorGUILayout.LabelField($"vertex:{i},start:{info.start},count:{info.count}");
                EditorGUI.indentLevel++;
                for (int boneId = 0; boneId < info.count; boneId++)
                {
                    var weight = boneWeights[boneId + info.start];
                    EditorGUILayout.LabelField($"boneId:{boneId},weights:{weight.weight}");
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
#endif