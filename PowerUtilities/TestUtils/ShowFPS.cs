using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
namespace PowerUtilities.Test
{
    public class ShowFPS : MonoBehaviour
    {
        public Text overviewText;
        //public TMPro.TextMeshProUGUI textMeshProUGUI;

        [Range(30,2000)]public int maxFps=2000;
        public bool isUseSmoothTime;

        [Header("ShaderLod")]
        public int shaderLod = 600;
        [EditorButton(onClickCall = "OnSetShaderLod")]public bool isSetShaderLod;

        [Header("Texture Lod")]
        [Range(0,3)]public int textureLod = 0;
        [EditorButton(onClickCall = "OnSetTextureLod")] public bool isSetTextureLod;

        int fpsCounter;
        float startTime;
        StringBuilder overviewSB = new StringBuilder();
        string fpsStr;
        // Start is called before the first frame update
        void Start()
        {
            Application.targetFrameRate = maxFps;

            if (!overviewText)
                enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            Application.targetFrameRate = maxFps;

            UpdateFps();
        }

        private void UpdateOverviewInfo()
        {
            if (!overviewText)
                return;

            overviewSB.Clear();
            overviewSB.AppendLine($"FPS: {fpsStr}");
            overviewSB.AppendLine($"frameRate: {Application.targetFrameRate}");
            overviewSB.AppendLine($"Screen: {Screen.width}x{Screen.height}");
            overviewSB.AppendLine($"ShaderLod: {Shader.globalMaximumLOD}");
            overviewSB.AppendLine($"TextureLod: {QualitySettings.globalTextureMipmapLimit}");
#if UNITY_EDITOR
            var inputMode = (ProjectSettingManagers.InputMode)ProjectSettingManagers.GetAsset(ProjectSettingManagers.ProjectSettingTypes.ProjectSettings).FindProperty("activeInputHandler").intValue;
            overviewSB.AppendLine($"InputMode: {inputMode}");
#endif

            overviewText.text = overviewSB.ToString();
        }

        private void UpdateFps()
        {
            if (isUseSmoothTime)
            {
                var testFPS = 1f / Time.smoothDeltaTime;
                fpsStr = testFPS.ToString("F1");
                UpdateOverviewInfo();
                return;
            }

            if (Time.unscaledTime - startTime > 1)
            {
                fpsStr = fpsCounter.ToString();
                UpdateOverviewInfo();

                startTime = Time.unscaledTime;
                fpsCounter = 0;
            }

            fpsCounter++;
        }

        public void OnSetShaderLod(int lod)
        {
            Shader.globalMaximumLOD = lod;
        }

        public void OnSetShaderLod()
        {
            OnSetShaderLod(shaderLod);
        }

        public void OnSetTextureLod()
        {
            OnSetTextureLod(textureLod);
        }
        public void OnSetTextureLod(int textureLod)
        {
#if UNITY_2022_1_OR_NEWER
            QualitySettings.globalTextureMipmapLimit = textureLod;
#else
            QualitySettings.masterTextureLimit = textureLod;
#endif
        }
    }
}