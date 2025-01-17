#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// UnityEngine.TagManager c# invoke
    /// </summary>
    public static partial class UnityTagManager
    {

        public static SerializedObject GetUnityTagManager()
            => ProjectSettingManagers.GetAsset(ProjectSettingManagers.ProjectSettingTypes.TagManager);

        /// <summary>
        /// call unity's TagManager 's member
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="type"></param>
        /// <param name="onInvoke"></param>
        public static TResult InvokeUnityTagManagerMember<TResult>(Func<Type, object, TResult> onInvoke)
        {
            var tagManager = GetUnityTagManager();
            var inst = tagManager.targetObject;
            var type = inst.GetType();

            return onInvoke.Invoke(type, inst);
        }

        public static void InvokeUnityTagManagerMember(Action<Type, object> onInvoke)
        {
            var tagManager = GetUnityTagManager();
            var inst = tagManager.targetObject;
            var type = inst.GetType();

            onInvoke?.Invoke(type, inst);
        }

        public static string[] GetTags()
        {
            return InvokeUnityTagManagerMember((type, inst) =>
            {
                return (string[])type.GetMemberValue("tags", inst, Type.EmptyTypes);
            });
        }

        public static int AddTag(string tag)
        {
            return InvokeUnityTagManagerMember((type, inst) =>
            {
                return (int)type.GetMemberValue("AddTag", inst, tag);
            });
        }

        public static void RemoveTag(string tag)
        {
            InvokeUnityTagManagerMember((type, inst) =>
            {
                type.GetMemberValue(nameof(RemoveTag), inst, tag);
            });
        }

        //public static void RemoveTagRepeated()
        //{
        //    var tags = GetTags();
        //    var sets = new HashSet<string>();
        //    foreach (var item in tags)
        //    {
        //        sets.Add(item);

        //        RemoveTag(item);
        //    }
        //    tags = sets.ToArray();
        //    foreach (var item in tags)
        //    {
        //        AddTag(item);
        //    }

        //    tags = GetTags();
        //    tags.ForEach(Debug.Log);
        //}


        public static int GetDefinedLayerCount()
        {
            return InvokeUnityTagManagerMember((type, inst) =>
            {
                return (int) type.GetMemberValue(nameof(GetDefinedLayerCount), inst);
            });
        }

        public static void GetDefinedLayers(ref string[] layerNames, ref int[] layerValues)
        {
            (string[] layerName, int[] layerValues) info;

            info = InvokeUnityTagManagerMember((type, inst) =>
            {
                var m = type.GetMethod(nameof(GetDefinedLayers), ReflectionTools.instanceBindings);

                string[] _layerNames = default;
                int[] _layerValues = default;

                var args = new object[] { _layerNames, _layerValues };
                m.Invoke(default, args);

                return (_layerNames, _layerValues);
            });

            layerNames = info.layerName;
            layerValues=info.layerValues;
        }

        public static string[] GetLayers()
        {
            string[] layerNames = default;
            int[] layerValues  =default;

            GetDefinedLayers(ref layerNames,ref layerValues);
            return layerNames;
        }
    }
}
#endif