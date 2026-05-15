using System;
using UnityEngine;

namespace PowerUtilities
{
    public static class SingletonTools
    {

        /// <summary>
        /// Get gameObject which attach Component(T), if not exists create new one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static T GetInstance<T>(ref T instance) where T : Component
        {
            if (instance == null)
                instance = UnityObjectEx.FindObject<T>(false);

            if (!instance)
            {
                instance = new GameObject(typeof(T).Name).GetOrAddComponent<T>();
            }
            return instance;
        }
        /// <summary>
        /// Get Instance of T, if not exists create new one by GetInstFunc.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="GetInstFunc"></param>
        /// <returns></returns>
        public static T Get<T>(ref T instance, Func<T> GetInstFunc)
        {
            if (instance == null && GetInstFunc != null)
                instance = GetInstFunc();
            
            return instance;
        }
    }
}