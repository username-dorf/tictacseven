Shader "Hidden/Outline/MaskURP"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" "IgnoreProjector"="True"
        }
        Pass
        {
            Name "Mask"
            ZTest LEqual
            ZWrite Off
            Cull Back
            ColorMask R

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS:POSITION;
            };

            struct Varyings
            {
                float4 positionCS:SV_POSITION;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            half4 frag(Varyings i):SV_Target { return half4(1, 0, 0, 1); } // R=1 — маска
            ENDHLSL
        }
    }
    FallBack Off
}