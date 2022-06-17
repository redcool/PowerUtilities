namespace GameUtilsFramework
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using PowerUtilities;

#if UNITY_EDITOR
    using UnityEditor;
    public class SaveSkinnedBonesWindow : EditorWindow
    {
        public new string name = nameof(SaveSkinnedBonesWindow);
        string rootBoneName = "Root";
        string savePath = "Assets/SkinnedParts";

        public const string PATH = "PowerUtilities/SkinnedMesh/Save SkinnedBones Window";

        [MenuItem(PATH)]
        static void Init()
        {
            var win = EditorWindow.GetWindow<SaveSkinnedBonesWindow>();
            win.Show();
        }
        
        int Save()
        {
            var prefabDirPath = $"{savePath}/Prefabs";
            var dataDirPath = $"{savePath}/Dats";

            PathTools.CreateAbsFolderPath(savePath);
            PathTools.CreateAbsFolderPath(prefabDirPath);
            PathTools.CreateAbsFolderPath(dataDirPath);

            var skins = SelectionTools.GetSelectedComponents<SkinnedMeshRenderer>();
            foreach (var skin in skins)
            {
                var prefabPath = $"{prefabDirPath}/{skin.name}.prefab";
                // save prefab
                var skinInst = Object.Instantiate(skin);
                var prefab = PrefabUtility.SaveAsPrefabAsset(skinInst.gameObject, prefabPath, out var isSaved);
                DestroyImmediate(skinInst.gameObject);
                // save bone infos

                var dataPath = $"{dataDirPath}/{skin.name}.asset";
                var boneData = ScriptableObject.CreateInstance<SkinnedBonesData>();
                boneData.name = skin.name;
                boneData.rootBonePath = skin.rootBone.transform.GetHierarchyPath(rootBoneName);
                boneData.bonesPath = skin.GetBonesPath(rootBoneName);
                boneData.skinnedPrefab = prefab.GetComponent<SkinnedMeshRenderer>();

                AssetDatabase.CreateAsset(boneData, dataPath);
            }
            return skins.Length;
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Select SkinnedMeshRenderers ", MessageType.Info);

            GUILayout.BeginVertical("Box");

            rootBoneName = EditorGUILayout.TextField(nameof(rootBoneName),rootBoneName);
            savePath = EditorGUILayout.TextField(nameof(savePath), savePath);

            if (GUILayout.Button("Save selection Skinneds"))
            {
                var length = Save();

                if (length == 0)
                {
                    EditorUtility.DisplayDialog("Warning!", "nothing selected ", "ok");
                }
            }
            GUILayout.EndVertical();
        }

    }

#endif

    public class SkinnedBonesData : ScriptableObject
    {
        public string rootBonePath;
        public string[] bonesPath;
        public SkinnedMeshRenderer skinnedPrefab;
    }
}
