#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using System;

namespace PowerUtilities
{

    /// <summary>
    /// 管理材质上代码绘制的属性
    /// 这是属性需要定义在Layout.txt里,通过代码来控制材质的状态.
    /// </summary>
    public class MaterialCodeProps
    {
        public enum CodePropNames
        {
            _PresetBlendMode,
            _RenderQueue,
            _ToggleGroups,
            _BakedEmission,
        }

        Dictionary<string, bool> materialCodePropDict = new Dictionary<string, bool>();
        public void Clear()
        {
            var keys = materialCodePropDict.Keys.ToList();
            foreach (var item in keys)
            {
                materialCodePropDict[item] = false;
            }
        }

        private MaterialCodeProps()
        {
            var names = Enum.GetNames(typeof(CodePropNames));
            foreach (var item in names)
            {
                materialCodePropDict[item] = false;
            }
        }
        private static MaterialCodeProps instance;

        public static MaterialCodeProps Instance
        {
            get
            {
                if (instance == null)
                    instance = new MaterialCodeProps();
                return instance;
            }
        }

        public void InitMaterialCodeVars(string propName)
        {
            if (materialCodePropDict.ContainsKey(propName))
            {
                materialCodePropDict[propName] = true;
            }
        }

        public bool IsPropExists(CodePropNames propName)
        {
            var key = propName.ToString();
            materialCodePropDict.TryGetValue(key, out var isExist);
            return isExist;
        }
    }

    public class PowerShaderInspector : ShaderGUI
    {
        // events
        public event Action<MaterialProperty, Material> OnDrawProperty;
        public event Action<Dictionary<string, MaterialProperty>, Material> OnDrawPropertyFinish;

        public string shaderName = ""; //子类来指定,用于EditorPrefs读写

        string[] tabNames;
        bool[] tabToggles;
        List<int> tabSelectedIds = new List<int>();

        List<string[]> propNameList = new List<string[]>();
        string materialSelectedId => shaderName + "_SeletectedId";
        string materialToolbarCount => shaderName + "_ToolbarCount";

        string GetMaterialSelectionIdKey(string matName)
        {
            return matName + shaderName + "_SeletectedId";
        }

        //int selectedTabId;
        bool showOriginalPage;

        Dictionary<string, MaterialProperty> propDict;
        Dictionary<string, string> propNameTextDict;
        Dictionary<string, string> colorTextDict;
        Dictionary<string, string> propHelpDict;

        bool isFirstRunOnGUI = true;
        string helpStr;
        string[] tabNamesInConfig;

        Shader lastShader;

        MaterialEditor materialEditor;
        PresetBlendMode presetBlendMode;
        int toolbarCount = 5;

        Color defaultContentColor;
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

                defaultContentColor = GUI.contentColor;
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

            // draw additional options 
            if (IsTargetShader(mat))
            {
                //draw preset blend mode
                if (MaterialCodeProps.Instance.IsPropExists(MaterialCodeProps.CodePropNames._PresetBlendMode))
                    MaterialEditorGUITools.DrawBlendMode(mat, ConfigTool.Text(propNameTextDict, "PresetBlendMode"));

                //draw render queue
                if (MaterialCodeProps.Instance.IsPropExists(MaterialCodeProps.CodePropNames._RenderQueue))
                {
                    // render queue, instanced, double sided gi
                    DrawMaterialProps(mat);
                }

                //draw toggle groups
                if (MaterialCodeProps.Instance.IsPropExists(MaterialCodeProps.CodePropNames._ToggleGroups))
                {
                    DrawToggleGroups(mat);
                }
                if (MaterialCodeProps.Instance.IsPropExists(MaterialCodeProps.CodePropNames._BakedEmission))
                    DrawBakedEmission();
            }

            if (OnDrawPropertyFinish != null)
                OnDrawPropertyFinish(propDict, mat);
        }

        private void DrawProperty(MaterialEditor materialEditor, Material mat, string propName)
        {
            // found color
            var contentColor = defaultContentColor;
            string colorString;
            if (colorTextDict.TryGetValue(propName, out colorString))
            {
                ColorUtility.TryParseHtmlString(colorString, out contentColor);
            }
            //show color
            GUI.contentColor = contentColor;
            var prop = propDict[propName];

            MaterialEditorEx.ShaderProperty(materialEditor, prop, ConfigTool.GetContent(propNameTextDict, propHelpDict, prop.name));
            //materialEditor.ShaderProperty(prop, ConfigTool.GetContent(propNameTextDict, propHelpDict, prop.name));

            GUI.contentColor = defaultContentColor;

            if (OnDrawProperty != null)
                OnDrawProperty(prop, mat);
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
            SetupLayout(shaderFilePath);

            propNameTextDict = ConfigTool.ReadConfig(shaderFilePath, ConfigTool.I18N_PROFILE_PATH);

            helpStr = ConfigTool.Text(propNameTextDict, "Help").Replace('|', '\n');

            tabNamesInConfig = tabNames.Select(item => ConfigTool.Text(propNameTextDict, item)).ToArray();
            tabToggles = new bool[tabNamesInConfig.Length];

            colorTextDict = ConfigTool.ReadConfig(shaderFilePath, ConfigTool.COLOR_PROFILE_PATH);
            propHelpDict = ConfigTool.ReadConfig(shaderFilePath, ConfigTool.PROP_HELP_PROFILE_PATH);


        }

        private void SetupLayout(string shaderFilePath)
        {
            var layoutConfigPath = ConfigTool.FindPathRecursive(shaderFilePath, ConfigTool.LAYOUT_PROFILE_PATH);
            var dict = ConfigTool.ReadKeyValueConfig(layoutConfigPath);

            if (!dict.TryGetValue("tabNames", out var tabNamesLine))
                return;

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


        void DrawMaterialProps(Material mat)
        {
            GUILayout.BeginVertical();
            EditorGUILayout.Space(10);

            GUILayout.Label("Material Props", EditorStyles.boldLabel);
            //mat.renderQueue = EditorGUILayout.IntField(ConfigTool.Text(propNameTextDict, "RenderQueue"), mat.renderQueue);
            materialEditor.RenderQueueField();
            materialEditor.EnableInstancingField();
            materialEditor.DoubleSidedGIField();

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