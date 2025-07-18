#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    [Serializable]
    public class PowerShaderMinVersionInfoSO : ScriptableObject
    {
        [Header("Shader")]
        public Shader sourceShader;

        [Header("Folder")]
        [Tooltip("Check materials in this folder")]
        public Object folderObj;

        [Header("MinVersion Keyword")]
        [Tooltip("min version dont use this keywords")]
        public string[] keywords;

        [Tooltip("checked, min version use this keywords")]
        public bool isKeywordsValidReverse;

        [Header("MinVersion properties")]
        [Tooltip("min version dont use this props")]
        public string[] materialProps;

        [Tooltip("checked, min version use this props")]
        public bool isPropValidReverse;

        [EditorButton(onClickCall = "Check")]
        public bool isCheck;

        [Multiline]
        public string log;

        public void Check()
        {
            var folder = AssetDatabaseTools.GetAssetFolder(folderObj);

            var count = PowerShaderMinVersionChecker.CheckMinVersion(folder, sourceShader, keywords, materialProps,
                isKeywordValidReverse: isKeywordsValidReverse,
                isPropValidReverse: isPropValidReverse
                );

            log = $"Checked :{count}";
        }

    }
}

#endif