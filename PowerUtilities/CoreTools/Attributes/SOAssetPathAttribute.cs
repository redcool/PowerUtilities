using System;
using System.Reflection;

namespace PowerUtilities
{
    /// <summary>
    /// scriptableObject's Asset Path
    /// like "Assets/PowerUtilities/PowerAssetImporterSettings.asset"
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SOAssetPathAttribute : Attribute
    {
        public const string PATH_POWER_UTILITIES = "Assets/PowerUtilities";
        public string path;
        public SOAssetPathAttribute(string pathOrName)
        {
            if (!pathOrName.EndsWith(".asset"))
            {
                path = $"{PATH_POWER_UTILITIES}/{pathOrName}.asset";
            }
            else
                path = pathOrName;
        }

        public static string GetPath(Type t)
        {
            if (t == null)
                return default;

            var attr = t.GetCustomAttribute<SOAssetPathAttribute>();
            if (attr == null) return default;
            return attr.path;
        }
    }
}