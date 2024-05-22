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

        // Start is called before the first frame update
        void Start()
        {

        }


        // Update is called once per frame
        public void UpdateUGUIDefaultMat()
        {
            Graphic.defaultGraphicMaterial.SetPresetBlendMode(blendMode);
        }
    }
}
