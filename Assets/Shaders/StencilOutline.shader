Shader "Hidden/Framework/StencilOutline"
{
    Properties
    {
        _StencilOutlineColor ("Outline Color", Color) = (0,0,0,1)
        _StencilOutlineWidth ("Outline Width (Pixels)", Range(0,12)) = 3
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            half4 _StencilOutlineColor;
            float _StencilOutlineWidth;
        CBUFFER_END
        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            UNITY_VERTEX_OUTPUT_STEREO
        };
        Varyings MaskVertex(Attributes input)
        {
            Varyings output;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
            return output;
        }
        Varyings OutlineVertex(Attributes input)
        {
            Varyings output = MaskVertex(input);
            float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
            float3 normalVS = TransformWorldToViewDir(normalWS);
            float2 projectedNormal = mul((float3x3)UNITY_MATRIX_P, normalVS).xy;
            float2 pixelDirection = projectedNormal * _ScaledScreenParams.xy;
            pixelDirection /= max(length(pixelDirection), 0.0001);
            output.positionCS.xy += pixelDirection * (2.0 * _StencilOutlineWidth / _ScaledScreenParams.xy) * output.positionCS.w;
            return output;
        }
        half4 MaskFragment(Varyings input) : SV_Target { return 0; }
        half4 OutlineFragment(Varyings input) : SV_Target { return _StencilOutlineColor; }
        Varyings ClearVertex(uint vertexID : SV_VertexID)
        {
            Varyings output;
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            output.positionCS = GetFullScreenTriangleVertexPosition(vertexID);
            return output;
        }
        ENDHLSL
        Pass
        {
            Name "Mask"
            Cull Back
            ZWrite Off
            ZTest LEqual
            ColorMask 0
            Stencil { Ref 1 ReadMask 1 WriteMask 1 Comp Always Pass Replace }
            HLSLPROGRAM
            #pragma vertex MaskVertex
            #pragma fragment MaskFragment
            #pragma multi_compile_instancing
            ENDHLSL
        }
        Pass
        {
            Name "Outline"
            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            Stencil { Ref 1 ReadMask 1 WriteMask 0 Comp NotEqual Pass Keep }
            HLSLPROGRAM
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment
            #pragma multi_compile_instancing
            ENDHLSL
        }
        Pass
        {
            Name "ClearOutlineBit"
            Cull Off
            ZWrite Off
            ZTest Always
            ColorMask 0
            Stencil { Ref 0 ReadMask 1 WriteMask 1 Comp Always Pass Replace }
            HLSLPROGRAM
            #pragma vertex ClearVertex
            #pragma fragment MaskFragment
            ENDHLSL
        }
    }
    Fallback Off
}
