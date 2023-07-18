using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    public class MotionVectorData
    {
        static Dictionary<Camera, Matrix4x4> dict = new Dictionary<Camera, Matrix4x4>();

        static MotionVectorData instance;
        int frameCount;
        public static MotionVectorData Instance()
        {
            if(instance == null)
            {
                instance = new MotionVectorData();
            }
            return instance;
        }
        /// <summary>
        /// call GetPreviousVP after call this
        /// </summary>
        /// <param name="cam"></param>
        public void Update(Camera cam)
        {
            if (Time.frameCount - frameCount == 0)
                return;

            frameCount = Time.frameCount;

            var vp = GL.GetGPUProjectionMatrix(cam.projectionMatrix,true) * cam.worldToCameraMatrix;
            dict[cam] = vp;
        }

        public Matrix4x4 GetPreviousVP(Camera cam)
        {
            if(dict.TryGetValue(cam, out var vp))
                return vp;
            else
                return GL.GetGPUProjectionMatrix(cam.projectionMatrix, true) * cam.worldToCameraMatrix;
        }
    }
}
