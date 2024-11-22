using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PowerUtilities
{
    public class SceneControl : MonoBehaviour
    {

        public string sceneName;

        [EditorButton(onClickCall = "LoadScene")]
        public bool isLoad;

        [EditorButton(onClickCall = "LoadSceneAsync")]
        public bool isAsyncLoad;

        public void LoadScene()
        {
            if (string.IsNullOrEmpty(sceneName))
                return;

            SceneManager.LoadScene(sceneName);

        }

        public void LoadSceneAsync()
        {
            if (string.IsNullOrEmpty(sceneName))
                return;

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;
            op.completed += Op_completed;
        }

        private void Op_completed(AsyncOperation op)
        {

            op.allowSceneActivation = true;
        }
    }
}
