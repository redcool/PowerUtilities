using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    public static class ShaderPropertyIds
    {
        public static readonly int
            _MainTex = Shader.PropertyToID("_MainTex"),
            _CameraOpaqueTexture = Shader.PropertyToID(nameof(_CameraOpaqueTexture)),
            _CameraDepthTexture = Shader.PropertyToID(nameof(_CameraDepthTexture)),
            _CameraColorAttachmentA = Shader.PropertyToID(nameof(_CameraColorAttachmentA)),
            _CameraColorAttachmentB = Shader.PropertyToID(nameof(_CameraColorAttachmentB)),
            _CameraDepthAttachment = Shader.PropertyToID(nameof(_CameraDepthAttachment)),
            _GBuffer0 = Shader.PropertyToID(nameof(_GBuffer0)),
            _GBuffer1 = Shader.PropertyToID(nameof(_GBuffer1)),
            _GBuffer2 = Shader.PropertyToID(nameof(_GBuffer2)),
            _GBuffer3 = Shader.PropertyToID(nameof(_GBuffer3))
            ;

        public const string _DEBUG = nameof(_DEBUG),
            _CUSTOM_DEPTH_TEXTURE = nameof(_CUSTOM_DEPTH_TEXTURE)
            ;
    }
}