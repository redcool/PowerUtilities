using UnityEngine.Events;

namespace PowerUtilities
{
    public static class UnityEventEx
    {

        public static void AddListener<T>(this UnityEvent<T> unityEvent, UnityAction<T> action)
        {
            if (unityEvent == null)
                unityEvent = new();

            unityEvent.AddListener(action);
        }

        public static void RemoveAllListeners<T>(this UnityEvent<T> unityEvent)
        {
            if (unityEvent != null)
            {
                unityEvent.RemoveAllListeners();
            }
        }
    }
}