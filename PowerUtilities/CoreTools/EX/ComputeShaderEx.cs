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
        public static void DispatchKernel(this ComputeShader cs,int kernelId,int resultTexWidth,int resultTexHeight,int resultTexDepth)
        {
            cs.GetKernelThreadGroupSizes(kernelId, out var xSize, out var ySize, out var zSize);

            var xGroups = Mathf.CeilToInt(resultTexWidth / (float)xSize);
            var yGroups = Mathf.CeilToInt(resultTexHeight / (float)ySize);
            var zGroups = Mathf.CeilToInt(resultTexDepth / (float)zSize);
            cs.Dispatch(kernelId, xGroups, yGroups, zGroups);
        }
    }
}
