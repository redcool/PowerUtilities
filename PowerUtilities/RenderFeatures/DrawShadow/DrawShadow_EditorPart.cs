#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PowerUtilities
{
    public partial class DrawShadow
    {
        partial void CheckEditorSceneLoaded()
        {
            EditorSceneManager.sceneOpened -= EditorSceneManager_sceneOpened;
            //EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;
        }

        private void EditorSceneManager_sceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            Instance?.StepDrawShadow();
        }

    }
}
#endif