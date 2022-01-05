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

            DrawProfileUI(inst);

            var isApplied = drawInfoSerailizedObject.ApplyModifiedProperties();
            if (isApplied)
            {
                inst.drawInfoSO.UpdateGroupListMaterial(inst.drawInfoSO.enableLightmap);
            }

            if (GUILayout.Button("Bake Children Gos"))
            {
                inst.drawInfoSO.Clear();
                inst.drawInfoSO.SetupChildren(inst.gameObject, inst.GetLevelId());
                if (inst.drawInfoSO.destroyGameObjectWhenCannotUse && !EditorUtility.DisplayDialog("Warning", "删除所有的子节点吗?", "no", "ok"))
                {
                    inst.drawInfoSO.DestroyOrHiddenChildren(true);
                }
                AssetDatabase.Refresh();
            }
        }

        private static void LoadFromFile(DrawChildrenInstanced inst, string soPath)
        {
            inst.drawInfoSO = AssetDatabase.LoadAssetAtPath<DrawChildrenInstancedSO>(soPath);

        }

        private void DrawProfileUI(DrawChildrenInstanced inst)
        {
            GUILayout.BeginVertical("Box");
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty(nameof(inst.drawInfoSO.lightmaps)));
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty(nameof(inst.drawInfoSO.enableLightmap)));
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty(nameof(inst.drawInfoSO.destroyGameObjectWhenCannotUse)));
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty(nameof(inst.drawInfoSO.culledUnderLevel2)));
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty(nameof(inst.drawInfoSO.culledRatio)));
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty(nameof(inst.drawInfoSO.forceRefresh)));
            EditorGUILayout.PropertyField(drawInfoSerailizedObject.FindProperty(nameof(inst.drawInfoSO.groupList)));
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
