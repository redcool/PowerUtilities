namespace PowerUtilities
{
#if UNITY_EDITOR
    using PowerUtilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class TileTerrainWindow : BaseEditorWindow
    {
        /// <summary>
        /// 1,1/2,1/4,1/8,1/16
        /// </summary>
        public enum SaveResolution { Full, Half, Quarter, Eighth, Sixteeth }

        [Serializable]
        public class ExportTileTerrainLODInfo
        {
            public SaveResolution saveResolution = SaveResolution.Half;
            //public int tileRowCount = 1;
            public float screenSzieRate = 1;
        }
        // mesh lodGroup
        public List<ExportTileTerrainLODInfo> lodInfoList = new()
        {
            new ExportTileTerrainLODInfo{ saveResolution = SaveResolution.Half ,screenSzieRate = 0.96f},
            new ExportTileTerrainLODInfo{ saveResolution = SaveResolution.Quarter, screenSzieRate = 0.93f },
            new ExportTileTerrainLODInfo{ saveResolution = SaveResolution.Eighth , screenSzieRate = 0.1f},

        };
        public const string 
            ROOT_PATH = "PowerUtilities/Terrain/Command",
            SHOW_TILE_TERRAIN_WINDOW = ROOT_PATH + "/Tile Terrain Window"
            ;

        Material terrainMat;
        Terrain[] terrainObjs;

        int tileRowCount = 1;
        string tileTerrainFolder = "Assets/TileTerrain";
        private Vector2 scrollPosition;

        ReorderableList lodInfoRList;
        SerializedObject soObj;

        [MenuItem(SHOW_TILE_TERRAIN_WINDOW)]
        static void Init()
        {
            var win = GetWindow<TileTerrainWindow>();
            win.Show();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            soObj = new SerializedObject(this);

            lodInfoRList = EditorGUITools.SetupReorderableList<ExportTileTerrainLODInfo>(soObj,soObj.FindProperty("lodInfoList"));
        }

        public override void OnGUI()
        {
            if (lodInfoRList == null)
                OnEnable();

            base.OnGUI();

            EditorGUILayout.HelpBox("Export Terrain to Mesh", MessageType.Info);

            if (GUILayout.Button("Check Terrain"))
            {
                terrainObjs = FindObjectsOfType<Terrain>();
            }

            if (terrainObjs != null && terrainObjs.Length > 0)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, "HelpBox", GUILayout.Height(200));
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
                EditorGUILayout.HelpBox("Terrain not found", MessageType.Info);
                return;
            }
            lodInfoRList.DoLayoutList();

            //saveResolution = (SaveResolution)EditorGUILayout.EnumPopup(GUIContentEx.TempContent("Save Resolution:","larger value ,mesh density small"), saveResolution);

            tileRowCount = Mathf.Max(1, EditorGUILayout.IntField(GUIContentEx.TempContent("Tile Row Count:","tile count in a row"), Mathf.NextPowerOfTwo(tileRowCount)));
            terrainMat = (Material)EditorGUILayout.ObjectField(GUIContentEx.TempContent("Terrain Material:","material for generated meshRenderer"), terrainMat, typeof(Material), false);
            tileTerrainFolder = EditorGUILayout.TextField(GUIContentEx.TempContent("SavePath", "save mesh to Assets folder"), tileTerrainFolder);

            if (GUILayout.Button("Export"))
            {
                var rootGo = new GameObject($"Terrains Mesh {terrainObjs.Length}");
                foreach (var item in terrainObjs)
                {
                    ExportTerrain(item, rootGo.transform);
                }
            }
        }

        public void ExportTerrain(Terrain terrainObj, Transform rootTr)
        {
            // make folder exist
            if (string.IsNullOrEmpty(tileTerrainFolder))
                tileTerrainFolder = "Assets/TileTerrain";

            var saveFolder = $"{tileTerrainFolder}/{terrainObj.terrainData.name}";
            PathTools.CreateAbsFolderPath(saveFolder);

            // create root object
            var terrainGo = new GameObject(terrainObj.gameObject.name + " Mesh");
            terrainGo.transform.localPosition = terrainObj.gameObject.transform.localPosition;
            terrainGo.transform.localRotation = terrainObj.transform.localRotation;
            terrainGo.transform.localScale = terrainObj.transform.localScale;
            terrainGo.transform.SetParent(rootTr, false);

            var lods = new LOD[lodInfoList.Count+1];
            // generate tile mesh objects
            for (int i = 0; i < lodInfoList.Count; i++)
            {
                var lodInfo = lodInfoList[i];
                var tileList = GenerateTiles(terrainObj, tileRowCount, terrainGo.transform, lodInfo.saveResolution);
                foreach (var tileGo in tileList)
                {
                    var mr = tileGo.GetComponent<MeshRenderer>();
                    if (terrainMat)
                        mr.sharedMaterial = terrainMat;

                    var tileMesh = tileGo.GetComponent<MeshFilter>().sharedMesh;
                    var path = $"{saveFolder}/{tileMesh.name}.asset";
                    AssetDatabase.CreateAsset(tileMesh, path);
                    lods[i] = new LOD(lodInfo.screenSzieRate, new[] { mr });
                }
            }

            var lodGroup = terrainGo.GetOrAddComponent<LODGroup>();
            lodGroup.SetLODs(lods);
            lodGroup.size = 90; //

            AssetDatabaseTools.SaveRefresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath(saveFolder, typeof(Object));
        }

        public static List<GameObject> GenerateTiles(Terrain terrain, int tileRowCount, Transform parent, SaveResolution saveResolution)
        {
            tileRowCount = Mathf.Max(1,Mathf.ClosestPowerOfTwo(tileRowCount)); // need pow of 2
            var resScale = (int)Mathf.Pow(2, Mathf.Max(0,(int)saveResolution));

            var list = new List<GameObject>();

            var td = terrain.terrainData;
            var heightmapSize = (td.heightmapResolution - 1) / tileRowCount;

            var id = 0;
            var count = tileRowCount * tileRowCount;

            for (int x = 0; x < tileRowCount; x++)
            {
                for (int z = 0; z < tileRowCount; z++)
                {
                    var heightmapRect = new RectInt(x * heightmapSize, z * heightmapSize, heightmapSize + 1, heightmapSize + 1);
                    var tileMesh = TerrainTools.GenerateTileMesh(terrain, heightmapRect, resScale);
                    tileMesh.name = $"Tile-{x}_{z}-{saveResolution}";

                    var tileGo = GenerateTileGo(tileMesh.name, tileMesh, parent, terrain);
                    list.Add(tileGo);

                    id++;

                    EditorTools.DisplayProgress(id, count);
                }
            }
            return list;
        }

        public static GameObject GenerateTileGo(string name, Mesh mesh, Transform parent, Terrain terrain)
        {
            var tileGo = new GameObject(name);
            tileGo.transform.SetParent(parent, false);

            var mr = tileGo.AddComponent<MeshRenderer>();
            TerrainTools.CopyFromTerrain(mr, terrain);

            var mf = tileGo.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            tileGo.AddComponent<MeshCollider>();

            return tileGo;
        }

    }
#endif
}