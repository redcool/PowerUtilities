#if UNITY_2020_3
using System;

[Flags]
public enum RTClearFlags
{
    //
    // 摘要:
    //     Do not clear any render target.
    None = 0,
    //
    // 摘要:
    //     Clear all color render targets.
    Color = 1,
    //
    // 摘要:
    //     Clear the depth buffer.
    Depth = 2,
    //
    // 摘要:
    //     Clear the stencil buffer.
    Stencil = 4,
    //
    // 摘要:
    //     Clear all color render targets, the depth buffer, and the stencil buffer. This
    //     is equivalent to combining RTClearFlags.Color, RTClearFlags.Depth and RTClearFlags.Stencil.
    All = 7,
    //
    // 摘要:
    //     Clear both the depth and the stencil buffer. This is equivalent to combining
    //     RTClearFlags.Depth and RTClearFlags.Stencil.
    DepthStencil = 6,
    //
    // 摘要:
    //     Clear both the color and the depth buffer. This is equivalent to combining RTClearFlags.Color
    //     and RTClearFlags.Depth.
    ColorDepth = 3,
    //
    // 摘要:
    //     Clear both the color and the stencil buffer. This is equivalent to combining
    //     RTClearFlags.Color and RTClearFlags.Stencil.
    ColorStencil = 5
}
#endif