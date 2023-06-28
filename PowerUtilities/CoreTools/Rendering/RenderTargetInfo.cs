namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;

    [Serializable]
    public class RenderTargetInfo
    {
        public string name;
        public GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm;
        public bool isHdr;
        public bool hasDepthBuffer;

        public int GetHash() => Shader.PropertyToID(name);
        public bool IsValid() => !string.IsNullOrEmpty(name) && format != default;
    }
}
