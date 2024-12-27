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
        [MenuItem("PowerUtilities/Clear/Clear ScriptableObject's Missing Scripts")]
        static public void ClearMissingTypesOnScriptableObjects()
        {
            var report = new StringBuilder();

            string[] paths;
            var soObjects = AssetDatabaseTools.FindAssetsPathAndLoad<Object>(out paths, "t:ScriptableObject", ".asset");
            for (int i = 0; i < soObjects.Length; i++)
            {
                var path = paths[i];
                //Debug.Log(path);
                var soObj = soObjects[i];
                if (soObj == null)
                    continue;
                
                if (SerializationUtility.ClearAllManagedReferencesWithMissingTypes(soObj))
                {
                    report.Append("Cleared missing types from ").Append(path).AppendLine();
                }
            }

            if (report.Length > 0)
                Debug.Log(report.ToString());
        }

        [MenuItem("PowerUtilities/Clear/Clear GameObject's Missing Scripts")]
        public static void ClearGameObjectMissingMonos()
        {
            var objs = Selection.gameObjects;

            objs.ForEach(obj =>
            {
                var count = GameObjectTools.RemoveChildrenMissingMonos(obj);
                Debug.Log($"{obj} removed count :{count}");
            });
        }
    }
}
#endif