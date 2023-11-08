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
        public static T GetInstance<T>(ref T instance) where T : Component {
            if (instance == null)
                instance = Object.FindObjectOfType<T>();

            if (!instance)
            {
                instance = new GameObject(typeof(T).Name).GetOrAddComponent<T>();
            }
            return instance;
        }
    }
}