namespace PowerUtilities.RenderFeatures
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using System.IO;
    using Object = UnityEngine.Object;
    using System.Reflection;
    using UnityEngine.Rendering;


#if UNITY_EDITOR
    [CustomEditor(typeof(SRPFeatureListSO))]
    public class SRPFeatureListEditor : PowerEditor<SRPFeatureListSO>
    {
        List<SerializedObject> featureSOList;

        IEnumerable<Type> srpFeatureTypes;

        //GenericMenu createPassMenu;

        //{typeName for display, type : for userData}
        List<(string featureName,object featureType)> featureNameTypeList = new();

        private void OnEnable()
        {
            // setup create sfc pass menu items
            var inst = serializedObject.targetObject as SRPFeatureListSO;
            srpFeatureTypes = ReflectionTools.GetTypesDerivedFrom<SRPFeature>();

            //createPassMenu = CreateAddSFCPassMenu(srpFeatureTypes, inst);
            SetupFreatureNameList();
        }

        private void SetupFreatureNameList()
        {
            featureNameTypeList.Clear();
            foreach (var type in srpFeatureTypes)
            {
                featureNameTypeList.Add((type.Name,type));
            }
        }

        public override void DrawInspectorUI(SRPFeatureListSO inst)
        {
            serializedObject.UpdateIfRequiredOrScript();

            /**
                1 show pass list
             */
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck() || featureSOList == null || featureSOList.Count != inst.featureList.Count)
                TryInitFeatureSOList(inst);

            if (featureSOList.Count == 0)
            {
                EditorGUILayout.SelectableLabel("No Features");
            }
            else
            {
                DrawDetails();
            }
            
            /**
                2 create pass asset menus
             */
            DrawAddPassButton();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAddPassButton()
        {
            if (GUILayout.Button("Add SFC Pass"))
            {
                var inst = serializedObject.targetObject as SRPFeatureListSO;
                //createPassMenu.ShowAsContext();
                var provider = SearchWindowTools.CreateProvider<StringListSearchProvider>();
                provider.windowTitle = "SFC Feature";
                provider.itemList = featureNameTypeList;
                provider.onSelectedChanged = ((string name, object type) itemInfo) =>
                {
                    var passType = (Type)itemInfo.type;
                    CreateSFCPassAsset(passType, inst);
                };

                SearchWindowTools.OpenSearchWindow(provider);
            }
        }

        private void DrawDetails()
        {
            var isDetailsFoldout = serializedObject.FindProperty("isDetailsFoldout");
            isDetailsFoldout.boolValue = EditorGUILayout.Foldout(isDetailsFoldout.boolValue, "Details", true);

            EditorGUIUtility.labelWidth = serializedObject.FindProperty("labelWidth").floatValue;
            if (isDetailsFoldout.boolValue)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < featureSOList.Count; i++)
                {
                    var featureSO = featureSOList[i];
                    if (featureSO == null)
                        continue;

                    var feature = (SRPFeature)featureSO.targetObject;
                    if (!feature)
                        continue;

                    //var featureEditor = feature.GetEditor();

                    var color = feature.enabled ? (feature.interrupt ? Color.red : GUI.color) : Color.gray;
                    var title = feature.name;
                    var foldoutProp = featureSO.FindProperty(nameof(SRPFeature.isFoldout));
                    // draw pass details
                    EditorGUI.BeginChangeCheck();

                    PassDrawer.DrawPassDetail(featureSO, color, foldoutProp, EditorGUITools.TempContent(title, feature.Tooltip));
                    //PassDrawer.DrawPassDetail(featureEditor, color, foldoutProp, EditorGUITools.TempContent(title, feature.Tooltip));

                    if (EditorGUI.EndChangeCheck())
                    {
                        /**
                         same as SRPFeature's OnValidate
                         */
                        //? temporary disable this
                        //feature.DestroyPassInstance();
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        static GenericMenu CreateAddSFCPassMenu(IEnumerable<Type> featureTypes, SRPFeatureListSO inst)
        {
            GenericMenu menu = new GenericMenu();
            foreach (var passType in featureTypes)
            {
                var createAssetMenuAttr = passType.GetCustomAttribute<CreateAssetMenuAttribute>();
                if (createAssetMenuAttr == null)
                    continue;
                // setup menuItem text
                var menuContent = new GUIContent(passType.Name);
                //menuContent.text = createAssetMenuAttr.menuName;

                menu.AddItem(menuContent, false, (passTypeObj) =>
                {
                    var passType = (Type)passTypeObj;
                    CreateSFCPassAsset(passType, inst);
                }, passType);
            }
            return menu;
        }
        
        /// <summary>
        /// create sfcPass ,add inst.featureLIST
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="curSODir"></param>
        /// <param name="inst"></param>
        public static void CreateSFCPassAsset(Type passType,SRPFeatureListSO inst) {
            var curSODir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(inst));
            var passSO = ScriptableObject.CreateInstance(passType);
            var passSavePath = $"{curSODir}/{inst.featureList.Count} {passType.Name}.asset";

            var passAsset = AssetDatabaseTools.CreateAssetThenLoad<SRPFeature>(passSO, passSavePath);
            AssetDatabaseTools.SaveRefresh();

            inst.featureList.Add(passAsset);
        }

        public static void DestroySFCPassAsset(SRPFeatureListSO inst,int id)
        {
            if (id >= 0 && id < inst.featureList.Count)
                inst.featureList[id].Destroy();
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

        public float labelWidth = 250;

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
