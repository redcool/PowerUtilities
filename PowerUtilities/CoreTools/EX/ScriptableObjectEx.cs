﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class ScriptableObjectEx
    {
        static Dictionary<ScriptableObject, string> soNameDict = new Dictionary<ScriptableObject, string>();

        /// <summary>
        /// Get ScriptableObject's name with cache
        /// </summary>
        /// <param name="so"></param>
        /// <returns></returns>
        public static string GetName(this ScriptableObject so)
        => DictionaryTools.Get(soNameDict, so, so => so.name);

        /// <summary>
        /// SettingSOType is subclass of ScriptableObject
        /// </summary>
        /// <param name="SettingSOType"></param>
        /// <returns></returns>
        public static bool IsExtendsScriptableObject(Type SettingSOType)
        {
            if (!SettingSOType.IsSubclassOf(typeof(ScriptableObject)) && SettingSOType != typeof(ScriptableObject))
            {

                return false;
            }

            return true;
        }
    }
}
