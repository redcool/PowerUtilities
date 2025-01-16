#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using static UnityEditor.Experimental.GraphView.GraphView;

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
            AddPowerUtilsSymbol();

            AddTags();
            AddLayers();
            //AddSortingLayers();
        }

        public static void AddPowerUtilsSymbol()
        {
            var asmNames = AssemblyNames.GetAssemblyNames();
            asmNames.ForEach(asmName => 
                PlayerSettingTools.AddMacroDefines(asmName)
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