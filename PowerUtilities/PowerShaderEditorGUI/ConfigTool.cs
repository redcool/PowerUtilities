#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace PowerUtilities
{

    /// <summary>
    /// key=value,配置文件操作
    /// </summary>
    public static class ConfigTool
    {

        public enum LayoutProfileType
        {
            Standard = 0,
            MIN_VERSION
        }

        /// <summary>
        /// * = *
        /// </summary>
        static Regex kvRegex = new Regex(@"\s*=\s*");
        public const string 
            I18N_PROFILE_PATH = "Profiles/i18n.txt",
            LAYOUT_PROFILE_PATH_FORMAT = "Profiles/Layout{0}.{1}",// Layout.txt,LayoutMIN_VERSION.txt,Layout.json
            COLOR_PROFILE_PATH = "Profiles/Colors.txt",
            PROP_HELP_PROFILE_PATH = "Profiles/Helps.txt",

            MIN_VERSION = nameof(MIN_VERSION)
            ;

        static GUIContent powerShaderInspectorContent = new GUIContent();

        public static string GetLayoutProfilePath(LayoutProfileType profileType,string extName="txt") 
            => string.Format(LAYOUT_PROFILE_PATH_FORMAT
                , profileType == LayoutProfileType.Standard ? "" : Enum.GetName(typeof(LayoutProfileType),profileType)
                , extName);
        /// <summary>
        /// 从configPath开始找configFileName文件,一直找到Assets目录
        /// </summary>
        /// <param name="configPath"></param>
        /// <param name="configFileName"></param>
        /// <param name="maxFindCount"></param>
        /// <returns></returns>
        public static string FindPathRecursive(string configPath,string configFileName="i18n.txt",int maxFindCount=10)
        {
            var pathDir = Path.GetDirectoryName(configPath);
            var filePath = "";
            var findCount = 0;
            while (!pathDir.EndsWith("Assets"))
            {
                filePath = pathDir + "/"+ configFileName;
                pathDir = Path.GetDirectoryName(pathDir);
                if (File.Exists(filePath) || ++findCount > maxFindCount)
                    break;
            }
            return filePath;
        }

        /// <summary>
        /// key=value的配置文件读入到内存.
        /// 
        /// dict 结构为
        /// {
        ///     key1 = *1,*2
        /// }
        /// </summary>
        /// <param name="configFilePath"></param>
        /// <returns></returns>
        public static Dictionary<string,string> ReadKeyValueConfig(string configFilePath)
        {
            var dict = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(configFilePath) && File.Exists(configFilePath))
            {
                var text = File.ReadAllText(configFilePath);
                text.ReadKeyValue(dict);
            }
            return dict;
        }

        public static string[] SplitBy(string line,char splitChar=',')
        {
            var vs = line.Split(splitChar);
            return vs.Select(v => v.Trim()).ToArray();
        }


        public static Dictionary<string,string> ReadConfig(string shaderFilePath,string profileFilePath)
        {
            var path = FindPathRecursive(shaderFilePath, profileFilePath);
            return ReadKeyValueConfig(path);
        }


        public static Dictionary<string, MaterialProperty> CacheProperties(MaterialProperty[] properties)
        {
            var propDict = new Dictionary<string, MaterialProperty>();

            foreach (var prop in properties)
            {
                propDict[prop.name] = prop;
            }

            return propDict;
        }

        public static string Text(Dictionary<string,string> dict,string propName)
        {
            if(dict != null && dict.TryGetValue(propName,out var text))
            {
                return text;
            }

            return propName;
        }

       
        public static GUIContent GetContent(Dictionary<string,string> nameDict,Dictionary<string,string> propHelpDict,string propName)
        {
            var text = Text(nameDict, propName);
            propHelpDict.TryGetValue(propName, out var tooltips);

            return EditorGUITools.TempContent(text, tooltips,inst: powerShaderInspectorContent);
        }

        public static Color GetPropColor(Dictionary<string, string> colorTextDict, string propName)
        {
            var contentColor = GUI.contentColor;
            if (colorTextDict.TryGetValue(propName, out var colorString))
            {
                ColorUtility.TryParseHtmlString(colorString, out contentColor);
            }

            return contentColor;
        }
    }
}
#endif