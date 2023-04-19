namespace PowerUtilities.RenderFeatures
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(SRPFeatureList))]
    public class SRPFeatureListEditor : PowerEditor<SRPFeatureList>
    {
        bool isFold;
        List<SerializedObject> featureSOList;

        public override void DrawInspectorUI(SRPFeatureList inst)
        {
            DrawDefaultGUI();

            if(featureSOList == null || featureSOList.Count != inst.settingList.Count)
                featureSOList = inst.settingList.Select(feature => new SerializedObject(feature)).ToList();

            isFold = EditorGUILayout.Foldout(isFold, "Passes",true);
            if (isFold)
            {
                for (int i = 0; i < inst.settingList.Count; i++)
                {
                    var feature = inst.settingList[i];
                    var color = feature.enabled ? GUI.color : Color.gray;
                    var title = feature.name;
                    var foldoutProp = featureSOList[i].FindProperty(nameof(SRPFeature.isFoldout));
                    PassDrawer.DrawPassDetail(featureSOList[i], color, foldoutProp, EditorGUITools.TempContent(title));
                }
            }
        }
    }
#endif

    [CreateAssetMenu(menuName = "SrpRenderFeatures/SRPSettingList")]
    public class SRPFeatureList : ScriptableObject
    {
        public List<SRPFeature> settingList = new List<SRPFeature>();
    }
}
