#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditorInternal;
using UnityEngine;

namespace PowerUtilities
{
    public class VersionDefineInfo
    {
        public string name;
        public string expression;
        public string define;
    }
    public class AssemblyDefinitionInfo
    {
        public string name;
        public string rootNamespace;
        public string[] references;
        public string[] includePlatforms;
        public string[] excludePlatforms;
        public bool allowUnsafeCode;
        public bool overrideReferences;
        public string[] precompiledReferences;
        public bool autoReferenced;
        public string[] defineConstraints;
        public VersionDefineInfo[] versionDefines = new VersionDefineInfo[] { };
        public bool noEngineReferences;
    }

    public class AssemblyDefinitionImporterInspectorContext
    {
        [MenuItem("CONTEXT/AssemblyDefinitionImporter/ShowAssemblyInfo", secondaryPriority = 8)]
        public static void ShowAssemblyInfo()
        {
            Debug.Log(GetAsmDefInfo());
        }

        [MenuItem("CONTEXT/AssemblyDefinitionImporter/SaveAsmMetaInfo", secondaryPriority = 8)]
        public static void SaveAsmMetaInfo()
        {
            var sb = new StringBuilder();
            foreach (TextAsset ta in Selection.objects)
            {
                var asmDef = JsonUtility.FromJson<AssemblyDefinitionInfo>(ta.text);
                sb.AppendLine($"{ta.name}.dll");
                sb.AppendLine("GUID         , AssetPath");
                sb.AppendLine();
                foreach (var item in asmDef.references)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(item.Substring(5));
                    sb.AppendLine($"{item} , {assetPath}");
                }
                sb.AppendLine();

                // save
                var taPath = AssetDatabase.GetAssetPath(ta);
                var filePath = taPath.SubstringLast(7) + ".metaInfo.txt";
                if (!File.Exists(filePath))
                    using (File.Create(filePath)) 
                    {}
                File.WriteAllText(filePath, sb.ToString());
                sb.Clear();

                AssetDatabaseTools.SaveRefresh();
                
            }
        }

        private static string GetAsmDefInfo()
        {
            var sb = new StringBuilder();
            foreach (TextAsset ta in Selection.objects)
            {
                var asmDef = JsonUtility.FromJson<AssemblyDefinitionInfo>(ta.text);
                sb.AppendLine($"{ta.name}.dll");
                sb.AppendLine("GUID         , AssetPath");
                sb.AppendLine();
                foreach (var item in asmDef.references)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(item.Substring(5));
                    sb.AppendLine($"{item} , {assetPath}");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
#endif