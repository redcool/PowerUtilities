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
        const string DIR = "Assets/TerrainBaseMaps";
        const string PATH_FORMAT = DIR+"/{0}_baseMap.png";

        [MenuItem(TileTerrainWindow.ROOT_PATH + "/BakeTerrainsBaseMap")]
        static void StartBake()
        {
            PathTools.CreateAbsFolderPath(DIR);

            var trs = SelectionTools.GetSelectedComponents<Terrain>();
            trs.ForEach(t => { 
                BakeBaseMap(t);
            });
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(DIR));
        }


        static void BakeBaseMap(Terrain t)
        {
            if (t == null)
                return;
            var path = string.Format(PATH_FORMAT, t.name);
            PathTools.CreateAbsFolderPath(path);
            TerrainTools.ExtractAlphaMapToPNG(t, path);
            AssetDatabase.Refresh();

            
        }
    }
#endif
}