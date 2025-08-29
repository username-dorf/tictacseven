Shader "URP/Keycap2ToneWaveRounded_v3_5"
{
    Properties
    {
        _TopColorA("Top Color A", Color) = (1,0.9,0.2,1)
        _TopColorB("Top Color B", Color) = (1,0.75,0.1,1)
        _SideColor("Side Color", Color) = (0.97,0.58,0.12,1)

        _CornerRadius("Corner Radius (UV)", Range(0,0.5)) = 0.12

        _BorderColor("Border Color", Color) = (1,0,0,0)
        _Border2Color("Inner Line Color", Color) = (1,0.8,0.3,1)
        _Margin("Border Inset", Range(-0.25,0.45)) = 0.0
        _Feather("Border Fade", Range(0,0.2)) = 0.03
        _InnerOffset("Inner Line Offset", Range(0,0.45)) = 0.18
        _InnerWidth("Inner Line Width", Range(0,0.2)) = 0.03
        _InnerSoft("Inner Line Soft", Range(0,0.1)) = 0.01
        _BorderOpacity("Border Opacity", Range(0,1)) = 0.6
        _InnerOpacity("Inner Line Opacity", Range(0,1)) = 0.7

        _TopCutoff("Top Mask Cutoff", Range(0,1)) = 0.6
        _TopFeather("Top Mask Feather", Range(0,0.5)) = 0.15

        [Toggle]_EnableWave("Enable Wave", Float) = 1
        [Toggle]_UseWaveTex("Use Wave Texture", Float) = 0
        _WaveTex("Wave Mask (R)", 2D) = "black" {}
        _WaveThreshold("Wave Threshold", Range(0,1)) = 0.5
        _WaveSoftness("Wave Edge Softness", Range(0,0.2)) = 0.02

        _WaveAngle("Wave Angle", Range(-180,180)) = 0
        _WaveFreq("Wave Frequency", Range(0,20)) = 8
        _WaveAmp("Wave Amplitude", Range(0,0.5)) = 0.06
        _WavePhase("Wave Phase", Range(-6.283,6.283)) = 0
        _WaveOffset("Wave Offset", Range(0,1)) = 0.5
        _WaveSoft("Wave Softness", Range(0,0.2)) = 0.02

        _GlowColor("Glow Color", Color) = (1,1,1,1)
        _GlowIntensity("Glow Intensity", Range(0,5)) = 0
        _GlowWidth("Glow Width", Range(0,0.3)) = 0.08
        _GlowSoft("Glow Softness", Range(0,0.2)) = 0.05

        _DigitTex("Digit (RGBA)", 2D) = "white" {}
        _DigitTint("Digit Tint (RGBA)", Color) = (0,0,0,1)
        _DigitOpacity("Digit Opacity", Range(0,1)) = 0
        _DigitInset("Digit Inset", Range(0,0.45)) = 0.12
        _DigitCorner("Digit Corner Radius", Range(0,0.5)) = 0.12
        _DigitSoft("Digit Softness", Range(0,0.1)) = 0.01
        _DigitScale("Digit Scale", Range(0.0,2)) = 1
        _DigitOffsetX("Digit Offset X", Range(-0.5,0.5)) = 0
        _DigitOffsetY("Digit Offset Y", Range(-0.5,0.5)) = 0
        _DigitRotation("Digit Rotation (deg)", Range(-180,180)) = 0

        // Новые: реальные половины и центр верхней площадки в OBJECT SPACE
        _HalfSizeOS("Half Size XZ (object space)", Vector) = (0.5,0,0.5,0)
        _CenterOS ("Center   XZ (object space)", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "Unlit"
            Tags
            {
                "LightMode"="SRPDefaultUnlit"
            }

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS:POSITION;
                float3 normalOS:NORMAL;
                float2 uv:TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS:SV_POSITION;
                float3 normalWS:TEXCOORD0;
                float2 uv:TEXCOORD1;
                float3 posOS:TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _TopColorA, _TopColorB, _SideColor;
                float _CornerRadius;
                float4 _BorderColor, _Border2Color;
                float _Margin, _Feather, _InnerOffset, _InnerWidth, _InnerSoft, _BorderOpacity, _InnerOpacity;
                float _TopCutoff, _TopFeather;
                float _EnableWave, _UseWaveTex, _WaveThreshold, _WaveSoftness;
                float _WaveAngle, _WaveFreq, _WaveAmp, _WavePhase, _WaveOffset, _WaveSoft;
                float4 _GlowColor;
                float _GlowIntensity, _GlowWidth, _GlowSoft;
                float4 _DigitTint;
                float _DigitOpacity, _DigitInset, _DigitCorner, _DigitSoft, _DigitScale, _DigitOffsetX, _DigitOffsetY,
                      _DigitRotation;
                float4 _HalfSizeOS;
                float4 _CenterOS;
            CBUFFER_END

            TEXTURE2D(_WaveTex);
            SAMPLER(sampler_WaveTex);
            TEXTURE2D(_DigitTex);
            SAMPLER(sampler_DigitTex);

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = v.uv;
                o.posOS = v.positionOS;
                return o;
            }

            float2 rot2(float2 p, float a)
            {
                float s = sin(a), c = cos(a);
                return float2(c * p.x - s * p.y, s * p.x + c * p.y);
            }

            // Базовый SDF скруглённого прямоугольника
            float sdRoundRect(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b;
                return length(max(q, 0)) + min(max(q.x, q.y), 0) - r;
            }

            // UV-варианты (для цифры/иконки — оставляем как было)
            float sdRoundedInset(float2 p, float c, float ins)
            {
                float2 b = float2(0.5 - c, 0.5 - c) - ins;
                float r = c - ins;
                b = max(b, 0.0001);
                float rMax = max(0.0, min(b.x, b.y));
                r = clamp(r, 0.0, rMax);
                return sdRoundRect(p, b, r);
            }

            float maskRounded(float2 p, float c, float ins, float sft)
            {
                float d = sdRoundedInset(p, c, ins);
                float aa = sft + fwidth(d);
                return smoothstep(0, aa, -d);
            }

            // Мировые масштабы по локальным осям X/Z (учитывает неравномерный scale и поворот)
            float2 GetScaleXZ()
            {
                float3 c0 = float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20);
                float3 c2 = float3(unity_ObjectToWorld._m02, unity_ObjectToWorld._m12, unity_ObjectToWorld._m22);
                return float2(length(c0), length(c2));
            }

            // WS-варианты (корректная рамка для прямоугольников)
            float sdRoundedInsetWS(float2 pWS, float2 halfWS, float rWS, float insWS)
            {
                float2 b = halfWS - rWS - insWS;
                b = max(b, 1e-5);
                float r = max(rWS - insWS, 0.0);
                return sdRoundRect(pWS, b, r);
            }

            float maskRoundedWS(float2 pWS, float2 halfWS, float rWS, float insWS, float sftWS)
            {
                float d = sdRoundedInsetWS(pWS, halfWS, rWS, insWS);
                float aa = sftWS + fwidth(d);
                return smoothstep(0, aa, -d);
            }

            float waveMask_proc(float2 uv, float ang)
            {
                float2 q = rot2(uv - 0.5, ang);
                float curve = sin(q.x * _WaveFreq + _WavePhase) * _WaveAmp + _WaveOffset - 0.5;
                float d = q.y - curve;
                float aa = _WaveSoft + fwidth(d);
                return smoothstep(0 - aa, 0 + aa, d);
            }

            float waveMask_tex(float2 uv)
            {
                float2 dx = ddx(uv), dy = ddy(uv);
                float v = SAMPLE_TEXTURE2D_GRAD(_WaveTex, sampler_WaveTex, uv, dx, dy).r;
                float d = v - _WaveThreshold;
                float aa = _WaveSoftness + fwidth(v);
                return smoothstep(0 - aa, 0 + aa, d);
            }

            float4 frag(Varyings i) : SV_Target
            {
                // Верхняя плоскость
                float3 n = normalize(i.normalWS);
                float up = dot(n, float3(0, 1, 0));
                float topMask = smoothstep(_TopCutoff - _TopFeather, _TopCutoff + _TopFeather, up);

                // Реальные половины/центр верхней площадки в OS
                float2 halfOS = float2(_HalfSizeOS.x, _HalfSizeOS.z);
                float2 centerOS = float2(_CenterOS.x, _CenterOS.z);

                // Переход в мир с учётом неравномерного скейла и поворота
                float2 sxz = GetScaleXZ();
                float2 halfWS = halfOS * sxz;

                // Положение текущего пикселя относительно центра площадки
                float2 pOS = float2(i.posOS.x, i.posOS.z) - centerOS;
                float2 pWS = pOS * sxz;

                // Базовая шкала толщин — от меньшей половины
                float sMin = min(halfWS.x, halfWS.y);

                // Пересчёт параметров в world-единицы
                float rWS = _CornerRadius * sMin;
                float marginWS = _Margin * sMin;
                float featherWS = _Feather * sMin;
                float innerOffWS = _InnerOffset * sMin;
                float innerWWS = _InnerWidth * sMin;
                float innerSoftWS = _InnerSoft * sMin;
                float glowWWS = _GlowWidth * sMin;
                float glowSoftWS = _GlowSoft * sMin;

                // Градиент/волна по UV как было
                float m = (_EnableWave > 0.5)
                                                 ? ((_UseWaveTex > 0.5)
                                                                           ? waveMask_tex(i.uv)
                                                                           : waveMask_proc(i.uv, radians(_WaveAngle)))
                                                 : 1.0;
                float3 topTwo = lerp(_TopColorB.rgb, _TopColorA.rgb, m);

                // Наружная рамка (кольцо)
                float m0 = maskRoundedWS(pWS, halfWS, rWS, marginWS, featherWS);
                float m1 = maskRoundedWS(pWS, halfWS, rWS, marginWS + featherWS, featherWS);
                float borderRing = saturate(m0 - m1) * step(1e-5, abs(marginWS) + featherWS);

                // Внутренняя линия
                float in0 = maskRoundedWS(pWS, halfWS, rWS, innerOffWS, innerSoftWS);
                float in1 = maskRoundedWS(pWS, halfWS, rWS, innerOffWS + innerWWS, innerSoftWS);
                float innerRing = saturate(in0 - in1) * step(1e-5, innerWWS);

                // Цвет верхней пластины с линиями
                float3 topCol = topTwo;
                topCol = lerp(topCol, _BorderColor.rgb, borderRing * _BorderOpacity);
                topCol = lerp(topCol, _Border2Color.rgb, innerRing * _InnerOpacity);

                // Свечение по краю
                float dEdge = sdRoundRect(pWS, halfWS - rWS, rWS);
                float x = -dEdge;
                float aa = glowSoftWS + fwidth(dEdge);
                float glow = saturate(smoothstep(0, aa, x) * (1.0 - smoothstep(glowWWS, glowWWS + aa, x)));

                // Смешивание верх/бок
                float3 col = lerp(_SideColor.rgb, topCol, topMask);
                col += _GlowColor.rgb * glow * _GlowIntensity * topMask;

// ---------- ЦИФРА/ИКОНКА: COVER без растяжения и по форме верхней площадки ----------
// pWS, halfWS, rWS, sMin, topMask уже посчитаны выше.

// 1) Изотропные координаты: единица = меньшая половина (по короткой стороне)
//    Так картинка «покрывает» длинную сторону и не растягивается.
float2 iso = pWS / sMin;        // [-1..1] по короткой стороне на краю
float2 dC0 = iso * 0.5;         // [-0.5..0.5] квадратное пространство

// 2) Трансформации картинки (масштаб/поворот/смещение) в изотропных координатах:
float  s   = max(_DigitScale, 0.001);
float2 dR  = rot2(dC0 / s, radians(_DigitRotation));
float  a2  = (1.0 - 2.0 * _DigitInset);
float2 dUV = dR * a2 + 0.5 + float2(_DigitOffsetX, _DigitOffsetY);

// 3) Сэмпл. Лучше чтобы у текстуры Wrap Mode = Clamp.
float4 digit = SAMPLE_TEXTURE2D(_DigitTex, sampler_DigitTex, saturate(dUV));

// 4) МАСКА ПО ФОРМЕ ВЕРХНЕЙ ПЛОЩАДКИ (скруглённый прямоугольник в world-метрике)
float insetWS = _DigitInset * sMin;
float softWS  = _DigitSoft  * sMin;
float digitTopMask = maskRoundedWS(pWS, halfWS, rWS, insetWS, softWS);

// 5) Рисуем ТОЛЬКО на верхней плоскости и внутри формы верхушки
float dmask = digit.a * saturate(_DigitOpacity) * _DigitTint.a * digitTopMask * topMask;

// 6) Применяем тинт к картинке
col = lerp(col, _DigitTint.rgb, dmask);
// -------------------------------------------------------------------------------

                return float4(col, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode"="ShadowCaster"
            }
            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex sc_vert
            #pragma fragment sc_frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float3 positionOS:POSITION;
                float3 normalOS:NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS:SV_POSITION;
            };

            Varyings sc_vert(Attributes v)
            {
                Varyings o;
                float3 posWS = TransformObjectToWorld(v.positionOS);
                float3 nrmWS = TransformObjectToWorldNormal(v.normalOS);
                o.positionHCS = TransformWorldToHClip(ApplyShadowBias(posWS, nrmWS, 0));
                return o;
            }

            half4 sc_frag(Varyings i) : SV_Target { return 0; }
            ENDHLSL
        }
    }

    FallBack Off
}