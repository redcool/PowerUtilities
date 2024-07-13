#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public class TestAssetPostProcessorControl
    {
        [InitializeOnLoadMethod]
        static void OnInit()
        {
            AssetPostProcessorControl.onPreprocessAsset += (imp) =>
            {
                //Debug.Log("onPreprocessAsset "+imp);
            };

            AssetPostProcessorControl.onPostProcessAsset += AssetPostProcessorControl_onPostProcessAsset;
        }

        private static void AssetPostProcessorControl_onPostProcessAsset(AssetImporter imp, UnityEngine.Object obj)
        {
            //Debug.Log("post imported : " + imp + ":" + obj);
        }
    }
}
#endif