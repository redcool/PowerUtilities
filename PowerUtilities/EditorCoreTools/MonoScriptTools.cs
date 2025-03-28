#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public static class MonoScriptTools
    {
        /// <summary>
        /// Change current script to newScript
        /// </summary>
        /// <param name="current"></param>
        /// <param name="newScript"></param>
        public static void ChangeRefScript(this Object current, MonoScript newScript)
        {
            if (!newScript)
                return;

            var so = new SerializedObject(current);
            var script = so.FindProperty("m_Script");
            script.objectReferenceValue = newScript;
            so.ApplyModifiedProperties();
        }

        public static void ChangeRefScript(this Object current, string newScriptName)
        {
            ChangeRefScript(current,GetMonoScript(newScriptName));
        }

        public static MonoScript GetMonoScript(string scriptName)
        {
            var ms = AssetDatabaseTools.FindAssetPathAndLoad<MonoScript>(out var _, scriptName, ".cs",true);
            return ms;
        }
    }
}
#endif