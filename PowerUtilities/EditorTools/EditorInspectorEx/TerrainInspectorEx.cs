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

        public string exportMapPath = "Assets/TerrainMaps";
        public bool 
            isExportMapFolded,
            isControlMapFolded,
            isHeightMapFolded,
            isHolesMapFolded,
            isDetailMapFolded,
            isTerrainToolsFolded,
            isUpdateHeightmapResolution
            ;

        public float holesmapThreshold = 0.5f;
        public float heightmapScale = 1;

        public Texture2D targetReplaceMap;
        public List<Texture2D> detailTextureList = new();
        // details
        public float detailMapThreshold = 0.5f;
        public float detailMaxDensity = 500;


        // for export maps
        List<(Texture2D texture,string path)> exportMapInfoList = new();

        public override string GetDefaultInspectorTypeName() => "UnityEditor.TerrainInspector";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var terrain = (Terrain)target;
            var td = terrain.terrainData;

            EditorGUITools.BeginVerticalBox(() =>
            {
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
                DrawDetailMap(td);
                DrawTerrainTools(terrain);

            }, nameof(EditorStylesEx.HelpBox));

        }

        private void DrawTerrainTools(Terrain terrain)
        {
            isTerrainToolsFolded = EditorGUILayout.Foldout(isTerrainToolsFolded, GUIContentEx.TempContent("TerrainTools", "show Terrain tools"), true);
            if (!isTerrainToolsFolded)
                return;

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                if (GUILayout.Button("TileTerrainWindow"))
                    EditorApplication.ExecuteMenuItem(TileTerrainWindow.SHOW_TILE_TERRAIN_WINDOW);

                if (GUILayout.Button(GUIContentEx.TempContent("StampControl", "Add TerrainStampControl")))
                {
                    terrain.gameObject.GetOrAddComponent<TerrainStampControl>();
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawDetailMap(TerrainData td)
        {
            isDetailMapFolded = EditorGUILayout.Foldout(isDetailMapFolded, GUIContentEx.TempContent("DetailMap", "show detailmap, replace"), true);
            if (!td || !isDetailMapFolded)
                return;

            if (detailTextureList.Count != td.detailPrototypes.Length)
                detailTextureList.Clear();

            if (detailTextureList.Count == 0)
            {
                for (int i = 0; i < td.detailPrototypes.Length; i++)
                    detailTextureList.Add(td.GetDetailLayerTexture(i));
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                // options
                detailMapThreshold = EditorGUILayout.Slider(GUIContentEx.TempContent("threshold", "detail map pixel threshold"), detailMapThreshold, 0, 1);
                detailMaxDensity = EditorGUILayout.Slider(GUIContentEx.TempContent("maxDensity", "detail map max density"), detailMaxDensity, 0, 1000);
                // detail maps
            }
            GUILayout.EndVertical();

            for (int i = 0; i < detailTextureList.Count; i++)
            {
                var tex = detailTextureList[i];
                GUILayout.BeginHorizontal(EditorStyles.helpBox);

                var isNeedRefresh = DrawTerrainMap(tex, ref targetReplaceMap, () =>
                {
                    if (EditorUtility.DisplayDialog("Waring", $"replace {td} {i} details", "ok"))
                    {
                        td.SetDetailLayerTexture(i, targetReplaceMap, detailMapThreshold, detailMaxDensity);
                    }
                });
                if (GUILayout.Button($"Clear {tex.name}"))
                {
                    td.SetDetailLayerTexture(i, Texture2D.blackTexture);
                    isNeedRefresh = true;
                }
                GUILayout.EndHorizontal();

                if (isNeedRefresh)
                {
                    detailTextureList[i] = td.GetDetailLayerTexture(i);
                }
            }
 
        }

        private void DrawHeightMap(TerrainData td)
        {
            isHeightMapFolded = EditorGUILayout.Foldout(isHeightMapFolded, GUIContentEx.TempContent("HeightMap", "show heightmap, replace"), true);
            if (!td || !isHeightMapFolded)
                return;

            // heightmap options
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                isUpdateHeightmapResolution = EditorGUILayout.Toggle(GUIContentEx.TempContent("Update heightmap resolution", "sync terrainData heightmap resolution with texture"), isUpdateHeightmapResolution);
                heightmapScale = EditorGUILayout.Slider(GUIContentEx.TempContent("HeightmapScale","heightmap pixel scale"),heightmapScale, 0, 1);
            }
            GUILayout.EndVertical();

            // heightmap
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                DrawTerrainMap(td.heightmapTexture, ref targetReplaceMap, () =>
                {
                    if (EditorUtility.DisplayDialog("Waring", $"replace {td} heightmap", "ok"))
                    {
                        td.ApplyHeightmap(targetReplaceMap, isUpdateHeightmapResolution, heightmapScale);
                    }
                });

                if (GUILayout.Button(GUIContentEx.TempContent("Clear", "Clear heightmap")))
                {
                    td.ApplyHeightmap(Texture2D.blackTexture);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawHolesMap(TerrainData td)
        {
            isHolesMapFolded = EditorGUILayout.Foldout(isHolesMapFolded, GUIContentEx.TempContent("HolesMap", "show holesmap, replace"), true);
            if (!td || !isHolesMapFolded)
                return;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                holesmapThreshold = EditorGUILayout.Slider(GUIContentEx.TempContent("HolesMap Threshold", "> threshold is hole"), holesmapThreshold, 0, 1);

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                // holes map
                DrawTerrainMap(td.holesTexture, ref targetReplaceMap, () =>
                {
                    if (EditorUtility.DisplayDialog("Waring", $"replace {td} holesmap", "ok"))
                    {
                        td.SetHolesMap(targetReplaceMap, holesmapThreshold);
                    }
                });

                if (GUILayout.Button(GUIContentEx.TempContent("Clear", "Clear holesmap")))
                {
                    td.SetHolesMap(Texture2D.whiteTexture);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw Terrain alphamapTextures
        /// </summary>
        /// <param name="terrain"></param>
        private void DrawControlMaps(TerrainData td)
        {
            isControlMapFolded = EditorGUILayout.Foldout(isControlMapFolded,GUIContentEx.TempContent("ControlMaps", "show controlMap(SplatAlpha) , replace"), true);
            if (!td || !isControlMapFolded)
                return;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            for (int i = 0; i < td.alphamapTextures.Length; i++)
            {
                var tex = td.alphamapTextures[i];

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    var isCall = DrawTerrainMap(tex, ref targetReplaceMap, () =>
                    {
                        if (EditorUtility.DisplayDialog("Waring", $"replace {td} {tex}", "ok"))
                        {
                            td.ApplyAlphamap(i, targetReplaceMap);
                        }
                    });
                    if (isCall)
                        break;

                    if (GUILayout.Button($"Clear {tex.name}"))
                    {
                        td.ApplyAlphamap(i, Texture2D.blackTexture);
                    }
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
        /// <param name="onReplace">callback when replaceTex valid</param>
        public static bool DrawTerrainMap(Texture tex,ref Texture2D replaceTex,Action onReplace)
        {
            GUILayout.BeginHorizontal();
            //preview
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(20));
            rect.width = EditorGUIUtility.singleLineHeight;
            EditorGUI.DrawPreviewTexture(rect, tex);
            // name
            GUILayout.Label(tex.name);

            // replace 
            replaceTex = (Texture2D)EditorGUILayout.ObjectField(replaceTex, typeof(Texture2D), false);
            GUILayout.EndHorizontal();

            if (replaceTex && onReplace != null)
            {
                onReplace();
                replaceTex = null;
                return true;
            }
            return false;
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
                exportMapPath = EditorGUILayout.TextField(GUIContentEx.TempContent("Map Path", "export maps to this folder"), exportMapPath);
                PathTools.CreateAbsFolderPath(exportMapPath);

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                if (GUILayout.Button(GUIContentEx.TempContent("HeightMap", "export heightmap to exportTerrainMapPath")))
                {
                    var path = $"{exportMapPath}/{td.name}_heightmap.tga";
                    exportMapInfoList.Add((td.GetHeightmap(), path));

                }
                if (GUILayout.Button(GUIContentEx.TempContent("HolesMap", "export holesMap to exportTerrainMapPath")))
                {
                    var path = $"{exportMapPath}/{td.name}_holesmap.tga";
                    exportMapInfoList.Add((td.GetHolesMap(), path));
                }
                if (GUILayout.Button(GUIContentEx.TempContent("ControlMaps", "export controlMaps to exportTerrainMapPath")))
                {
                    exportMapInfoList.AddRange(td.alphamapTextures.Select(tex => (
                    tex,
                    $"{exportMapPath}/{td.name}_{tex.name}.tga"
                    )));
                }

                if(GUILayout.Button(GUIContentEx.TempContent("DetailMaps", "export detail maps to exportTerrainMapPath")))
                {
                    List<Texture2D> texList = null;
                    td.GetDetailLayerTextures(ref texList);
                    exportMapInfoList.AddRange(texList.Select(tex => (tex, $"{exportMapPath}/{td.name}_{tex.name}.tga")));
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