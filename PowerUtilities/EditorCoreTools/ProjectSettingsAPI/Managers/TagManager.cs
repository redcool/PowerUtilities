#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// control project' tagManager
    /// </summary>
    public static class TagManager 
    {
        const int MAX_TAGS = 10000;

        public const string 
            TAGS = "tags",
            LAYERS = "layers"
            ;

        static CacheTool<string, SerializedObject> cachedManagers = new CacheTool<string, SerializedObject>();

        public static SerializedObject GetTagLayerManager()
            => ProjectSettingManagers.GetAsset(ProjectSettingManagers.ProjectSettingTypes.TagManager);

        public static void GetTagsLayers(out SerializedObject tagManager, out SerializedProperty tagLayers,string arrayName=TAGS)
        {
            tagManager= GetTagLayerManager();
            tagLayers = tagManager.FindProperty(arrayName);
        }

        public static void UpdateTagsLayers(Action<SerializedObject, SerializedProperty> onAction, string arrayName = TAGS)
        {
            if (onAction == null)
                return;

            GetTagsLayers(out var tagManager, out var tags, arrayName);
            tagManager.Update();
            onAction(tagManager, tags);
            tagManager.ApplyModifiedProperties();
        }

        public static bool IsTagExists(string tag,string arrayName = TAGS)
        {
            GetTagsLayers(out var tagManager, out var tags,arrayName);
            return tags.IsElementExists(p => p.stringValue == tag);
        }


        public static bool IsLayerExists(string layer) => IsTagExists(layer, LAYERS);

        public static void AddTag(string tag)
        {
            UpdateTagsLayers((tagManager, tags) =>
            {
                if (tags.arraySize>= MAX_TAGS)
                {
                    throw new Exception("tags length more than "+MAX_TAGS);
                }

                if (IsTagExists(tag))
                    return;
                tags.AppendElement(prop => prop.stringValue = tag);
            });
        }

        public static void RemoveTag(string tag,string arrayName=TAGS)
        {
            UpdateTagsLayers((tagManager, tags) =>
            {
                var id = tags.GetElementIndex(prop => prop.stringValue == tag);
                if (id != -1)
                    tags.DeleteArrayElementAtIndex(id);
            }, arrayName);
        }
        public static void RemoveLayer(string layer)
        {
            //UpdateTagsLayers((tagManager, tags) =>
            //{
            //    var id = tags.GetElementIndex(prop => prop.stringValue == layer);
            //    if (id != -1)
            //        tags.GetArrayElementAtIndex(id).stringValue = "";
            //}, LAYERS);

            RenameLayer(layer, "");
        }

        public static void RenameTag(string oldTag, string newTag, string arrayName = TAGS)
        {
            UpdateTagsLayers((tagManager, tags) =>
            {
                var id = tags.GetElementIndex(prop => prop.stringValue == oldTag);
                var newTagId = tags.GetElementIndex(p => p.stringValue == newTag);

                var isValid = arrayName == TAGS ? (id != -1 && newTagId == -1) : id != -1;
                if (isValid)
                    tags.GetArrayElementAtIndex(id).stringValue = newTag;
            }, arrayName);
        }

        public static void RenameLayer(string oldLayer,string newLayer) => RenameTag(oldLayer,newLayer,LAYERS);

        public static int GetLayerIndex(Func<SerializedProperty,bool> predicate)
        {
            if (predicate == null)
                return -1;

            GetTagsLayers(out var tagManager, out var tags, LAYERS);
            var id = tags.GetElements().FindIndex(predicate);
            return id;
        }

        public static void AddLayer(string layer)
        {
            UpdateTagsLayers((tagManager, layers) =>
            {
                if (!IsLayerExists(layer))
                {
                    var id = GetLayerIndex(p => string.IsNullOrEmpty(p.stringValue));
                    if (id != -1)
                        layers.GetArrayElementAtIndex(id).stringValue = layer;
                }

            }, LAYERS);
        }

        public static string[] GetTags(string arrayName=TAGS)
        {
            GetTagsLayers(out var tagManager, out var tags,arrayName);
            return tags.GetElements().Select(prop => prop.stringValue).ToArray();
        }

        public static string[] GetLayers() => GetTags(LAYERS);

        public static void ClearTags(string arrayName=TAGS)
        {
            UpdateTagsLayers((tagManager, tags)=> { 
                tags.ClearArray();
            }, arrayName);
        }

        public static void ClearLayers()
        {
            UpdateTagsLayers((tagManager, layers) => {
                layers.GetElements().ForEach((prop, id) => {
                    prop.stringValue = "";
                });
            }, LAYERS);
        }
        
    }
}
#endif