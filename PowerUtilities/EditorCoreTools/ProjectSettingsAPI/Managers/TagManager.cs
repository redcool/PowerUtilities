#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// control project' tagManager file
    /// Tags,Layers,SortingLayers
    /// </summary>
    public static class TagManager 
    {
        const int MAX_TAGS = 10000;

        public const string 
            TAGS = "tags",
            LAYERS = "layers",
            SORTING_LAYERS = "m_SortingLayers"
            ;

        static CacheTool<string, SerializedObject> cachedManagers = new CacheTool<string, SerializedObject>();

        public static void GetTagsLayers(out SerializedObject tagManagerSo, out SerializedProperty tagLayersSO,string arrayName=TAGS)
        {
            tagManagerSo= UnityTagManager.GetUnityTagManager();
            tagLayersSO = tagManagerSo.FindProperty(arrayName);
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

        public static int GetSortingLayerCount()
        {
            return UnityTagManager.InvokeUnityTagManagerMember((type, inst) =>
            {
                return (int)type.GetMemberValue(nameof(GetSortingLayerCount), inst, Type.EmptyTypes);
            });
        }

        public static void AddSortingLayer()
        {
            UnityTagManager.InvokeUnityTagManagerMember((type, inst) =>
            {
                type.GetMemberValue(nameof(AddSortingLayer), inst, Type.EmptyTypes);
            });
        }

        public static string GetSortingLayerName(int index)
        {
            return UnityTagManager.InvokeUnityTagManagerMember((type, inst) =>
            {
                return (string)type.GetMemberValue(nameof(GetSortingLayerName), inst, new object[] { index });
            });
        }

        public static List<string> GetSortingLayerNames()
        {
            var count = GetSortingLayerCount();
            var list = new List<string>();
            for (int i = 0; i < count; i++)
            {
                list.Add(GetSortingLayerName(i));
            }
            return list;
        }

        public static bool IsSortingLayerExists(string layer)
        {
            var list =GetSortingLayerNames();
            return list.Contains(layer);
        }

        public static bool IsTagExists(string tag)
        {
            return GetTags().Contains(tag);
        }

        public static void AddSortingLayer(string layer)
        {
            UpdateTagsLayers((tagManager, layers) =>
            {
                if (IsSortingLayerExists(layer))
                    return;

                layers.AppendElement(prop => prop.stringValue = layer);
            }, SORTING_LAYERS);
        }

        public static void AddTag(string tag)
        {
            if (IsTagExists(tag))
            {
                //Debug.Log($"{tag} is exists");
                return;
            }
            
            UnityTagManager.AddTag(tag);

            //UpdateTagsLayers((tagManager, tags) =>
            //{
            //    if (tags.arraySize >= MAX_TAGS)
            //    {
            //        throw new Exception("tags length more than " + MAX_TAGS);
            //    }
            //    tags.AppendElement(prop => prop.stringValue = tag);
            //});
        }

        public static void AddLayer(string layer)
        {
            UpdateTagsLayers((tagManager, layers) =>
            {
                if (layers.IsElementExists(p => p.stringValue == layer))
                {
                    //Debug.Log($"{layer} is exists");
                    return;
                }

                var id = GetLayerIndex(p => string.IsNullOrEmpty(p.stringValue));
                if (id != -1)
                    layers.GetArrayElementAtIndex(id).stringValue = layer;
            }, LAYERS);
        }

        public static void RemoveTag(string tag) => UnityTagManager.RemoveTag(tag);
        public static void RemoveLayer(string layer)
        {
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

        /// <summary>
        /// Get tags from UnityEngine.TagManager
        /// </summary>
        /// <returns></returns>
        public static string[] GetTags() => UnityTagManager.GetTags();

        public static int GetLayerIndex(Func<SerializedProperty,bool> predicate)
        {
            if (predicate == null)
                return -1;

            GetTagsLayers(out var tagManager, out var tags, LAYERS);
            var id = tags.GetElements().FindIndex(predicate);
            return id;
        }

        public static string[] GetLayerNames() => UnityTagManager.GetLayers();

        public static void ClearTags(string arrayName=TAGS)
        {
            UpdateTagsLayers((tagManager, tags)=> { 
                tags.ClearArray();
            }, arrayName);
        }

        public static void ClearTags(Func<SerializedProperty, bool> predicate)
        {
            UpdateTagsLayers((tagManager, tags) =>
            {
                for (int i = 0; i < tags.arraySize; i++)
                {
                    if (predicate(tags.GetArrayElementAtIndex(i)))
                    {
                        tags.DeleteArrayElementAtIndex(i);
                        i--;
                    }
                }
            }, TAGS);
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