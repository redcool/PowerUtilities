#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using UnityEngine;

    using UnityEditor;

    [CustomEditor(typeof(MaterialPropertyI18NSO))]
    public class MaterialPropertyI18NEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var inst = target as MaterialPropertyI18NSO;

            GUILayout.Label($"Property Count : {inst.Count()}");

            if (GUILayout.Button("Load Profile"))
            {
                inst.LoadProfile(inst.profile);
            }
        }
    }

    //[CreateAssetMenu(menuName = "PowerUtilities/Material/I18NProfile")]
    [SOAssetPath("Assets/PowerUtilities/MaterialPropertyI18N.asset")]
    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS+"/MaterialPropertyI18N")]
    public class MaterialPropertyI18NSO : ScriptableObject
    {
        public TextAsset profile;
        public bool forceLoadProfile;

        static Dictionary<string, string> i18nDict = new Dictionary<string, string>();

        static MaterialPropertyI18NSO instance;

        public static MaterialPropertyI18NSO Instance
        {
            get
            {
                if (!instance)
                {
                    instance = ScriptableObjectTools.GetInstance<MaterialPropertyI18NSO>();
                    instance.TryInit();
                }
                return instance;
            }
        }

        public void LoadProfile(TextAsset profile)
        {
            if (!profile)
                return;

            i18nDict.Clear();

            profile.text.ReadKeyValue(i18nDict);
        }

        public void TryInit()
        {
            if (forceLoadProfile)
            {
                forceLoadProfile = false;
                i18nDict.Clear();
            }

            //use default profile
            if (!profile)
            {
                profile = AssetDatabaseTools.FindAssetPathAndLoad<TextAsset>(out _, "MaterialProperty_CN", "txt");
            }

            if (Instance.Count() == 0)
            {
                Instance.LoadProfile(Instance.profile);
            }
        }

        public string GetText(string propName)
        {
            if (string.IsNullOrEmpty(propName))
                return propName;

            var isExist = i18nDict.TryGetValue(propName, out var text);
            return isExist ? text : propName;
        }

        public static string Text(string propName)
        {
            return Instance.GetText(propName);
        }

        public int Count() => i18nDict.Count;
    }
}
#endif