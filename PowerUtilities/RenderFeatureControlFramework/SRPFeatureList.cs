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

        public override void DrawInspectorUI(SRPFeatureList inst)
        {
            DrawDefaultGUI();

            isFold = EditorGUILayout.Foldout(isFold, "Passes",true);
            if (isFold)
            {
                for (int i = 0; i < inst.settingList.Count; i++)
                {
                    var feature = inst.settingList[i];
                    
                    EditorGUILayout.BeginHorizontal("Box");
                    {
                        var text = $"{i},{feature.name}";
                        EditorGUILayout.LabelField(text);
                        if (!feature.enabled)
                        {
                            EditorGUITools.DrawColorUI(() =>
                            {
                                EditorGUILayout.ObjectField(feature, feature.GetType(), allowSceneObjects: false, GUILayout.Height(18));

                            },Color.gray, GUI.color);
                        }
                        else
                        {
                            EditorGUILayout.ObjectField(feature, feature.GetType(), allowSceneObjects: false,GUILayout.Height(18));
                        }

                    }
                    EditorGUILayout.EndHorizontal();
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
