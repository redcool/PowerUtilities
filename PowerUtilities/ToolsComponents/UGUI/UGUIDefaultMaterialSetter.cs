using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PowerUtilities
{
    public class UGUIDefaultMaterialSetter : MonoBehaviour
    {
        [EditorButton(onClickCall = "UpdateUGUIDefaultMat")]
        public bool isTest;

        public PresetBlendMode blendMode = PresetBlendMode.AlphaBlend;
        [Range(0, 1)] public float _GlyphMin = 0.4f;
        [Range(0, 1)] public float _GlyphMax = 0.6f;

        // Start is called before the first frame update
        void Start()
        {

        }


        // Update is called once per frame
        public void UpdateUGUIDefaultMat()
        {
            var mat = Graphic.defaultGraphicMaterial;
            mat.SetPresetBlendMode(blendMode);
            mat.SetVector("_GlyphRange", new Vector4(_GlyphMin, _GlyphMax));
        }
    }
}
