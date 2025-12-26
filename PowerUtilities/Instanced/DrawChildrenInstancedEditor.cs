#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{

    [CustomEditor(typeof(DrawChildrenInstanced))]
    public class DrawChildrenInstancedEditor : PowerEditor<DrawChildrenInstanced>
    {
        GUIContent guiBakeChildren = new GUIContent("BakeChildren", "record children meshRenderers instanced info,then use gpu instance rendering them")
            , guiBakeMaterials = new GUIContent("BakeMaterial","clone children's shaderMaterial to sceneFolder/Materials")
            ,guiRenderActiveSwitch = new GUIContent("EnableRenders","switch children renderer active")
            ,guiDeleteChildren = new GUIContent("DeleteChildren","delete children renderers")
            ,guiSelectAll = new GUIContent("SelectAll","select children gameobjects")
            ;

        bool isActive;

        public override string Version => "v0.0.3";

        public override bool NeedDrawDefaultUI() => true;

        public override void DrawInspectorUI(DrawChildrenInstanced inst)
        {
            if (!inst.drawInfoSO)
                return;

            DrawButtons(inst);
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
                //inst.drawInfoSO.SetupRenderers(inst.gameObject);
                inst.drawInfoSO.SetRendersActive(isActive);
            }

            if (GUILayout.Button(guiSelectAll))
            {
                //inst.drawInfoSO.SetupRenderers(inst.gameObject);
                var objs = inst.drawInfoSO.renders.Select(r => r.gameObject).ToArray();
                Selection.objects = objs;
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void BakeChildren(DrawChildrenInstanced inst)
        {
            inst.SetupDrawInfo();

            EditorUtility.SetDirty(inst.drawInfoSO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
            AssetDatabaseTools.SaveRefresh();
        }

    }
}
#endif
