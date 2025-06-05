using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class ComputeShaderEx
    {
        /// <summary>
        /// Send var : _DispatchGroupSize
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="kernelId"></param>
        /// <param name="resultTexWidth"></param>
        /// <param name="resultTexHeight"></param>
        /// <param name="resultTexDepth"></param>
        public static void DispatchKernel(this ComputeShader cs,int kernelId,int resultTexWidth,int resultTexHeight,int resultTexDepth)
        {
            cs.GetKernelThreadGroupSizes(kernelId, out var xSize, out var ySize, out var zSize);

            var xGroups = Mathf.CeilToInt(resultTexWidth / (float)xSize);
            var yGroups = Mathf.CeilToInt(resultTexHeight / (float)ySize);
            var zGroups = Mathf.CeilToInt(resultTexDepth / (float)zSize);

            cs.SetFloats("_DispatchGroupSize", xGroups, yGroups, zGroups,0);
            cs.SetFloats("_NumThreads", xSize, ySize, zSize, xSize * ySize * zSize);
            cs.Dispatch(kernelId, xGroups, yGroups, zGroups);
        }

        public static bool CanExecute(this ComputeShader cs)
        => SystemInfo.supportsComputeShaders && cs;
    }
}
