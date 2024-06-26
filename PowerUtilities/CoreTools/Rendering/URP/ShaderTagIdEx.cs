﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public static class ShaderTagIdEx
    {
        public static List<ShaderTagId> legacyShaderPassNames = new List<ShaderTagId>
        {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM"),
        };

        public static List<ShaderTagId> urpForwardShaderPassNames = new List<ShaderTagId>{
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("LightweightForward"),
            new ShaderTagId("SRPDefaultUnlit"),
        };
        public static List<ShaderTagId> shadowCaster = new List<ShaderTagId>
        {
            new ShaderTagId("ShadowCaster")
        };
        public static List<ShaderTagId> depthOnly = new List<ShaderTagId>
        {
            new ShaderTagId("DepthOnly")
        };
        public static ShaderTagId motionVectors = new ShaderTagId("MotionVectors");
    }
}
