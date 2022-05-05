#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    using UnityEditor;
    using UnityEngine.SceneManagement;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    [CustomEditor(typeof(DrawChildrenInstanced))]
    public class DrawChildrenInstancedEditor : Editor
    {
        SerializedObject drawInfoSerailizedObject;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var inst = target as DrawChildrenInstanced;

            if (!inst.drawInfoSO)
            {
                var sceneFolder = AssetDatabaseTools.CreateFolderSameNameAsScene();
                var soName = inst.transform.GetHierarchyPath(null, "_");
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
                    inst.drawInfoSO = CreateNewProfile(inst,soName, soPath,existProfile);
                }
            }

            if (!inst.drawInfoSO)
                return;

            if (drawInfoSerailizedObject == null)
                drawInfoSerailizedObject = new SerializedObject(inst.drawInfoSO);

            drawInfoSerailizedObject.Update();

            DrawProfileUI(inst);

            var isApplied = drawInfoSerailizedObject.ApplyModifiedProperties();
            if (isApplied)
            {
                inst.drawInfoSO.UpdateGroupListMaterial(inst.drawInfoSO.IsLightMapEnabled());
            }

            if (GUILayout.Button("Bake Children Gos"))
            {
                inst.drawInfoSO.Clear();
                inst.drawInfoSO.SetupChildren(inst.gameObject);
                if (inst.drawInfoSO.destroyGameObjectsWhenBaked && EditorUtility.DisplayDialog("Warning", "删除所有的子节点吗?", "yes"))
                {
                    inst.drawInfoSO.DestroyOrHiddenChildren(true);
                }
                EditorUtility.SetDirty(inst.drawInfoSO);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
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



        private void DrawProfileUI(DrawChildrenInstanced inst)
        {
            GUILayout.BeginVertical("Box");
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty(nameof(inst.drawInfoSO.lightmaps)));
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty(nameof(inst.drawInfoSO.enableLightmap)));
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty(nameof(inst.drawInfoSO.destroyGameObjectsWhenBaked)));

            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty(nameof(inst.drawInfoSO.culledRatio)));
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty(nameof(inst.drawInfoSO.forceRefresh)));
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty(nameof(inst.drawInfoSO.groupList)));
            GUILayout.EndVertical();
        }

        private static void DrawProfileField(DrawChildrenInstanced inst)
        {
            GUILayout.BeginHorizontal("Box");
            EditorGUILayout.PrefixLabel(nameof(inst.drawInfoSO));
            inst.drawInfoSO  = (DrawChildrenInstancedSO)EditorGUILayout.ObjectField(inst.drawInfoSO, typeof(DrawChildrenInstancedSO),false); ;
            GUILayout.EndHorizontal();
        }

        // save to xx.unity's folder,name is inst's name
        private static DrawChildrenInstancedSO SaveAndGetSO(DrawChildrenInstancedSO drawInfoSO,string soPath,bool isExist)
        {
            if(isExist)
                AssetDatabase.DeleteAsset(soPath);

            AssetDatabase.CreateAsset(drawInfoSO, soPath);
            return AssetDatabase.LoadAssetAtPath<DrawChildrenInstancedSO>(soPath);
        }


    }
}
#endif
