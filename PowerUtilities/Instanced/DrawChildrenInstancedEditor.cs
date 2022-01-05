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

    //[CustomEditor(typeof(DrawChildrenInstanced))]
    public class DrawChildrenInstancedEditor : Editor
    {
        SerializedObject drawInfoSerailizedObject;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var inst = target as DrawChildrenInstanced;


            if (!inst.drawInfoSO)
            {
                // 1 find exist profile
                var sceneFolder = AssetDatabaseTools.CreateFolderSameNameAsScene();
                var soName = inst.transform.GetHierarchyPath(null, "_");
                var soPath = $"{sceneFolder}/{soName}.asset";
                LoadFromFile(inst, soPath);

                //2 create a new profile
                if (!inst.drawInfoSO)
                {
                    inst.drawInfoSO = ScriptableObject.CreateInstance<DrawChildrenInstancedSO>();
                    inst.drawInfoSO.name = soName;
                    inst.drawInfoSO = SaveAndGetSO(inst.drawInfoSO, soPath);
                }
            }

            if (drawInfoSerailizedObject == null)
                drawInfoSerailizedObject = new SerializedObject(inst.drawInfoSO);

            drawInfoSerailizedObject.Update();

            DrawProfileUI();

            var isApplied = drawInfoSerailizedObject.ApplyModifiedProperties();
            if (isApplied)
            {
                inst.drawInfoSO.UpdateGroupListMaterial(inst.drawInfoSO.enableLightmap);
            }

            if (GUILayout.Button("Bake Children Gos"))
            {
                if (inst.drawInfoSO.destroyGameObjectWhenCannotUse)
                {
                    //if (EditorUtility.DisplayDialog("Warning", "烘焙后不可编辑,继续吗?", "ok"))
                    {
                        inst.drawInfoSO.SetupChildren(inst.gameObject, inst.GetLevelId());
                    }
                }
            }
        }

        private static void LoadFromFile(DrawChildrenInstanced inst, string soPath)
        {
            inst.drawInfoSO = AssetDatabase.LoadAssetAtPath<DrawChildrenInstancedSO>(soPath);

        }

        private void DrawProfileUI()
        {
            GUILayout.BeginVertical("Box");
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty("lightmaps"));
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty("enableLightmap"));
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty("destroyGameObjectWhenCannotUse"));
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty("culledUnderLevel2"));
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty("forceRefresh"));
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty("groupList"));
            GUILayout.EndVertical();
        }

        // save to xx.unity's folder,name is inst's name
        private static DrawChildrenInstancedSO SaveAndGetSO(DrawChildrenInstancedSO drawInfoSO,string soPath)
        {
            AssetDatabase.DeleteAsset(soPath);

            AssetDatabase.CreateAsset(drawInfoSO, soPath);
            return AssetDatabase.LoadAssetAtPath<DrawChildrenInstancedSO>(soPath);
        }


    }
}
#endif
