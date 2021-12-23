namespace PowerUtilities
{
#if UNITY_EDITOR
    //using PowerUtilities;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class TerrainMapExtractor
    {
        const string PATH_FORMAT = "Assets/TileTerrain/{0}_blend.png";

        [MenuItem(TileTerrainWindow.ROOT_PATH + "/ExtractAlphaMap")]
        static void ExtractBlendMap()
        {
            var t = Terrain.activeTerrain;
            if (t)
            {
                var path = string.Format(PATH_FORMAT, t.name);
                PathTools.CreateAbsFolderPath(path);
                TerrainTools.ExtractAlphaMapToPNG(t, path);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("select a Terrain first.");
            }
        }


    }
#endif
}