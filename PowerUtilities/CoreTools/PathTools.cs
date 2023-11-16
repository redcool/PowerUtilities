namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using UnityEngine;

    public static class PathTools
    {
        /// <summary>
        /// dont set value
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string GetCallerFilePath([CallerFilePath]string filepath="") {
            return filepath;
        }

        /// <summary>
        /// dont set value
        /// </summary>
        /// <param name="callerFilePath"></param>
        /// <returns></returns>
        public static string GetCallerFolderPath([CallerFilePath] string callerFilePath="")
        {
            return Path.GetDirectoryName(callerFilePath);
        }

        public static string GetAssetDir(string assetPath,params string[] combineStrings)
        {
            if (string.IsNullOrEmpty(assetPath))
                return "";

            var dir = assetPath;
            if(Path.HasExtension(dir))
                dir = Path.GetDirectoryName(assetPath);

            var sb = new StringBuilder();
            sb.Append(dir);
            foreach (var item in combineStrings)
                sb.Append(item);
            return sb.ToString();
        }

        public static string GetAssetAbsPath(string assetPath)
        {
            return Application.dataPath + "/" + assetPath.Substring("Assets".Length);
        }

        public static string GetAssetPath(string absPath)
        {
            var index = absPath.IndexOf("Assets");
            if (index != -1)
                return absPath.Substring(index);
            return absPath;
        }

        public static void CreateAbsFolderPath(string assetPath)
        {
            var absPath = GetAssetAbsPath(assetPath);
            var dirPath = absPath;

            var extName = Path.GetExtension(assetPath);
            if (!string.IsNullOrEmpty(extName))
            {
                dirPath = Path.GetDirectoryName(absPath);
            }

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

    }
}