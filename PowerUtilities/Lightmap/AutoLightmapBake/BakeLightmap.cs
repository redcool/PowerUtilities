namespace PowerUtilities
{
#if UNITY_EDITOR
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    public class BakeLightmap
    {

        const string BAKE_MENU = "PowerUtilities/Lightmap";

        [MenuItem(BAKE_MENU + "/烘焙目录中的场景")]
        static void InteractiveBake()
        {
            var folders = EditorTools.GetSelectedObjectAssetFolder();
            if (folders.Length == 0)
            {
                Debug.Log("选中目标目录.");
                return;
            }

            StartBake(folders);
        }
        /// <summary>
        /// -paths=Assets/Tmp,Assets/Tmp2
        /// </summary>

        static void CommandBake()
        {
            var args = System.Environment.GetCommandLineArgs();

            var pathLine = args.Where(arg => arg.StartsWith("-paths")).FirstOrDefault();
            if (string.IsNullOrEmpty(pathLine))
                return;

            var paths = pathLine.Split(',');

            if (paths.Length > 0 && paths[0].Contains("="))
                paths[0] = paths[0].Split('=')[1];

            StartBake(paths);

        }

        static void StartBake(string[] folders)
        {
            if (folders == null || folders.Length == 0)
                return;

            //var scenes = AssetDatabaseTools.FindAssetsInProjectByType<SceneAsset>(folders);
            //var scenePaths = scenes.Select(s => AssetDatabase.GetAssetPath(s)).ToArray();

            var scenePaths = AssetDatabaseTools.FindAssetsPath("t:SceneAsset", null,false, folders);

            foreach (var scenePath in scenePaths)
            {
                var scene = EditorSceneManager.OpenScene(scenePath);
                Lightmapping.Bake();
                EditorSceneManager.SaveScene(scene);
            }

            var sceneNames = string.Join("\n", scenePaths);
            Debug.Log("烘焙完毕. \n\n" + sceneNames);
        }

        [MenuItem(BAKE_MENU + "/停止")]
        static void ForceStop()
        {
            Lightmapping.ForceStop();
        }

    }
#endif
}