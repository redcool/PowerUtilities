#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using static UnityEditor.SceneManagement.EditorSceneManager;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Events;


namespace PowerUtilities
{
    /// <summary>
    /// SceneManager and EditorSceneManager
    /// </summary>
    public static class SceneManagerTools
    {
        /// <summary>
        /// Remove action first
        /// then Add action to sceneUnloaded
        /// </summary>
        /// <param name="action"></param>
        public static void AddSceneUnloaded(UnityAction<Scene> action)
        {
            RemoveSceneUnloaded(action);
            SceneManager.sceneUnloaded += action;
        }
        public static void RemoveSceneUnloaded(UnityAction<Scene> action)
        {
            SceneManager.sceneUnloaded -= action;
        }
        /// <summary>
        /// Remove action first
        /// then Add action to sceneLoaded
        /// </summary>
        /// <param name="action"></param>
        public static void AddSceneLoaded(UnityAction<Scene,LoadSceneMode> action)
        {
            RemoveSceneLoaded(action);
            SceneManager.sceneLoaded += action;
        }
        public static void RemoveSceneLoaded(UnityAction<Scene, LoadSceneMode> action)
        {
            SceneManager.sceneLoaded -= action;
        }

        /// <summary>
        /// Remove action first
        /// then Add action to activeSceneChanged
        /// </summary>
        /// <param name="action"></param>
        public static void AddActiveSceneChanged(UnityAction<Scene, Scene> action)
        {
            RemoveActiveSceneChanged(action);
            SceneManager.activeSceneChanged += action;
#if UNITY_EDITOR
            activeSceneChangedInEditMode += action;
#endif
        }

        public static void RemoveActiveSceneChanged(UnityAction<Scene, Scene> action)
        {
            SceneManager.activeSceneChanged -= action;
#if UNITY_EDITOR
            activeSceneChangedInEditMode -= action;
#endif
        }
    }
}
