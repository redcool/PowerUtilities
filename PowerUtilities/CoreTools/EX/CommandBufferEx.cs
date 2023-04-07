using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{

    public static class CommandBufferEx
    {
        public static void ExecuteCommand(this CommandBuffer cmd,ScriptableRenderContext context)
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

    }
}