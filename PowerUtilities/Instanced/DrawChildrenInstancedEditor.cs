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
        GUIContent guiBakeChildren = new GUIContent("BakeChildren", "record children meshRenderers instanced info,then use gpu instance rendering them")
            , guiBakeMaterials = new GUIContent("BakeMaterial","clone children's shaderMaterial to sceneFolder/Materials")
            ,guiRenderActiveSwitch = new GUIContent("EnableRenders","switch children renderer active")
            ,guiDeleteChildren = new GUIContent("DeleteChildren","delete children renderers")
            ,guiSelectAll = new GUIContent("SelectAll","select children gameobjects")
            ;

        SerializedObject drawInfoSerailizedObject;

        Editor drawInfoEditor;
        bool isActive;

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
            if (GUILayout.Button(guiBakeChildren))
            {
                BakeChildren(inst);
            }

            if(GUILayout.Button(guiBakeMaterials) && EditorUtility.DisplayDialog("Warning","Create new Materials and use it?","yes"))
            {
                CreateInstancedMaterials(inst.drawInfoSO.groupList);
            }

            var setChildenText = isActive ? "DisableRenders" : "EnableRenders";
            guiRenderActiveSwitch.text = setChildenText;
            if (GUILayout.Button(guiRenderActiveSwitch))
            {
                isActive = !isActive;
                //inst.gameObject.SetChildrenActive(isChildrenActive);
                inst.drawInfoSO.SetupRenderers(inst.gameObject);
                inst.drawInfoSO.SetRendersActive(isActive);
            }
            if (GUILayout.Button(guiDeleteChildren))
            {
                inst.gameObject.DestroyChildren<MeshRenderer>(true);
            }

            if (GUILayout.Button(guiSelectAll))
            {
                inst.drawInfoSO.SetupRenderers(inst.gameObject);
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
                var matName = g.originalMat.name;
                var removedLen = matName.Contains(" (Instance)") ? 11 : 0;
                var path = $"{matFolder}/{matName.Substring(0, matName.Length-removedLen)}.mat";

                var targetMat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!targetMat)
                {
                    AssetDatabase.CreateAsset(Instantiate(g.mat), path);
                    targetMat= AssetDatabase.LoadAssetAtPath<Material>(path);
                }
                // use material reference
                g.originalMat = targetMat;
            });
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif
