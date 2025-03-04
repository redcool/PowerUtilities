 Shader "TestIndirect"
{
    Properties{
        [GroupToggle(,INDEX_BUFFER)] _IndexBufferOn("_IndexBufferOn",float) = 0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature  INDEX_BUFFER

            #include "UnityCG.cginc"
            #if defined(INDEX_BUFFER)
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #else
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawArgs
            #endif

            #include "UnityIndirect.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
            };

            struct InstanceInfo{
                float3 posOffsets[20];
            };

            StructuredBuffer<int> _Triangles;
            StructuredBuffer<float3> _Positions;
            StructuredBuffer<float3> _PosOffsets;
            
            uniform uint _BaseVertexIndex;
            uniform float4x4 _ObjectToWorld;
            float _IndexBufferOn;
            float _InstanceCountList[2];

            v2f vert(uint svVertexID: SV_VertexID, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                v2f o;
                uint cmdID = GetCommandID(0);
                uint instanceID = GetIndirectInstanceID(svInstanceID);
                uint triangleId = GetIndirectVertexID(svVertexID);

                if(!_IndexBufferOn)
                {
                    triangleId = _Triangles[triangleId] + _BaseVertexIndex; // no IndexBuffer
                }
                float3 pos = _Positions[triangleId]; // has IndexBuffer
                uint count =  _InstanceCountList[cmdID];
                float3 posOffset = _PosOffsets[cmdID * count + instanceID];

                pos += posOffset;
                float4 wpos = mul(_ObjectToWorld,float4(pos,1));
                // float4 wpos = mul(_ObjectToWorld, float4(pos + float3(instanceID, cmdID, 0.0f), 1.0f));
                o.pos = mul(UNITY_MATRIX_VP, wpos);
                o.color = float4(cmdID & 1 ? 0.0f : 1.0f, cmdID & 1 ? 1.0f : 0.0f, instanceID / float(GetIndirectInstanceCount()), 0.0f);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}