#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// Override Unity Terrain inspector
    /// </summary>
    [CustomEditor(typeof(Terrain))]
    public class TerrainInspectorEx : BaseEditorEx
    {
        static readonly GUIContent 
            GUI_EXPORT_HEIGHTMAP = new GUIContent("HeightMap", "export heightmap to exportTerrainMapPath"),
            GUI_EXPORT_HOLESMAP = new GUIContent("HolesMap", "export holesMap to exportTerrainMapPath"),
            GUI_EXPORT_CONTROLMAPS = new GUIContent("ControlMaps", "export controlMaps to exportTerrainMapPath"),
            GUI_EXPORT_MAP_PATH = new GUIContent("Map Path","export maps to this folder"),


            GUI_SHOW_CONTROLMAP = new GUIContent("ControlMaps","show controlMap(SplatAlpha) , replace"),
            GUI_SHOW_HEIGHT_MAP = new GUIContent("HeightMap","show heightmap, replace"),
            GUI_SHOW_HOLES_MAP = new GUIContent("HolesMap","show holesmap, replace")

            ;


        public string exportMapPath = "Assets/TerrainMaps";
        public bool 
            isExportMapFolded,
            isControlMapFolded,
            isHeightMapFolded,
            isHolesMapFolded,
            isUpdateHeightmapResolution
            ;

        public float holesmapThreshold = 0.5f;

        public Texture2D targetReplaceMap;


        // for export maps
        List<(Texture2D texture,string path)> exportMapInfoList = new();

        public override string GetDefaultInspectorTypeName() => "UnityEditor.TerrainInspector";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var terrain = (Terrain)target;
            var td = terrain.terrainData;
            DrawTerrainData(terrain);

            if (!td)
            {
                EditorGUILayout.HelpBox($"{terrain} terrainData missing", MessageType.Info);
                return;
            }

            DrawExportMapsButton(td);
            DrawControlMaps(td);
            DrawHeightMap(td);
            DrawHolesMap(td);
        }

        private void DrawHeightMap(TerrainData td)
        {
            isHeightMapFolded = EditorGUILayout.Foldout(isHeightMapFolded, GUI_SHOW_HEIGHT_MAP, true);
            if (!td || !isHeightMapFolded)
                return;

            // heightmap
            //GUILayout.Label(GUI_UPDATE_HEIGHTMAP_RESOLUTION);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            //isUpdateHeightmapResolution = GUILayout.Toggle(isUpdateHeightmapResolution, GUI_UPDATE_HEIGHTMAP_RESOLUTION);
            
            isUpdateHeightmapResolution = EditorGUILayout.ToggleLeft(GUIContentEx.TempContent("Update resolution", "sync terrainData heightmap resolution with texture"),isUpdateHeightmapResolution);

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            DrawTerrainMap(td.heightmapTexture, ref targetReplaceMap);
            if (targetReplaceMap && EditorUtility.DisplayDialog("Waring", $"replace {td} heightmap", "ok"))
            {
                td.ApplyHeightmap(targetReplaceMap, isUpdateHeightmapResolution);
                targetReplaceMap = null;
            }

            if (GUILayout.Button(GUIContentEx.TempContent("Clear","Clear heightmap")))
            {
                td.ApplyHeightmap(Texture2D.blackTexture);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawHolesMap(TerrainData td)
        {
            isHolesMapFolded = EditorGUILayout.Foldout(isHolesMapFolded, GUI_SHOW_HOLES_MAP, true);
            if (!td || !isHolesMapFolded)
                return;

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(GUIContentEx.TempContent("HolesMap Threshold", "> threshold is hole"));
            holesmapThreshold = EditorGUILayout.Slider(holesmapThreshold, 0, 1);
            // holes map
            DrawTerrainMap(td.holesTexture, ref targetReplaceMap);
            if (targetReplaceMap && EditorUtility.DisplayDialog("Waring", $"replace {td} holesmap", "ok"))
            {
                td.SetHolesMap(targetReplaceMap, holesmapThreshold);
                targetReplaceMap = null;
            }

            if (GUILayout.Button(GUIContentEx.TempContent("Clear","Clear holesmap")))
            {
                td.SetHolesMap(Texture2D.whiteTexture);
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw Terrain alphamapTextures
        /// </summary>
        /// <param name="terrain"></param>
        private void DrawControlMaps(TerrainData td)
        {
            isControlMapFolded = EditorGUILayout.Foldout(isControlMapFolded, GUI_SHOW_CONTROLMAP, true);
            if (!td || !isControlMapFolded)
                return;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            for (int i = 0; i< td.alphamapTextures.Length;i++)
            {
                var tex = td.alphamapTextures[i];

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                DrawTerrainMap(tex, ref targetReplaceMap);
                if (targetReplaceMap && EditorUtility.DisplayDialog("Waring", $"replace {td} {tex}", "ok"))
                {
                    var map = targetReplaceMap;
                    targetReplaceMap = null;
                    td.ApplyAlphamap(i, map);
                    break;
                }
                if(GUILayout.Button($"Clear {tex.name}"))
                {
                    td.ApplyAlphamap(i, Texture2D.blackTexture);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draw terrain map preview,replace object field in a line
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="replaceTex"></param>
        public static void DrawTerrainMap(Texture tex,ref Texture2D replaceTex)
        {
            GUILayout.BeginHorizontal();
            //preview
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(20));
            rect.width = EditorGUIUtility.singleLineHeight;
            EditorGUI.DrawPreviewTexture(rect, tex);
            // name
            GUILayout.Label(tex.name);

            // replace 
            //GUILayout.Label(GUI_REPLACE_CONTROLMAP);
            replaceTex = (Texture2D)EditorGUILayout.ObjectField(replaceTex, typeof(Texture2D), false);
            GUILayout.EndHorizontal();
        }

        public void DrawExportMapsButton(TerrainData td)
        {
            isExportMapFolded = EditorGUILayout.Foldout(isExportMapFolded, "Export Maps", true);
            if (!isExportMapFolded || !td)
                return;

            exportMapInfoList.Clear();

            EditorGUI.indentLevel++;
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                exportMapPath = EditorGUILayout.TextField(GUI_EXPORT_MAP_PATH, exportMapPath);
                PathTools.CreateAbsFolderPath(exportMapPath);

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                if (GUILayout.Button(GUI_EXPORT_HEIGHTMAP))
                {
                    var path = $"{exportMapPath}/{td.name}_heightmap.tga";
                    exportMapInfoList.Add((td.GetHeightmap(), path));

                }
                if (GUILayout.Button(GUI_EXPORT_HOLESMAP))
                {
                    var path = $"{exportMapPath}/{td.name}_holesmap.tga";
                    exportMapInfoList.Add((td.GetHolesMap(), path));
                }
                if (GUILayout.Button(GUI_EXPORT_CONTROLMAPS))
                {
                    exportMapInfoList.AddRange(td.alphamapTextures.Select(tex => (
                    tex,
                    $"{exportMapPath}/{td.name}_{tex.name}.tga"
                    )));
                }
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();

            EditorGUI.indentLevel--;

            if (exportMapInfoList.Count > 0)
            {
                foreach (var texturePath in exportMapInfoList)
                {
                    File.WriteAllBytes(texturePath.path, texturePath.texture.GetEncodeBytes(TextureEncodeType.TGA));
                }

                AssetDatabaseTools.SaveRefresh();
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(exportMapPath));
            }
        }

        public void DrawTerrainData(Terrain terrain)
        {
            var terrainDataProp = serializedObject.FindProperty("m_TerrainData");
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.ObjectField(terrainDataProp);
            serializedObject.ApplyModifiedProperties();
            // terrain collider data
            var tc = terrain.GetComponent<TerrainCollider>();
            if (tc && tc.terrainData && tc.terrainData != terrainDataProp.objectReferenceValue)
            {
                tc.terrainData = (TerrainData)terrainDataProp.objectReferenceValue;
            }
        }
    }
}
#endif