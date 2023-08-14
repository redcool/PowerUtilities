using UnityEngine;

namespace PowerUtilities
{
    public static class SingletonTools
    {

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