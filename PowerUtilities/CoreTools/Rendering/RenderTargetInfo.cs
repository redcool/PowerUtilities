namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;

    [Serializable]
    public class RenderTargetInfo
    {
        [Tooltip("rt name")]
        public string name;


        [Tooltip("rt format")]
        public GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm;

        //public bool isHdr;

        [Tooltip("setup depth buffer")]
        public bool hasDepthBuffer;

        public int GetHash() => Shader.PropertyToID(name);
        public bool IsValid() => !string.IsNullOrEmpty(name) && format != default;
    }
}
