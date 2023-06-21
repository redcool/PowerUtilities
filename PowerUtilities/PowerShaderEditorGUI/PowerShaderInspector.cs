#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using System;
using System.IO;

namespace PowerUtilities
{
    /// <summary>
    /// Power shader material gui
    /// need configs:
    ///     Layout
    ///     Colors
    ///     Helps
    ///     i18n
    /// </summary>
    public class PowerShaderInspector : ShaderGUI
    {

        // events
        public event Action<MaterialProperty, Material> OnDrawProperty;
        public event Action<Dictionary<string, MaterialProperty>, Material> OnDrawPropertyFinish;

        /// <summary>
        /// 子类来指定,用于EditorPrefs读写
        /// </summary>
        public string shaderName = "";

        /// <summary>
        /// use txt or json
        /// </summary>
        public bool isLayoutUseJson; 

        string[] tabNames = new string[] { } ;
        bool[] tabToggles = new bool[] { } ;
        List<int> tabSelectedIds = new List<int>();

        List<string[]> propNameList = new List<string[]>();
        //string materialSelectedId => shaderName + "_SeletectedId";
        string materialToolbarCount => shaderName + "_ToolbarCount";

        string GetMaterialSelectionIdKey(string matName)=> matName + shaderName + "_SeletectedId";

        //int selectedTabId;
        bool showOriginalPage;

        // contents from Config files
        Dictionary<string, MaterialProperty> propDict;
        Dictionary<string, string> propNameTextDict;
        Dictionary<string, string> colorTextDict;
        Dictionary<string, string> propHelpDict;

        bool isFirstRunOnGUI = true;
        string helpStr;
        string[] tabNamesInConfig = new string[] { };

        Shader lastShader;

        MaterialEditor materialEditor;
        PresetBlendMode presetBlendMode;
        int toolbarCount = 5;

        bool isAllGroupsOpen;

        static PowerShaderInspector()
        {
            MaterialGroupTools.SetStateAll(true);
        }


        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            this.materialEditor = materialEditor;

            var mat = materialEditor.target as Material;
            propDict = ConfigTool.CacheProperties(properties);

            if (isFirstRunOnGUI || lastShader != mat.shader)
            {
                lastShader = mat.shader;

                isFirstRunOnGUI = false;
                OnInit(mat, properties);
            }

            // title
            if (!string.IsNullOrEmpty(helpStr))
                EditorGUILayout.HelpBox(helpStr, MessageType.Info);

            //show original
            showOriginalPage = GUILayout.Toggle(showOriginalPage, ConfigTool.Text(propNameTextDict, "ShowOriginalPage"));
            if (showOriginalPage)
            {
                base.OnGUI(materialEditor, properties);
                return;
            }

            EditorGUILayout.BeginVertical("Box");
            {
                DrawPageTabs();

                EditorGUILayout.BeginVertical("Box");
                DrawPageDetails(materialEditor, mat);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        void DrawPageDetails(MaterialEditor materialEditor, Material mat)
        {
            if (tabSelectedIds.Count == 0)
            {
                EditorGUILayout.LabelField("No Selected");
            }
            foreach (var tabId in tabSelectedIds)
            {
                var tabName = tabNames[tabId];
                var propNames = propNameList[tabId];

                DrawPageDetail(materialEditor, mat, tabName, propNames);
            }
        }

        /// <summary>
        /// draw properties
        /// </summary>
        private void DrawPageDetail(MaterialEditor materialEditor, Material mat, string tabName, string[] propNames)
        {
            const string WARNING_NO_DETAIL = "No Details";
            if (propNames == null || propNames.Length == 0)
            {
                EditorGUILayout.HelpBox(WARNING_NO_DETAIL, MessageType.Warning, true);
                return;
            }

            // content's tab 
            EditorGUILayout.HelpBox(tabName, MessageType.Info, true);

            MaterialCodeProps.Instance.Clear();

            // contents
            foreach (var propName in propNames)
            {
                MaterialCodeProps.Instance.InitMaterialCodeVars(propName);

                if (!propDict.ContainsKey(propName))
                    continue;

                DrawProperty(materialEditor, mat, propName);
            }

            // draw code props
            DrawCodeProperties(mat);

            if (OnDrawPropertyFinish != null)
                OnDrawPropertyFinish(propDict, mat);
        }

        private void DrawCodeProperties(Material mat)
        {
            if (IsTargetShader(mat))
            {
                //draw preset blend mode
                if (MaterialCodeProps.Instance.IsPropExists(MaterialCodeProps.CodePropNames._PresetBlendMode))
                {
                    MaterialEditorGUITools.DrawBlendMode(mat
                        ,ConfigTool.GetContent(propNameTextDict, propHelpDict, nameof(MaterialCodeProps.CodePropNames._PresetBlendMode))
                        ,ConfigTool.GetPropColor(colorTextDict, nameof(MaterialCodeProps.CodePropNames._PresetBlendMode))
                        );
                }

                //draw render queue
                if (MaterialCodeProps.Instance.IsPropExists(MaterialCodeProps.CodePropNames._RenderQueue))
                {
                    // render queue, instanced, double sided gi
                    DrawMaterialProps(mat,ConfigTool.GetPropColor(colorTextDict,nameof(MaterialCodeProps.CodePropNames._RenderQueue)));
                }

                //draw toggle groups
                if (MaterialCodeProps.Instance.IsPropExists(MaterialCodeProps.CodePropNames._ToggleGroups))
                {
                    DrawToggleGroups(mat);
                }
                if (MaterialCodeProps.Instance.IsPropExists(MaterialCodeProps.CodePropNames._BakedEmission))
                {
                    DrawBakedEmission();
                }
            }
        }

        private void DrawProperty(MaterialEditor materialEditor, Material mat, string propName)
        {
            var prop = propDict[propName];

            EditorGUITools.DrawColorUI(() =>
            {
                MaterialEditorEx.ShaderProperty(materialEditor, prop, ConfigTool.GetContent(propNameTextDict, propHelpDict, prop.name));
                //materialEditor.ShaderProperty(prop, ConfigTool.GetContent(propNameTextDict, propHelpDict, prop.name));
            }, ConfigTool.GetPropColor(colorTextDict, propName), GUI.color);

            if (OnDrawProperty != null)
                OnDrawProperty(prop, mat);
        }

        private bool IsTargetShader(Material mat)
        {
            return mat.shader.name.Contains(shaderName);
        }
        void ReadFromCache()
        {
            // read from cache
            tabSelectedIds.Clear();
            EditorPrefTools.GetList(GetMaterialSelectionIdKey(materialEditor.target.name), ref tabSelectedIds, ",", (idStr) => Convert.ToInt32(idStr));

            for (int i = 0; i < tabSelectedIds.Count; i++)
            {
                var selectedId = tabSelectedIds[i];

                if (selectedId >= tabToggles.Length)
                {
                    selectedId = 0;
                    tabSelectedIds[i] = 0;
                }

                tabToggles[selectedId] = true;
            }

            toolbarCount = EditorPrefs.GetInt(materialToolbarCount, tabNamesInConfig.Length);
        }

        void SaveToCache()
        {
            //cache selectedId
            //EditorPrefs.SetInt(materialSelectedId, selectedTabId);
            EditorPrefTools.SetList(GetMaterialSelectionIdKey(materialEditor.target.name), tabSelectedIds, ",");
            EditorPrefs.SetInt(materialToolbarCount, toolbarCount);
        }

        private void DrawPageTabs()
        {
            ReadFromCache();

            // draw 
            GUILayout.BeginVertical("Box");
            toolbarCount = EditorGUILayout.IntSlider("ToolbarCount:", toolbarCount, 3, tabNamesInConfig.Length);
            //selectedTabId = GUILayout.SelectionGrid(selectedTabId, tabNamesInConfig, toolbarCount, EditorStyles.miniButton);
            EditorGUITools.MultiSelectionGrid(tabNamesInConfig, tabToggles, tabSelectedIds, toolbarCount);
            GUILayout.EndVertical();

            SaveToCache();
        }

        private void OnInit(Material mat, MaterialProperty[] properties)
        {
            if (IsTargetShader(mat))
                presetBlendMode = PresetBlendModeTools.GetPresetBlendMode(mat);

            var shaderFilePath = AssetDatabase.GetAssetPath(mat.shader);

            var profileType = mat.IsKeywordEnabled(ConfigTool.MIN_VERSION) ? ConfigTool.LayoutProfileType.MIN_VERSION : ConfigTool.LayoutProfileType.Standard;
            if (isLayoutUseJson)
                SetupLayoutFromJson(shaderFilePath, profileType);
            else
                SetupLayout(shaderFilePath, profileType);

            propNameTextDict = ConfigTool.ReadConfig(shaderFilePath, ConfigTool.I18N_PROFILE_PATH);

            helpStr = ConfigTool.Text(propNameTextDict, "Help").Replace('|', '\n');

            tabNamesInConfig = tabNames.Select(item => ConfigTool.Text(propNameTextDict, item)).ToArray();
            tabToggles = new bool[tabNamesInConfig.Length];

            colorTextDict = ConfigTool.ReadConfig(shaderFilePath, ConfigTool.COLOR_PROFILE_PATH);
            propHelpDict = ConfigTool.ReadConfig(shaderFilePath, ConfigTool.PROP_HELP_PROFILE_PATH);


        }

        /// <summary>
        /// Setup tabNames, propNameList
        /// </summary>
        /// <param name="shaderFilePath"></param>
        /// <param name="profileType"></param>
        /// <exception cref="Exception"></exception>
        private void SetupLayout(string shaderFilePath,ConfigTool.LayoutProfileType profileType)
        {
            var layoutConfigPath = ConfigTool.FindPathRecursive(shaderFilePath, ConfigTool.GetLayoutProfilePath(profileType));
            var dict = ConfigTool.ReadKeyValueConfig(layoutConfigPath);

            if (!dict.TryGetValue("tabNames", out var tabNamesLine))
            {
                throw new Exception($"layout profile file not found, path :{layoutConfigPath}");
                //return;
            }

            // for tabNames
            tabNames = ConfigTool.SplitBy(tabNamesLine);

            // for tab contents
            propNameList.Clear();
            for (int i = 0; i < tabNames.Length; i++)
            {
                var tabName = tabNames[i];
                if (!dict.ContainsKey(tabName))
                    continue;

                var propNamesLine = dict[tabName];
                var propNames = ConfigTool.SplitBy(propNamesLine);
                propNameList.Add(propNames);
            }
        }

        void SetupLayoutFromJson(string shaderFilePath, ConfigTool.LayoutProfileType profileType)
        {
            var layoutConfigPath = ConfigTool.FindPathRecursive(shaderFilePath, ConfigTool.GetLayoutProfilePath(profileType,"json"));
            var jsonStr = File.ReadAllText(layoutConfigPath);
            var layoutObj = JsonUtility.FromJson<LayoutData>(jsonStr);
            tabNames = layoutObj.tabNames;

            propNameList.Clear();
            for (int i = 0; i < tabNames.Length; i++)
            {
                propNameList.Add(layoutObj.contents[i].array);
            }
        }


        void DrawMaterialProps(Material mat,Color contentColor)
        {
            GUILayout.BeginVertical();
            EditorGUILayout.Space(10);

            GUILayout.Label("Material Props", EditorStyles.boldLabel);
            //mat.renderQueue = EditorGUILayout.IntField(ConfigTool.Text(propNameTextDict, "RenderQueue"), mat.renderQueue);

            EditorGUITools.DrawColorUI(() =>
            {
                materialEditor.RenderQueueField();
                materialEditor.EnableInstancingField();
                materialEditor.DoubleSidedGIField();

            }, contentColor, GUI.color);

            GUILayout.EndVertical();
        }

        void DrawToggleGroups(Material mat)
        {
            MaterialEditorGUITools.DrawField(mat, "Toggle Groups", () =>
            {
                isAllGroupsOpen = EditorGUILayout.Toggle(ConfigTool.Text(propNameTextDict, "_ToggleGroups"), isAllGroupsOpen);
            }, mat =>
            {
                MaterialGroupTools.SetStateAll(isAllGroupsOpen);
            });
        }

        void DrawBakedEmission()
        {
            EditorGUI.BeginChangeCheck();
            //GUILayout.Label("Material ", EditorStyles.boldLabel);
            materialEditor.LightmapEmissionProperty();
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Material mat in materialEditor.targets)
                    mat.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }


    }
}


#endif