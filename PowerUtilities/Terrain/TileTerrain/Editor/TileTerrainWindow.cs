namespace PowerUtilities
{
#if UNITY_EDITOR
    using PowerUtilities;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class TileTerrainWindow : EditorWindow
    {
        public const string ROOT_PATH = "PowerUtilities/Terrain/Command";

        Material terrainMat;
        Terrain[] terrainObjs;

        int tileCount = 1;


        public enum SaveResolution { Full, Half, Quarter, Eighth, Sixteeth }
        SaveResolution saveResolution = SaveResolution.Half;
        private Vector2 scrollPosition;

        [MenuItem(ROOT_PATH + "/Tile Terrain Window")]
        static void Init()
        {
            var win = GetWindow<TileTerrainWindow>();
            win.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Terrain导出为Mesh", MessageType.Info);

            if (GUILayout.Button("Check Terrain"))
            {
                terrainObjs = FindObjectsOfType<Terrain>();
            }

            if (terrainObjs != null && terrainObjs.Length > 0)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, "Box", GUILayout.Height(200));
                var id = 0;
                foreach (var item in terrainObjs)
                {
                    EditorGUILayout.BeginHorizontal("Box");
                    GUILayout.Label("id:" + (id++), GUILayout.Width(50));
                    EditorGUILayout.ObjectField(item, typeof(Terrain), false);
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("场景里没有Terrain", MessageType.Info);
                return;
            }

            saveResolution = (SaveResolution)EditorGUILayout.EnumPopup("Save Resolution:", saveResolution);

            tileCount = Mathf.Max(1, EditorGUILayout.IntField("Tile Count:", Mathf.NextPowerOfTwo(tileCount)));
            //terrainMat = (Material)EditorGUILayout.ObjectField("Terrain Material:",terrainMat, typeof(Material), false);

            if (GUILayout.Button("Export"))
            {
                var rootGo = new GameObject("Terrains Mesh");
                foreach (var item in terrainObjs)
                {
                    ExportTerrain(item, rootGo.transform);
                }
            }
        }

        private void ExportTerrain(Terrain terrainObj, Transform rootTr)
        {
            var terrainGo = new GameObject(terrainObj.gameObject.name + " Mesh");
            terrainGo.transform.localPosition = terrainObj.gameObject.transform.localPosition;
            terrainGo.transform.localRotation = terrainObj.transform.localRotation;
            terrainGo.transform.localScale = terrainObj.transform.localScale;
            terrainGo.transform.SetParent(rootTr, false);

            var resScale = (int)Mathf.Pow(2, (int)saveResolution);

            GenerateTiles(terrainObj, tileCount, terrainGo.transform, saveResolution, resScale);
        }

        static void GenerateTiles(Terrain terrain, int tileCount, Transform parent, SaveResolution saveResolution, int resScale)
        {
            var td = terrain.terrainData;
            var heightmapSize = (td.heightmapResolution - 1) / tileCount;

            var id = 0;
            var count = tileCount * tileCount;

            for (int x = 0; x < tileCount; x++)
            {
                for (int z = 0; z < tileCount; z++)
                {
                    var heightmapRect = new RectInt(x * heightmapSize, z * heightmapSize, heightmapSize + 1, heightmapSize + 1);
                    var tileMesh = TerrainTools.GenerateTileMesh(terrain, heightmapRect, resScale);

                    GenerateTileGo(string.Format("Tile-{0}_{1}", x, z), tileMesh, parent, terrain);
                    id++;

                    DisplayProgress(id, count);
                }
            }
        }

        public static void GenerateTileGo(string name, Mesh mesh, Transform parent, Terrain terrain)
        {
            var tileGo = new GameObject(name);
            tileGo.transform.SetParent(parent, false);

            var mr = tileGo.AddComponent<MeshRenderer>();
            TerrainTools.CopyFromTerrain(mr, terrain);

            var mf = tileGo.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            tileGo.AddComponent<MeshCollider>();
        }



        static void DisplayProgress(int id, int count)
        {
            EditorUtility.DisplayProgressBar("Progress", "Export Progress", (float)id / count);
            if (id == count)
                EditorUtility.ClearProgressBar();
        }


    }
#endif
}