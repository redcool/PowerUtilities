namespace PowerUtilities
{
#if UNITY_EDITOR
    //using PowerUtilities;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class TerrainMapBaker
    {
        const string PATH_FORMAT = "Assets/TileTerrain/{0}_baseMap.png";

        [MenuItem(TileTerrainWindow.ROOT_PATH + "/BakeTerrainBaseMap")]
        static void BakeTerrainBaseMap()
        {
            var t = Terrain.activeTerrain;
            if (t)
            {
                var path = string.Format(PATH_FORMAT, t.name);
                PathTools.CreateAbsFolderPath(path);
                TerrainTools.ExtractAlphaMapToPNG(t, path);
                AssetDatabase.Refresh();

                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
            }
            else
            {
                Debug.LogError("select a Terrain first.");
            }
        }


    }
#endif
}