namespace GameUtilsFramework
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using PowerUtilities;

#if UNITY_EDITOR
    using UnityEditor;

    /// <summary>
    /// 
    /// </summary>
    public class SaveSkinnedBonesWindow : EditorWindow
    {

        string helpStr = @"
            Make SkinnedMeshRenderers in Hierarchy to EquipPart
        ";

        string rootBoneName = "Root";
        string savePath = "Assets/SkinnedParts";

        public const string MENU_PATH = "PowerUtilities/SkinnedMesh";

        [MenuItem(MENU_PATH+"/Equips/Save SkinnedBones Window")]
        static void Init()
        {
            var win = EditorWindow.GetWindow<SaveSkinnedBonesWindow>();
            win.titleContent = new GUIContent(nameof(SaveSkinnedBonesWindow));
            win.Show();
        }
        
        int Save()
        {
            var prefabDirPath = $"{savePath}/Prefabs";
            var dataDirPath = $"{savePath}/Dats";

            PathTools.CreateAbsFolderPath(savePath);
            PathTools.CreateAbsFolderPath(prefabDirPath);
            PathTools.CreateAbsFolderPath(dataDirPath);

            var skins = SelectionTools.GetSelectedChildrenComponents<SkinnedMeshRenderer>(true);
            foreach (var skin in skins)
            {
                var prefabPath = $"{prefabDirPath}/{skin.name}.prefab";
                // save prefab
                var skinInst = Object.Instantiate(skin);
                skinInst.gameObject.SetActive(true);
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

            EditorGUILayout.HelpBox(helpStr, MessageType.Info);

            GUILayout.BeginVertical("Box");

            rootBoneName = EditorGUILayout.TextField(nameof(rootBoneName),rootBoneName);
            savePath = EditorGUILayout.TextField(nameof(savePath), savePath);

            if(Selection.count == 0)
            {
                EditorGUITools.DrawColorLabel("* Select SkinnedMeshRenderers in Hierarchy",Color.red);
            }

            EditorGUITools.BeginDisableGroup(Selection.count==0, () => {
                if (GUILayout.Button("Save selection Skinneds"))
                {
                    var length = Save();

                    if (length == 0)
                    {
                        EditorUtility.DisplayDialog("Warning!", "nothing selected ", "ok");
                    }
                }
                if (GUILayout.Button("Attach EquipPartControl to RootNode?"))
                {
                    var rootGo = Selection.activeGameObject.transform.root.gameObject;
                    GameObjectTools.GetOrAddComponent<EquipmentPartControl>(rootGo);
                    Selection.activeGameObject = rootGo;
                }
            });

            GUILayout.EndVertical();
        }

        private void OnSelectionChange()
        {
            Repaint();
        }
    }

#endif


}
