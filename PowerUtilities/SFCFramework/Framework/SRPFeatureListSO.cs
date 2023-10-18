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
    using System.IO;
    using Object = UnityEngine.Object;
    using System.Reflection;

    [CustomEditor(typeof(SRPFeatureListSO))]
    public class SRPFeatureListEditor : PowerEditor<SRPFeatureListSO>
    {
        List<SerializedObject> featureSOList;

        /// <summary>
        /// SRPFeatureListSO asset's folder in Assets
        /// </summary>
        string targetObjectAssetDir;
        IEnumerable<Type> srpFeatureTypes;

        GenericMenu createPassMenu;

        private void OnEnable()
        {
            var inst = serializedObject.targetObject as SRPFeatureListSO;
            targetObjectAssetDir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(serializedObject.targetObject));
            srpFeatureTypes = ReflectionTools.GetTypesDerivedFrom<SRPFeature>();

            createPassMenu = new GenericMenu();
            SetupMenu(srpFeatureTypes, createPassMenu, targetObjectAssetDir, inst);
        }

        public override void DrawInspectorUI(SRPFeatureListSO inst)
        {
            serializedObject.UpdateIfRequiredOrScript();

            /**
                show list
             */
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck() || featureSOList == null || featureSOList.Count != inst.featureList.Count)
                TryInitFeatureSOList(inst);

            if (featureSOList.Count == 0)
            {
                EditorGUILayout.SelectableLabel("No Features");
                return;
            }

            /**
                show details
             */
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
                    var foldoutProp = featureSO.FindProperty(nameof(SRPFeature.isFoldout));
                    // draw pass details
                    EditorGUI.BeginChangeCheck();
                    PassDrawer.DrawPassDetail(featureSO, color, foldoutProp, EditorGUITools.TempContent(title, feature.Tooltip));
                    if (EditorGUI.EndChangeCheck())
                    {
                        feature.DestroyPassInstance();
                    }
                }
                EditorGUI.indentLevel--;
                //EditorGUILayout.EndVertical();
            }
            /**
                create pass asset 
             */
            if (GUILayout.Button("Add SFC Pass"))
            {
                createPassMenu.ShowAsContext();
            }
            serializedObject.ApplyModifiedProperties();
        }
        public static void SetupMenu(IEnumerable<Type> featureTypes, GenericMenu menu, string curSODir, SRPFeatureListSO inst)
        {
            foreach (var passType in featureTypes)
            {
                var menuContent = new GUIContent(passType.Name);

                var createAssetMenuAttr = passType.GetCustomAttribute<CreateAssetMenuAttribute>();
                if (createAssetMenuAttr != null)
                {
                    menuContent.text = createAssetMenuAttr.menuName;
                }

                menu.AddItem(menuContent, false, (passTypeObj) =>
                {
                    var passType = (Type)passTypeObj;
                    var passSO = ScriptableObject.CreateInstance(passType);
                    var passSavePath = $"{curSODir}/{inst.featureList.Count} {passType.Name}.asset";

                    var passAsset = AssetDatabaseTools.CreateAssetThenLoad<SRPFeature>(passSO, passSavePath);
                    AssetDatabaseTools.SaveRefresh();

                    inst.featureList.Add(passAsset);

                }, passType);
            }
        }

        private void TryInitFeatureSOList(SRPFeatureListSO inst)
        {
            featureSOList = inst.featureList
                .Where(item => item)
                .Select(feature => new SerializedObject(feature))
                .ToList();
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
