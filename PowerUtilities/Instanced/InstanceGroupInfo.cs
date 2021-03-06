using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    [Serializable]
    public class InstancedTransformGroup
    {
        public List<Matrix4x4> transforms = new List<Matrix4x4>();
    }
    [Serializable]
    public class InstancedLightmapCoordGroup
    {
        public List<Vector4> lightmapCoords = new List<Vector4>();
    }

    /// <summary>
    /// 对instancing的物体,按1023个进行分组.
    /// groupId是originalTransformsGroup的索引,同时表示绘制中的相同的批次
    /// </summary>
    [Serializable]
    public class InstancedGroupInfo
    {
        public Mesh mesh;
        public Material mat;

        /// <summary>
        /// 要绘制的物体,untiy限制最多1023,这里用list来分组.
        /// 存放所有的 变换信息
        /// </summary>
        //public List<List<Matrix4x4>> originalTransformsGroup = new List<List<Matrix4x4>>();
        public List<InstancedTransformGroup> originalTransformsGroupList = new List<InstancedTransformGroup>();
        /// <summary>
        /// 用于绘制的列表
        /// 对originalTransformList进行洗牌,按概率过滤一部分
        /// </summary>
        //public List<List<Matrix4x4>> displayTransformsGroup = new List<List<Matrix4x4>>();
        public List<InstancedTransformGroup> displayTransformsGroupList = new List<InstancedTransformGroup>();

        /// <summary>
        /// 绘制物体的光照图uv分组
        /// </summary>
        //public List<List<Vector4>> lightmapCoordsList = new List<List<Vector4>>();
        public List<InstancedLightmapCoordGroup> lightmapCoordsList = new List<InstancedLightmapCoordGroup>();

        /// <summary>
        /// 每组(1023个)会分配一个 block
        /// </summary>
        public List<MaterialPropertyBlock> blockList = new List<MaterialPropertyBlock>();

        public int lightmapId;

        int groupId = 0;
        public void AddRender(Matrix4x4 transform, Vector4 lightmapST)
        {
            // new group
            if (originalTransformsGroupList.Count <= groupId)
            {
                originalTransformsGroupList.Add(new InstancedTransformGroup());
                displayTransformsGroupList.Add(new InstancedTransformGroup());
                lightmapCoordsList.Add(new InstancedLightmapCoordGroup());
                blockList.Add(new MaterialPropertyBlock());
            }
            // get current group
            var transformGroup = originalTransformsGroupList[groupId];
            transformGroup.transforms.Add(transform);

            var lightmapSTGroup = lightmapCoordsList[groupId];
            lightmapSTGroup.lightmapCoords.Add(lightmapST);

            var transformsCulledGroup = displayTransformsGroupList[groupId];
            transformsCulledGroup.transforms.Add(transform);

            // check need increment groupId.
            if (transformGroup.transforms.Count >= 1023)
            {
                groupId++;
            }
        }
    }
}
