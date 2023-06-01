#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public static class EditorApplicationTools 
    {
        /// <summary>
        /// 
        /// use Event.current in this event.
        /// keyboard event [keyup,keydown],
        /// 
        /// 
        /* 
        EditorApplicationTools.OnGlobalKeyEvent += () => {
                Debug.Log(Event.current);
            };
        */
    /// </summary>
    public static event Action OnGlobalKeyEvent;

        static bool isInited;

        [InitializeOnLoadMethod]
        static void EditorInit()
        {
            EditorApplication.update -= AddEvent;
            EditorApplication.update += AddEvent;
        }

        static void AddEvent()
        {
            if (OnGlobalKeyEvent == null)
                return;

            if (isInited)
            {
                EditorApplication.update -= AddEvent;
                return;
            }

            isInited = AddKeyEvent(OnGlobalKeyEvent);
        }

        private static bool AddKeyEvent(Action onKeyEvent)
        {
            EditorApplication.CallbackFunction KeyEvent = () => {
                onKeyEvent();
            };

            var globalEventHandler = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            EditorApplication.CallbackFunction globalEvent = (EditorApplication.CallbackFunction)globalEventHandler.GetValue(null);
            if (globalEvent !=null)
            {
                globalEvent -= KeyEvent;
                globalEvent += KeyEvent;

                globalEventHandler.SetValue(null, globalEvent);
                return true;
            }
            return false;
        }
    }
}
#endif