
namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [Serializable]
    public class BrgGroupInfo
    {
        [Tooltip("{mesh}_{material}_{instanceCount}")]
        public string groupName;
        public int lightmapId;
        public int floatsCount;

        [Header("Mesh")]
        public Mesh mesh;

        [Header("Material Info")]
        public Material mat;

        [ListItemDraw("name:,propName,type:,propType,floats:,floatsCount", "40,200,40,80,40,")]
        public List<CBufferPropInfo> matGroupList = new();

        [Header("Instance Info")]
        public int instanceCount;
        public List<Renderer> rendererList = new();

        [Tooltip("shader for batch renderers, RecordChildren auto set")]
        public BRGMaterialInfo brgMaterialInfo;

    }
}
