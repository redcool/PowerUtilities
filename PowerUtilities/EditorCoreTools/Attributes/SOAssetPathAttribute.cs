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
        public string path;
        public SOAssetPathAttribute(string path)
        {
            this.path = path;
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