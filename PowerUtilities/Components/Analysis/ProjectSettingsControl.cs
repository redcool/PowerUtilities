namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ProjectSettingsControl : MonoBehaviour
    {
        public Text fpsText;

        public int masterTextureMipmap = 0;

        int frameCount;
        float curSeconds;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            ShowFPS();
        }

        void ShowFPS()
        {
            if (!fpsText)
                return;

            //fpsText.text = (1f / Time.smoothDeltaTime).ToString("f2");
            frameCount++;

            if((Time.time - curSeconds) > 1)
            {
                fpsText.text = frameCount.ToString("f2");
                curSeconds = Time.time;
                frameCount = 0;
            }
        }

        public void ToggleShadow()
        {
            QualitySettings.shadows = QualitySettings.shadows == ShadowQuality.Disable ? ShadowQuality.HardOnly : ShadowQuality.Disable;
        }

        public void ToggleTextureMipmapLevel()
        {
            masterTextureMipmap++;
            masterTextureMipmap %= 2;

#if UNITY_2023_1_OR_NEWER
            QualitySettings.globalTextureMipmapLimit = masterTextureMipmap;
#else
            QualitySettings.masterTextureLimit = masterTextureMipmap;
#endif
        }

        public void ToggleShadowMaskMode()
        {
            QualitySettings.shadowmaskMode = QualitySettings.shadowmaskMode == ShadowmaskMode.DistanceShadowmask ? ShadowmaskMode.Shadowmask : ShadowmaskMode.DistanceShadowmask;
        }

        public void ToggleKeyword(string key)
        {
            if (Shader.IsKeywordEnabled(key))
            {
                Shader.DisableKeyword(key);
            }
            else
            {
                Shader.EnableKeyword(key);
            }
        }
    }
}