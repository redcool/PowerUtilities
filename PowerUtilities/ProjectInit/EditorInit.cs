#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace PowerUtilities
{
    public static class EditorInit
    {
        /// <summary>
        /// editor Initial first
        /// </summary>
        [InitializeOnLoadMethod]
        public static void Init()
        {
            AddProjectSymbols();

            AddTags();
            AddLayers(); 
            //AddSortingLayers();
        }
        /// <summary>
        /// Read AsmNames.txt and add define symbols
        /// </summary>
        public static void AddProjectSymbols()
        {
            var asmNamesFilePath = AssetDatabaseTools.FindAssetPath("AsmNames", "txt");
            var nameDict = ConfigTool.ReadKeyValueConfig(asmNamesFilePath);
            nameDict.ForEach(kv => 
                PlayerSettingTools.AddMacroDefines(kv.Value)
            );
        }

        public static void AddTags()
        {
            var tags = Tags.GetTags();
            tags.ForEach(tag => TagManager.AddTag(tag));
        }

        public static void AddLayers()
        {
            var layers = Layers.GetLayers();
            layers.ForEach(layer => TagManager.AddLayer(layer));
        }

        public static void AddSortingLayers()
        {
            var layers = SortingLayers.GetSortingLayers();
            layers.ForEach(layer => TagManager.AddSortingLayer(layer));
        }
    }
}
#endif