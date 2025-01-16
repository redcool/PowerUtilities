using UnityEngine;
using UnityEngine.SceneManagement;

namespace PowerUtilities
{
    public static class LightTools
    {

        static Light mainLight;

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            SceneManagerTools.AddSceneUnloaded(SceneManager_sceneUnloaded);
        }

        private static void SceneManager_sceneUnloaded(Scene arg0)
        {
            mainLight = null;
        }

        /// <summary>
        /// Find MainLight(max intensity) from cache,RenderSettings.sun, tags ,or scene Light objects
        /// </summary>
        /// <param name="isForceFind"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static Light FindMainLight(bool isForceFind=false,params string[] tags)
        {
            if (mainLight && !isForceFind)
                return mainLight;

            mainLight = RenderSettings.sun;
            if (mainLight)
                return mainLight;

            foreach (var tag in tags)
            {
                var tagGo = GameObject.FindGameObjectWithTag(tag);
                if(! tagGo)
                    continue;

                mainLight = tagGo.GetComponent<Light>();
                if (tagGo && mainLight)
                    return mainLight;
            }

            mainLight= FindSceneMainLight();
            return mainLight;
        }

        public static Light FindSceneMainLight()
        {
            var lights = GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None);
            var mainLight = lights.FindMax(l => l.intensity);
            return mainLight;
        }


    }
}