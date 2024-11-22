#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public class ClearAssetsMissingScripts : MonoBehaviour
    {
        [MenuItem("PowerUtilities/Clear/Clear Missing Scripts")]
        static public void ClearMissingTypesOnScriptableObjects()
        {
            var report = new StringBuilder();

            var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" });
            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                Object obj = AssetDatabase.LoadMainAssetAtPath(path);
                Debug.Log(path);
                if (obj != null)
                {
                    if (SerializationUtility.ClearAllManagedReferencesWithMissingTypes(obj))
                    {
                        report.Append("Cleared missing types from ").Append(path).AppendLine();
                    }
                }
            }

            Debug.Log(report.ToString());
        }
    }
}
#endif