#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{

    public class PowerAssetImporterSetting : ScriptableObject
    {
        public const string PATH = "Assets/PowerUtilities/PowerAssetImporterSettings.asset";

        [ProjectSetttingField]
        public bool isRemoveMixamoRig;


        public static PowerAssetImporterSetting GetInstance()
            => ScriptableObjectTools.GetInstance<PowerAssetImporterSetting>(PATH);

        public static SerializedObject GetSerializedInstance()
            => ScriptableObjectTools.GetSerializedInstance<PowerAssetImporterSetting>(PATH);

    }
}
#endif