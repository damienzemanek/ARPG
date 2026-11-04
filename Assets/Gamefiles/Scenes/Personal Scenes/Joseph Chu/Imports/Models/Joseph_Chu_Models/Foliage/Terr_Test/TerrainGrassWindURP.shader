Shader "Custom/Terrain Grass Wind URP"
{
    Properties
    {
        [MainTexture] _BaseMap ("Grass Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Grass Tint", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.4
        _WindStrength ("Wind Strength", Range(0,2)) = 0.18
        _WindSpeed ("Wind Speed", Range(0,10)) = 2
        _WindScale ("Wind Wave Scale", Range(0.01,5)) = 0.8
        _WindDirection ("Wind Direction XZ", Vector) = (1,0,0.3,0)
        _GrassHeight ("Grass Height (Local Units)", Float) = 1
        _RootHeight ("Root Height (Local Y)", Float) = 0
        _BendExponent ("Tip Bend Falloff", Range(0.5,5)) = 2
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        Cull Off
        ZWrite On
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Cutoff;
                float _WindStrength, _WindSpeed, _WindScale;
                float4 _WindDirection;
                float _GrassHeight, _RootHeight, _BendExponent;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                float3 rootWS = TransformObjectToWorld(float3(0, _RootHeight, 0));
                float3 world = TransformObjectToWorld(v.positionOS.xyz);
                float heightMask = saturate((v.positionOS.y - _RootHeight) / max(abs(_GrassHeight), 0.001));
                heightMask = pow(heightMask, _BendExponent);
                float2 dir = _WindDirection.xz;
                dir /= max(length(dir), 0.001);
                float phase = dot(rootWS.xz, float2(0.71, 1.13)) * _WindScale + _Time.y * _WindSpeed;
                float wave = sin(phase) * 0.65 + sin(phase * 1.73 + 1.2) * 0.35;
                world.xz += dir * (wave * _WindStrength * heightMask);
                o.positionWS = world;
                o.positionCS = TransformWorldToHClip(world);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }

            half4 frag(Varyings i, FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                clip(tex.a - _Cutoff);
                half3 normalWS = normalize(i.normalWS) * (IS_FRONT_VFACE(face, 1, -1));
                Light mainLight = GetMainLight();
                half ndl = saturate(dot(normalWS, mainLight.direction));
                half3 ambient = SampleSH(normalWS);
                half3 lighting = ambient + mainLight.color * (ndl * mainLight.distanceAttenuation);
                return half4(tex.rgb * max(lighting, half3(0.12,0.12,0.12)), 1);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
