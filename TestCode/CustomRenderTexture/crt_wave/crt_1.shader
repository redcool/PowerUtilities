Shader "CustomRenderTexture/crt1"
{
    Properties
    {
        _DisappearSpeed("_DisappearSpeed",range(-1,1)) = 0.01
     }

     SubShader
     {

        Pass
        {
            Name "Wave"
            
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            // #pragma vertex InitCustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            float4      _Color;
            sampler2D   _MainTex;
            float _DisappearSpeed;

            float4 frag(v2f_customrendertexture IN) : SV_Target
            {
                float2 uv = IN.globalTexcoord.xy;

                float2 texelSize = 1/_CustomRenderTextureInfo.xy;
                float p0 = tex2D(_SelfTexture2D,uv + float2(texelSize.x,0));
                float p1 = tex2D(_SelfTexture2D,uv - float2(texelSize.x,0));
                float p2 = tex2D(_SelfTexture2D,uv + float2(0,texelSize.y));
                float p3 = tex2D(_SelfTexture2D,uv - float2(0,texelSize.y));
                float4 tex = tex2D(_SelfTexture2D,uv);
                
                float newHeight = (p0+p1+p2+p3)*0.5 - tex.y;
                // newHeight *= 0.98;
                newHeight -= _DisappearSpeed * unity_DeltaTime.x;
                return float4(saturate(newHeight),tex.x,0,0);
            }
            ENDCG
        }
        Pass
        {
            Name "LeftClick"
            blend one one
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag

            float4 frag(v2f_customrendertexture i) : SV_Target
            {
                // 返回一个高亮值，产生波纹中心点
                return float4(1, 0,0,0);
            }
            ENDCG
        }
    }
}
