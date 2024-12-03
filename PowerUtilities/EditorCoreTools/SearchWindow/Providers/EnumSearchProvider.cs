#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;

    //[SOAssetPath("Assets/PowerUtilities/GraphicsFormatSearchProvider.asset")]
    //[ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/SearchWindows/GraphicsFormatSearchProvider")]
    /// <summary>
    /// Show Enum in SearchWindow
    /// 
    /// GraphicsFormat can use GraphicsFormat.txt specially
    /// src Check EnumSearchableAttributeDrawer, 
    /// 
    /// demo:
    /// 
    /// [EnumSearchable(typeof(GraphicsFormat), textFileName = "")]
    /// public GraphicsFormat gFormat; [EnumSearchable(typeof(GraphicsFormat), textFileName = "")]
    /// public GraphicsFormat gFormat;
    ///     
    /// </summary>
    public class EnumSearchProvider : BaseSearchWindowProvider<int>
    {
        //[LoadAsset("GraphicsFormat.txt")]
        public TextAsset formatTxt;

        /// <summary>
        /// use GraphicsFormat.txt or not
        /// </summary>
        public string textFileName;

        /// <summary>
        /// show enum to SearchWindow
        /// </summary>
        public Type enumType;

        /// <summary>
        /// cached tree, like GraphicsFormat.txt
        /// </summary>
        public List<SearchTreeEntry> graphicsFormatTreeList = new();

        public class GraphicsFormatInfo
        {
            public string name;
            public int value;
            public string desc;

            public static GraphicsFormatInfo zero = new ();
        }

        public enum GraphicsFormatType
        {
            None,
            ColorFormat,
            DepthFormat,
            Video,
            DXT,
            PVRTC,
            ETC,
            ASTC,
        }
        /*
        Dictionary<GraphicsFormatType, List<GraphicsFormatInfo>> formatTypesDict = new()
        {
            { GraphicsFormatType.ColorFormat, new List<GraphicsFormatInfo>() },
            { GraphicsFormatType.DepthFormat ,new List<GraphicsFormatInfo>()},
            { GraphicsFormatType.Video,new List<GraphicsFormatInfo>() },
            { GraphicsFormatType.ETC,new List<GraphicsFormatInfo>()},
            { GraphicsFormatType.PVRTC,new List<GraphicsFormatInfo>() },
            { GraphicsFormatType.ASTC, new List<GraphicsFormatInfo>()},
            {GraphicsFormatType.DXT,new List<GraphicsFormatInfo>() },
        };
        */

        public void TryLoadTextFile()
        {
            var fileName = Path.GetFileNameWithoutExtension(textFileName);
            var extName = Path.GetExtension(fileName);
            //"GraphicsFormat", ".txt"
            formatTxt = AssetDatabaseTools.FindAssetPathAndLoad<TextAsset>(out _, fileName, extName, true);
        }

        public void ParseTxt(string formatText, out Dictionary<GraphicsFormatType, List<GraphicsFormatInfo>> infoDict)
        {
            infoDict = new();

            var sb = new StringBuilder();
            foreach (var line in formatText.ReadLines())
            {
                // accumulate description
                if (line.StartsWith("//") || line.StartsWith("[Obsolete"))
                {
                    sb.Append(line);
                    continue;
                }
                var kv = line.SplitKeyValuePair();
                if (kv.Length == 2)
                {
                    var formatName = kv[0];
                    var formatValueStr = kv[1];
                    var formatInfo = new GraphicsFormatInfo
                    {
                        name = formatName,
                        value = Convert.ToInt32(RegExTools.GetMatch(formatValueStr, RegExTools.NUMBER)),
                        desc = sb.ToString()
                    };
                    AddToInfoDict(infoDict, formatInfo);
                }
                else
                {
                    throw new Exception("parse failed : " + line);
                }
                sb.Clear();
            }
        }

        /// <summary>
        /// Add an item to dict(Group)
        /// </summary>
        /// <param name="infoDict"></param>
        /// <param name="formatInfo"></param>
        private void AddToInfoDict(Dictionary<GraphicsFormatType, List<GraphicsFormatInfo>> infoDict, GraphicsFormatInfo formatInfo)
        {
            var formatName = formatInfo.name;
            
            var isColorFormat = GraphicsFormatTools.IsRGBAFormat(formatName);
            var isDepthFormat = GraphicsFormatTools.IsDepthFormat(formatName);
            var isVideoFormat = GraphicsFormatTools.IsVideoFormat(formatName);
            var isDXT = GraphicsFormatTools.IsDXTFormat(formatName);
            var isPVRTC = GraphicsFormatTools.IsPVRTCFormat(formatName);
            var isETC = GraphicsFormatTools.IsETCFormat(formatName);
            var isASTC = GraphicsFormatTools.IsASTCFormat(formatName);
            //Debug.Log($"{formatName} iscolor :{isColorFormat}, isDepth:{isDepthFormat},isVideo:{isVideoFormat},isDxt:{isDXT},isPVR:{isPVRTC},isEtc:{isETC},isAstc:{isASTC}");

            // combine type to array
            var formatEnumId = new[] { isColorFormat, isDepthFormat, isVideoFormat, isDXT, isPVRTC, isETC, isASTC }
            .Select((isFormat, id) => isFormat ? id + 1 : 0)
            .Sum();

            var formatType = (GraphicsFormatType)formatEnumId;

            if (!infoDict.ContainsKey(formatType))
            {
                infoDict[formatType] = new List<GraphicsFormatInfo>();
            }

            infoDict[formatType].Add(formatInfo);
        }
        public bool TryParseGraphicsFormatTxtFillList(ref List<SearchTreeEntry> list)
        {
            if (!formatTxt)
                TryLoadTextFile();

            if (!formatTxt)
                return false;

            ParseTxt(formatTxt.text, out var infoDict);

            list.Add(new SearchTreeGroupEntry(new GUIContent(windowTitle)));

            //// show in first page
            var noneFormat = infoDict[GraphicsFormatType.None].FirstOrDefault();
            list.Add(new SearchTreeEntry(new GUIContent(noneFormat.name, noneFormat.desc))
            {
                level = 1,
                userData = 0
            });

            // show in 2 page
            foreach (var formatType in infoDict.Keys)
            {
                if (formatType == GraphicsFormatType.None)
                    continue;

                var formatList = infoDict[formatType];
                list.Add(new SearchTreeGroupEntry(new GUIContent(formatType.ToString()), 1));

                foreach (var info in formatList)
                {
                    list.Add(new SearchTreeEntry(new GUIContent(info.name, info.desc))
                    {
                        level = 2,
                        userData = info.value
                    });
                }
            }

            return true;
        }


        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            // for GraphicsFormat
            if (enumType == typeof(GraphicsFormat) && !string.IsNullOrEmpty(textFileName))
            {
                if (graphicsFormatTreeList.Count == 0)
                    TryParseGraphicsFormatTxtFillList(ref graphicsFormatTreeList);
                return graphicsFormatTreeList;
            }
            else
            {
                var list = new List<SearchTreeEntry>();
                // Enum
                SearchWindowTools.FillWithEnum(ref list, enumType);
                return list;
            }

        }

    }
}
#endif