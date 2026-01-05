using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public static class BatchRendererGroupEx
    {
        /// <summary>
        /// GLES3 need check windowSize, otherwise windowSize is 0
        /// </summary>
        /// <param name="brg"></param>
        /// <param name="metadatas"></param>
        /// <param name="graphBuffer"></param>
        /// <returns></returns>
        public static BatchID AddBatch(this BatchRendererGroup brg, NativeArray<MetadataValue> metadatas, GraphicsBuffer graphBuffer)
        {
            var bufferOffset = 0u;
            var windowSize = 0u;
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
            {
                windowSize = (uint)BatchRendererGroup.GetConstantBufferMaxWindowSize();
                Debug.Log("gles3 windowSize bytes: "+ windowSize);
            }

            return brg.AddBatch(metadatas, graphBuffer.bufferHandle, bufferOffset, windowSize);
        }
    }
}
