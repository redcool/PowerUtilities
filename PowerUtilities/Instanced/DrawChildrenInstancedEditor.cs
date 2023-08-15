#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(DrawChildrenInstanced))]
    public class DrawChildrenInstancedEditor : Editor
    {
        SerializedObject drawInfoSerailizedObject;

        Editor drawInfoEditor;
        bool isChildrenActive;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var inst = target as DrawChildrenInstanced;

            DrawExistNew(inst);

            if (!inst.drawInfoSO)
                return;

            DrawInstancedInfoGroup(inst);

            // buttons
            DrawButtons(inst);
            DrawButtons2(inst);
        }

        private void DrawButtons(DrawChildrenInstanced inst )
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Bake Children"))
            {
                BakeChildren(inst);
            }

            if(GUILayout.Button("Bake Materials") && EditorUtility.DisplayDialog("Warning","Create new Materials and use it?","yes"))
            {
                CreateInstancedMaterials(inst.drawInfoSO.groupList);
            }

            var setChildenText = isChildrenActive ? "Disable" : "Enable";
            if (GUILayout.Button(setChildenText +" Children"))
            {
                isChildrenActive = !isChildrenActive;
                inst.gameObject.SetChildrenActive(isChildrenActive);
            }
            if (GUILayout.Button("Delete Children"))
            {
                inst.gameObject.DestroyChildren<MeshRenderer>(true);
            }

            if (GUILayout.Button("Select All"))
            {
                var objs = inst.drawInfoSO.renders.Select(r => r.gameObject).ToArray();
                Selection.objects = objs;
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawButtons2(DrawChildrenInstanced inst)
        {
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Lightmaps 2DArray"))
            {
                var sceneFolder = AssetDatabaseTools.CreateGetSceneFolder();
                EditorTextureTools.Create2DArray(inst.drawInfoSO.lightmaps.ToList(), $"{sceneFolder}/LightmapArray.asset");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawInstancedInfoGroup(DrawChildrenInstanced inst)
        {
            if (drawInfoEditor == null || drawInfoEditor.target != inst.drawInfoSO)
            {
                drawInfoEditor = CreateEditor(inst.drawInfoSO);
            }

            if (drawInfoSerailizedObject == null)
                drawInfoSerailizedObject = new SerializedObject(inst.drawInfoSO);

            drawInfoSerailizedObject.Update();

            // show default gui
            drawInfoEditor.OnInspectorGUI();

            var isApplied = drawInfoSerailizedObject.ApplyModifiedProperties();
        }

        private static void DrawExistNew(DrawChildrenInstanced inst)
        {
            if (inst.drawInfoSO)
                return;

            var sceneFolder = AssetDatabaseTools.CreateGetSceneFolder();
            var soName = inst.transform.GetHierarchyPath((Transform)null, "_");
            var soPath = $"{sceneFolder}/{soName}.asset";
            // 1 find exist profile
            var existProfile = AssetDatabase.LoadAssetAtPath<DrawChildrenInstancedSO>(soPath);
            if (existProfile && GUILayout.Button($"Found Exist Profile,use it ?"))
            {
                inst.drawInfoSO = AssetDatabase.LoadAssetAtPath<DrawChildrenInstancedSO>(soPath);
            }

            //2 create a new profile
            if (GUILayout.Button("Create New Profile"))
            {
                inst.drawInfoSO = CreateNewProfile(inst, soName, soPath, existProfile);
            }

        }

        private static void BakeChildren(DrawChildrenInstanced inst)
        {
            inst.drawInfoSO.Clear();
            inst.drawInfoSO.SetupChildren(inst.gameObject);

            EditorUtility.SetDirty(inst.drawInfoSO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static DrawChildrenInstancedSO CreateNewProfile(DrawChildrenInstanced inst,string soName, string soPath,bool isExist)
        {
            if(isExist && !EditorUtility.DisplayDialog("Waring","found exist profile ,create new of use exist profile?","new","use exist"))
            {
                inst.drawInfoSO = AssetDatabase.LoadAssetAtPath<DrawChildrenInstancedSO>(soPath);
                return inst.drawInfoSO;
            }

            var profile = CreateInstance<DrawChildrenInstancedSO>();
            profile.name = soName;
            return SaveAndGetSO(profile, soPath,isExist);
        }

        // save to xx.unity's folder,name is inst's name
        private static DrawChildrenInstancedSO SaveAndGetSO(DrawChildrenInstancedSO drawInfoSO,string soPath,bool isExist)
        {
            if(isExist)
                AssetDatabase.DeleteAsset(soPath);

            AssetDatabase.CreateAsset(drawInfoSO, soPath);
            return AssetDatabase.LoadAssetAtPath<DrawChildrenInstancedSO>(soPath);
        }

        public void CreateInstancedMaterials(List<InstancedGroupInfo> groupList)
        {
            var sceneFolder = AssetDatabaseTools.CreateGetSceneFolder();
            var matFolder = AssetDatabaseTools.CreateFolder(sceneFolder, "InstancedMaterials", true);
            AssetDatabaseTools.DeleteAsset(matFolder,true);

            groupList.ForEach(g =>
            {
                var matName = g.mat.name;
                var removedLen = matName.Contains(" (Instance)") ? 11 : 0;
                var path = $"{matFolder}/{matName.Substring(0, matName.Length-removedLen)}.mat";

                var targetMat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!targetMat)
                {
                    AssetDatabase.CreateAsset(Instantiate(g.mat), path);
                    targetMat= AssetDatabase.LoadAssetAtPath<Material>(path);
                }
                // use material reference
                g.mat = targetMat;
            });
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif
