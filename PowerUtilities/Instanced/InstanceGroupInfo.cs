using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerUtilities
{

    [Serializable]
    public class InstancedGroupInfo
    {
        public int lightmapId;
        public Mesh mesh;
        public Material originalMat;

        public int instanceCount;

        public int globalOffset; // cullingGroup index offset,start index in boundingSpheres
        /// <summary>
        /// localToWorld per object
        /// </summary>
        public Matrix4x4[] transforms;
        /// <summary>
        /// Lightmapcoord scale and offset per object
        /// </summary>
        public Vector4[] lightmapCoords;
        /// <summary>
        /// lightamapId per object, lightmapArray use this list
        /// </summary>
        //public float[] lightmapIds;

        public Vector4[] boundingSpheres;

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
        public InstancedGroupInfo(int instanceCount)
        {
            this.instanceCount = instanceCount;
            transforms = new Matrix4x4[instanceCount];
            lightmapCoords = new Vector4[instanceCount];
            //lightmapIds = new float[instanceCount];
            boundingSpheres = new Vector4[instanceCount];
        }

        public void Reset()
        {
            matInstance = null;
        }
        public void Clear()
        {
            matInstance = null;
            transforms = null;
            lightmapCoords = null;
            //lightmapIds = null;
            boundingSpheres = null;
        }

        public int UpdateVisibles(CullingGroup cullingGroup,Matrix4x4[] visibleTransforms,Vector4[] visibleLightmapCoords, int[] visibleIndices)
        {
            if (visibleTransforms == null)
                return 0;

            var visibleCount = cullingGroup.QueryIndices(true,visibleIndices,globalOffset,instanceCount);
            
            for (int i = 0; i < visibleCount; i++)
            {
                visibleTransforms[i] = transforms[visibleIndices[i]];

                if(visibleLightmapCoords != null)
                    visibleLightmapCoords[i] = lightmapCoords[visibleIndices[i]];
            }

            return visibleCount;
        }

    }
}
