Shader "Hidden/Utils/MRTComposite"
{
    Properties
    {
        // _SourceTex ("Texture", 2D) = "white" {}
    }

    HLSLINCLUDE
    #include "../../../../../PowerShaderLib/Lib/UnityLib.hlsl"
    #include "../../../../../PowerShaderLib/URPLib/Lighting.hlsl"
    #include "../../../../../PowerShaderLib/Lib/ScreenTextures.hlsl"
    #include "../../../../../PowerShaderLib/Lib/BlitLib.hlsl"
    #include "../../../../../PowerShaderLib/Lib/Colors.hlsl"

    struct v2f
    {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
    };
    struct appdata{
        float3 vertex:POSITION;
        float2 uv : TEXCOORD;
    };

    sampler2D _ColorBuffer0;
    sampler2D _ColorBuffer1;
    

    v2f vert (appdata i)
    {
        v2f o;
        // FullScreenTriangleVert(vid,o.vertex/**/,o.uv/**/);
        o.vertex = float4(i.vertex.xy*2,0,1);
        o.uv = i.uv;
        // o.uv.x = 1- o.uv.x; // cube's uv
        return o;
    }

    float4 frag (v2f i) : SV_Target
    {
        float2 suv = i.vertex.xy/_ScaledScreenParams.xy;
        float4 gbuffer0 = tex2D(_ColorBuffer0,suv);// color
        float4 gbuffer1 = tex2D(_ColorBuffer1,suv); // normal

        float3 albedo = gbuffer0.xyz;

        float3 normal = gbuffer1.xyz;
        normal.z = sqrt(1-gbuffer1.x*gbuffer1.x-gbuffer1.y*gbuffer1.y);

        float depth = GetScreenDepth(suv);    
        float3 worldPos = ComputeWorldSpacePosition(i.uv,depth,UNITY_MATRIX_I_VP);
        Light mainLight = GetMainLight();

        float3 v = normalize(_WorldSpaceCameraPos.xyz - worldPos);
        float3 l = mainLight.direction;
        float3 h = normalize(l+v);
        float nv = saturate(dot(normal,v));
        float nl = saturate(dot(normal,l));
        float nh = saturate(dot(normal,h));
        float lh = saturate(dot(l,h));

        float4 col = 0;

        float m = 0.5;
        float3 diffCol = albedo * (1-m);
        float3 specCol = lerp(0.04,albedo,m);
        
        float r = 0.5;
        float r2 = r*r;
        float d = nh*nh*(r2-1)+1;
        float specTerm = r2/(d*d* max(0.001,lh*lh) * (4*r+2));
        float3 diffuse = diffCol;

        float3 radiance = nl * mainLight.color;
        col.xyz = (diffuse + specTerm * specCol) * radiance;

        return col;
    }
    ENDHLSL

    SubShader
    {
        Pass
        {
            ztest always
            zwrite off
            Tags{"LightMode"="GBufferComposite"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}
