namespace PowerUtilities
{
#if UNITY_EDITOR

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

    public static class ShaderExclude
    {
        public static HashSet<string> shaderNames = new HashSet<string>
        {
            "Particles/Additive",
            "Legacy Shaders/Transparent/Diffuse",
            "Legacy Shaders/Transparent/Cutout/Diffuse",
            "Legacy Shaders/Transparent/TweenAlpha",
            "Hidden/FastBloom",
            "Hidden/FastBlur",
            "atsVegetation Unlit-Lightmap-Wind Alpha Tested",
            "Legacy Shaders/Transparent/Cutout/Diffuse",
            "Hidden/Grayscale Effect",
            "Mobile/Diffuse",
            "Hidden/Post FX/Uber Shader",
            "Hidden/Internal-Colored",
            "Unlit/Transparent Colored",
            "Spine/SkeletonGhost",
            "Unlit/Transparent Colored",
            "Hidden/Post FX/UI/Curve Background",
            "Hidden/Post FX/Monitors/Histogram Render",
            "Hidden/Post FX/Monitors/Parade Render",
            "Hidden/Post FX/Monitors/Vectorscope Render",
            "Hidden/Post FX/Monitors/Waveform Render",
            "Hidden/Post FX/UI/Trackball",
            "Particles/Alpha Blended",
            "Spine/Bones",
            "Spine/Skeleton",
            "Standard",
            "Standard (Specular setup)",
            "Mobile/Particles/Additive",
            "Mobile/Particles/Alpha Blended",
            "Mobile/Particles/VertexLit Blended",
            "Mobile/Particles/Multiply",
            "Particles/Additive",
            "Particles/Additive (Soft)",
            "Particles/Alpha Blended",
            "Particles/Blend",
            "Particles/Multiply",
            "Particles/Multiply (Double)",
            "Particles/Alpha Blended Premultiply",
            "Particles/VertexLit Blended",
            "Mobile/Particles/Additive Culled",
            "Legacy Shaders/Transparent/Diffuse",
            "Mobile/Transparent/Vertex Color",
            "ParticleCook/Transparent/AlphaBlend/Diffuse-Z",
            "Mobile/Transparent/Transparent Vertex Color",
            "Unlit/Transparent Cutout",
        };

        public static List<Shader> GetExcludeShaders()
        {
            var q = shaderNames.Select(shaderName => Shader.Find(shaderName))
                .Where(s => s);
            return q.ToList();
        }

        public static bool IsRefByCode(Shader shader)
        {
            return shaderNames.Contains(shader.name);
        }
    }
#endif
}