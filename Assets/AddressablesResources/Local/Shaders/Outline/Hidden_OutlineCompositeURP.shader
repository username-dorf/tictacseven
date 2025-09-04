Shader "Hidden/Outline/CompositeURP"
{
    Properties
    {
        _Color ("Outline Color", Color) = (0,0.42,1,1)
        _Opacity ("Opacity", Range(0,1)) = 1
        _DepthEps ("Depth Epsilon", Range(0.0001, 0.01)) = 0.002
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "Composite"
            ZTest Always
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_SourceTex);
            SAMPLER(sampler_SourceTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            TEXTURE2D_X_FLOAT(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            float2 _MaskTex_TexelSize;
            float4 _Color;
            float _Opacity;
            float _ThicknessPx;
            float _Softness;
            float _DepthEps;

            struct Varyings
            {
                float4 positionCS:SV_POSITION;
                float2 uv:TEXCOORD0;
            };

            Varyings vert(uint id:SV_VertexID)
            {
                Varyings o;
                o.positionCS = GetFullScreenTriangleVertexPosition(id);
                o.uv = GetFullScreenTriangleTexCoord(id);
                return o;
            }

            float LinearEyeDepthAt(float2 uv)
            {
                float raw = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
                return LinearEyeDepth(raw, _ZBufferParams);
            }

            float M(float2 uv) { return SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv).r; }

            half4 frag(Varyings i):SV_Target
            {
                float2 px = _MaskTex_TexelSize * max(_ThicknessPx, 1.0);

                // Маска (унион)
                float m0 = M(i.uv);

                // 8 соседей
                float2 o[8] = {
                    float2(px.x, 0), float2(-px.x, 0), float2(0, px.y), float2(0, -px.y),
                    float2(px.x, px.y), float2(-px.x, px.y), float2(px.x, -px.y), float2(-px.x, -px.y)
                };

                // --- Внешний контур, как раньше ---
                float m = 0;
                [unroll] for (int k = 0; k < 8; k++) m += M(i.uv + o[k]);
                float outlineOuter = saturate(m * (1.0 / 8.0));
                outlineOuter = max(outlineOuter - m0, 0.0);

                // --- Контур между двумя объектами по скачку глубины ---
                float d0 = LinearEyeDepthAt(i.uv);
                float depthEdgeFar = 0;

                // Рисуем контур на ТЕКУЩЕМ пикселе, если он ДАЛЬШЕ соседа (т.е. сосед ближе к камере)
                // и оба пикселя принадлежат маске (оба — "контурируемые" объекты).
                [unroll] for (int k = 0; k < 8; k++)
                {
                    float mN = M(i.uv + o[k]);
                    if (m0 > 0.5 && mN > 0.5)
                    {
                        float dN = LinearEyeDepthAt(i.uv + o[k]);
                        depthEdgeFar = max(depthEdgeFar, step(_DepthEps, d0 - dN)); // d0 > dN + eps → рисуем
                    }
                }

                // Сглаживание
                float aa = fwidth(outlineOuter + depthEdgeFar) * (_Softness + 1.0);
                float edge = smoothstep(0.0, aa, outlineOuter + depthEdgeFar);

                float4 src = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, i.uv);
                float a = edge * saturate(_Color.a * _Opacity);
                float3 rgb = lerp(src.rgb, _Color.rgb, a);
                return float4(rgb, src.a);
            }
            ENDHLSL
        }
    }
    FallBack Off
}