using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
namespace PowerUtilities.Test
{
    public class ShowFPS : MonoBehaviour
    {
        public Text fpsText;
        //public TMPro.TextMeshProUGUI textMeshProUGUI;

        [Range(30,2000)]public int maxFps=2000;
        public bool isUseSmoothTime;

        [Header("ShaderLod")]
        public int shaderLod = 600;
        [EditorButton(onClickCall = "OnSetShaderLod")]public bool isSetShaderLod;

        [Header("Texture Lod")]
        [Range(0,3)]public int textureLod = 0;
        [EditorButton(onClickCall = "OnSetTextureLod")] public bool isSetTextureLod;

        int fps;
        float startTime;
        // Start is called before the first frame update
        void Start()
        {
            Application.targetFrameRate = maxFps;

            if (!fpsText)
                enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            Application.targetFrameRate = maxFps;

            if (isUseSmoothTime)
            {
                var testFPS = 1f / Time.smoothDeltaTime;
                fpsText.text = testFPS.ToString("F1");
                return;
            }

            if (Time.unscaledTime - startTime > 1)
            {

                if(fpsText)
                    fpsText.text = fps.ToString();

                //if(textMeshProUGUI)
                //    textMeshProUGUI.text = fps.ToString();


                startTime = Time.unscaledTime;
                fps = 0;
            }

            fps++;
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