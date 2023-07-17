#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using UnityEngine;

    using UnityEditor;

    [CustomEditor(typeof(MaterialPropertyI18N))]
    public class MaterialPropertyI18NEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var inst = target as MaterialPropertyI18N;

            GUILayout.Label($"Property Count : {inst.Count()}");

            if (GUILayout.Button("Load Profile"))
            {
                inst.LoadProfile(inst.profile);
            }
        }
    }

    [CreateAssetMenu(menuName = "PowerUtilities/Material/I18NProfile")]
    public class MaterialPropertyI18N : ScriptableObject
    {
        public TextAsset profile;
        public bool forceLoadProfile;

        static Dictionary<string, string> i18nDict = new Dictionary<string, string>();

        static MaterialPropertyI18N instance;
         
        public static MaterialPropertyI18N Instance
        {
            get
            {
                if (!instance)
                {
                    var path= AssetDatabaseTools.FindAssetsPath("MaterialPropertyI18N", "cs").FirstOrDefault();
                    var profileDir = $"{Path.GetDirectoryName(path)}/Profiles/";
                    instance = AssetDatabaseTools.FindAssetsInProject<MaterialPropertyI18N>("", profileDir).FirstOrDefault();
                }
                return instance;
            }
        }

        [InitializeOnLoadMethod]
        static void Init()
        {
            Instance.TryInit();
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
                i18nDict.Clear();

            if (instance.Count() == 0)
            {
                instance.LoadProfile(instance.profile);
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
            return instance.GetText(propName);
        }

        public int Count() => i18nDict.Count;
    }
}
#endif