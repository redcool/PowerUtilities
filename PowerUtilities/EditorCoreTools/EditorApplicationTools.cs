#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.Compilation;
    using UnityEngine;

    public static class EditorApplicationTools
    {
        /// <summary>
        /// 
        /// Can use Event.current in this event.
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

        /// <summary>
        /// when editor inited, call
        /// </summary>
        public static event Action OnEditorReload;

        static bool isInited;

        [InitializeOnLoadMethod]
        static void EditorInit()
        {
            EditorApplication.update -= AddEvent;
            EditorApplication.update += AddEvent;

            if (OnEditorReload != null)
                OnEditorReload();
             
            CompilationPipeline.compilationStarted -= CompilationPipeline_compilationStarted;
            CompilationPipeline.compilationStarted += CompilationPipeline_compilationStarted;

            CompilationPipeline.compilationFinished -= CompilationPipeline_compilationFinished;
            CompilationPipeline.compilationFinished += CompilationPipeline_compilationFinished;
        }

        private static void CompilationPipeline_compilationFinished(object obj)
        {
            var startedMethods = TypeCache.GetMethodsWithAttribute<CompileFinishedAttribute>();
            foreach (var method in startedMethods)
            {
                method.Invoke(null, new[] { obj });
            }
        }

        private static void CompilationPipeline_compilationStarted(object obj)
        {
            var startedMethods = TypeCache.GetMethodsWithAttribute<CompileStartedAttribute>();
            foreach (var method in startedMethods)
            {
                if (!method.IsStatic)
                {
                    Debug.Log(method.Name + "not static!");
                    continue;
                }
                if(method.GetParameters().Length != 1)
                {
                    Debug.Log(method.Name + " dont have a parameter!");
                    continue;
                }
                method.Invoke(null, new[] { obj });
            }
        }


        static void AddEvent()
        {
            if (OnGlobalKeyEvent == null)
                return;

            // add global key events once
            if (isInited)
            {
                EditorApplication.update -= AddEvent;
                return;
            }

            isInited = AddKeyEvent(OnGlobalKeyEvent);
        }

        /// <summary>
        /// Editor keyboard event
        /// </summary>
        /// <param name="onKeyEvent"></param>
        /// <returns></returns>
        private static bool AddKeyEvent(Action onKeyEvent)
        {
            EditorApplication.CallbackFunction KeyEvent = () =>
            {
                onKeyEvent();
            };

            var globalEventHandler = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.NonPublic | BindingFlags.Static);
            EditorApplication.CallbackFunction globalEvent = (EditorApplication.CallbackFunction)globalEventHandler.GetValue(null);
            if (globalEvent != null)
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