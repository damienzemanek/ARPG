Shader "Custom/FakeGodRayURP"
{
    Properties
    {
        [HDR] _RayColor ("Ray Color", Color) = (1,0.85,0.55,1)
        _Opacity ("Opacity", Range(0,1)) = 0.18
        _Softness ("Edge Softness", Range(0.5,8)) = 2.5
        _LengthFade ("Length Fade", Range(0,1)) = 0.6
        _NoiseScale ("Noise Scale", Range(0.1,30)) = 5
        _NoiseStrength ("Noise Strength", Range(0,1)) = 0.15
        _NoiseSpeed ("Noise Speed", Range(-5,5)) = 0.2
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Name "FakeGodRay"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
            half4 _RayColor;
            float _Opacity, _Softness, _LengthFade, _NoiseScale, _NoiseStrength, _NoiseSpeed;
            CBUFFER_END
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                float across = saturate(1.0 - abs(i.uv.x * 2.0 - 1.0));
                float edge = pow(across, max(_Softness, 0.5));
                float along = lerp(1.0, 1.0 - smoothstep(0.0, 1.0, i.uv.y), _LengthFade);
                float2 p = i.uv * _NoiseScale + float2(_Time.y * _NoiseSpeed, _Time.y * _NoiseSpeed * 0.37);
                float noise = frac(sin(dot(floor(p), float2(127.1, 311.7))) * 43758.5453);
                float variation = lerp(1.0, 0.65 + 0.35 * noise, _NoiseStrength);
                float alpha = saturate(edge * along * _Opacity * variation * _RayColor.a);
                return half4(_RayColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
