﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    [Serializable]
    public class InstancedTransformGroup
    {
        public List<Matrix4x4> transforms = new List<Matrix4x4>();
        /// <summary>
        /// camera see transform ? 
        /// DrawChildrenInstancedCullingGroupControl will update this array
        /// </summary>
        public List<bool> transformVisibleList = new List<bool>();

        /// <summary>
        /// shuffle culling will update this array
        /// </summary>
        public List<bool> transformShuffleCullingList = new List<bool>();

        /// <summary>
        /// boundSphere radius
        /// </summary>
        public List<float> boundsSphereRadiusList = new List<float>();

        public void Add(Matrix4x4 transform,float boundsSphereRadius) {
            transformVisibleList.Add(true);
            transformShuffleCullingList.Add(false);

            transforms.Add(transform);
            boundsSphereRadiusList.Add(boundsSphereRadius);
        }

#if _DEBUG
        public override string ToString()
        {
            var sb = new StringBuilder();
            //sb.AppendLine("trs: " + StringEx.ToString(transforms));
            sb.AppendLine("cullingGroup : " + StringEx.ToString(transformVisibleList));
            sb.AppendLine("shuffleCull:" + StringEx.ToString(transformShuffleCullingList));
            return sb.ToString();
        }
#endif
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

        // material instance
        Material matInstance;
        public Material mat
        {
            get
            {
                if (matInstance == null)
                    matInstance = Object.Instantiate(originalMat);

                if(!matInstance.enableInstancing)
                    matInstance.enableInstancing = true;

                return matInstance;
            }
        }

        public Material originalMat;
        /// <summary>
        /// 要绘制的物体,untiy限制最多1023,这里用list来分组.
        /// 存放所有的 变换信息
        /// </summary>
        public List<InstancedTransformGroup> originalTransformsGroupList = new List<InstancedTransformGroup>();

        /// <summary>
        /// 绘制物体的光照图uv分组
        /// </summary>
        public List<InstancedLightmapCoordGroup> lightmapCoordsList = new List<InstancedLightmapCoordGroup>();

        public int lightmapId;
        /// <summary>
        /// 每个组,一个lightamapId
        /// </summary>
        public List<float> lightmapIdList = new List<float>();
        
        /// <summary>
        /// max count per transformGroup
        /// </summary>
        public int maxCountPerGroup = 1023;
        
        /// <summary>
        /// instance group id
        /// </summary>
        //public int instanceGroupId;

        /// <summary>
        /// instanceGroup's segment(contains transforms count < 1023)
        /// </summary>
        int groupId = 0;
        public void AddRender(float boundSphereRadius,Mesh mesh,Material mat,Matrix4x4 transform, Vector4 lightmapST,int lightmapId)
        {
            this.mesh = mesh;
            originalMat = mat;

            this.lightmapId = lightmapId;
            // new group
            if (originalTransformsGroupList.Count <= groupId)
            {
                originalTransformsGroupList.Add(new InstancedTransformGroup());
                lightmapCoordsList.Add(new InstancedLightmapCoordGroup());
            }

            // get current group and save all
            var transformGroup = originalTransformsGroupList[groupId];
            transformGroup.Add(transform, boundSphereRadius);
            

            var lightmapSTGroup = lightmapCoordsList[groupId];
            lightmapSTGroup.lightmapCoords.Add(lightmapST);
            //
            lightmapIdList.Add(lightmapId);

            // check need increment groupId.
            if (transformGroup.transforms.Count >= maxCountPerGroup)
            {
                groupId++;
            }
        }

        public void Reset()
        {
            matInstance = null;
        }

    }
}
