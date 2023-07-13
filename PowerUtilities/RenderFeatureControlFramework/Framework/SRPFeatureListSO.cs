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
    [CustomEditor(typeof(SRPFeatureListSO))]
    public class SRPFeatureListEditor : PowerEditor<SRPFeatureListSO>
    {
        List<SerializedObject> featureSOList;

        public override void DrawInspectorUI(SRPFeatureListSO inst)
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();

            TryInitFeatureSOList(inst);

            if (featureSOList.Count == 0)
            {
                EditorGUILayout.SelectableLabel("No Features");
                return;
            }

            var isDetailsFoldout = serializedObject.FindProperty("isDetailsFoldout");

            isDetailsFoldout.boolValue = EditorGUILayout.Foldout(isDetailsFoldout.boolValue, "Details", true);
            if (isDetailsFoldout.boolValue)
            {
                //EditorGUILayout.BeginVertical("Box");
                EditorGUI.indentLevel++;
                for (int i = 0; i < featureSOList.Count; i++)
                {
                    var featureSO = featureSOList[i];
                    var feature = (SRPFeature) featureSO.targetObject;

                    var color = feature.enabled ? (feature.interrupt ? Color.red : GUI.color) : Color.gray;
                    var title = feature.name;
                    var foldoutProp = featureSOList[i].FindProperty(nameof(SRPFeature.isFoldout));
                    PassDrawer.DrawPassDetail(featureSOList[i], color, foldoutProp, EditorGUITools.TempContent(title));
                }
                EditorGUI.indentLevel--;
                //EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void TryInitFeatureSOList(SRPFeatureListSO inst)
        {
            if (EditorGUI.EndChangeCheck() || featureSOList == null || featureSOList.Count != inst.featureList.Count)
            {
                featureSOList = inst.featureList
                    .Where(item => item)
                    .Select(feature => new SerializedObject(feature))
                    .ToList();
            }
        }
    }
#endif
    [Serializable]
    [CreateAssetMenu(menuName = SRPFeature.SRP_FEATURE_MENU + "/SRPFeatureList")]
    public class SRPFeatureListSO : ScriptableObject
    {
        [Tooltip("dont run featureList when disable")]
        public bool enabled = true;

        public List<SRPFeature> featureList = new List<SRPFeature>();

        [SerializeField]
        [HideInInspector]
        bool isDetailsFoldout; // feature details folded?


        public static SRPFeatureListSO instance; // last instance

        public T GetFeature<T>(string featureName) where T : SRPFeature
        {
            if (string.IsNullOrEmpty(featureName))
                return default;

            return (T)featureList.Find(f => f.name == featureName);
        }

        public List<T> GetFeatures<T>() where T : SRPFeature
        {
            return featureList.FindAll(f => f.GetType() == typeof(T))
                .Select(f=> (T)f)
                .ToList();
        }
    }
}
