Shader "Custom/Terrain Grass Wind URP Directional Gusts"
{
 Properties
 {
  [MainColor] _BaseColor("Base Color",Color)=(1,1,1,1)
  _WindStrength("Wind Strength",Range(0,2))=0.18
  _WindSpeed("Wind Speed",Range(0,10))=2
  _WindScale("Wind Wave Scale",Range(0.01,5))=0.8
  _WindDirection("Wind Direction XZ",Vector)=(1,0,0.3,0)
  _BreezeStrength("Breeze Strength",Range(0,1))=0.12
  _BreezeSpeed("Breeze Speed",Range(0,10))=1.8
  _GustStrength("Directional Gust Strength",Range(0,2))=0.5
  _GustSpeed("Gust Travel Speed",Range(0,15))=3
  _GustSpacing("Gust Wave Spacing",Range(1,40))=12
  _GustWidth("Gust Wave Width",Range(0.05,1))=0.3
  _GustVariation("Gust Variation",Range(0,1))=0.3
  _GrassHeight("Grass Height Local Units",Float)=1
  _RootHeight("Root Height Local Y",Float)=0
  _BendExponent("Tip Bend Falloff",Range(0.5,5))=2
  _ShadowStrength("Receive Shadow Strength",Range(0,1))=1
  _LocalAOStrength("Local AO Strength",Range(0,1))=0.55
  _LocalAOHeight("Local AO Height (Local Units)",Float)=0.7
  _LocalAOPower("Local AO Falloff",Range(0.2,5))=1.5
 }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
  Cull Off
  ZWrite On
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
  CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;
   half _ShadowStrength;
   half _LocalAOStrength;
   float _LocalAOHeight, _LocalAOPower;
   float _WindStrength, _WindSpeed, _WindScale;
   float4 _WindDirection;
   float _GrassHeight, _RootHeight, _BendExponent;
   float _BreezeStrength, _BreezeSpeed, _GustStrength, _GustSpeed, _GustSpacing, _GustWidth, _GustVariation;
  CBUFFER_END
  struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
  float3 WindWorld(float3 posOS)
  {
   float3 world = TransformObjectToWorld(posOS);
   float3 rootWS = TransformObjectToWorld(float3(0, _RootHeight, 0));
   float mask = pow(saturate((posOS.y - _RootHeight) / max(abs(_GrassHeight), 0.001)), _BendExponent);
   float2 dir = _WindDirection.xz;
   dir = (dot(dir, dir) > 0.0001) ? normalize(dir) : float2(1, 0);
   float2 across = float2(-dir.y, dir.x);

   // Continuous small, irregular breeze, spatially offset between clumps.
   float breezePhase = dot(rootWS.xz, float2(0.71, 1.13)) * _WindScale + _Time.y * _BreezeSpeed;
   float breeze = sin(breezePhase) * 0.65 + sin(breezePhase * 1.73 + 1.2) * 0.35;
   float2 breezeMove = dir * (breeze * _BreezeStrength) + across * (sin(breezePhase * 0.63) * _BreezeStrength * 0.2);

   // Traveling bands: phase advances along the chosen wind direction.
   // A narrow, smooth crest creates a gust that sweeps across the terrain.
   float distanceAlong = dot(rootWS.xz, dir);
   float cycle = (distanceAlong - _Time.y * _GustSpeed) / max(_GustSpacing, 0.01);
   float phase = frac(cycle);
   float distanceToCrest = min(phase, 1.0 - phase);
   float width = max(_GustWidth * 0.5, 0.01);
   float gust = 1.0 - smoothstep(0.0, width, distanceToCrest);
   gust = gust * gust * (3.0 - 2.0 * gust);
   float variation = lerp(1.0, 0.7 + 0.3 * sin(distanceAlong * 0.23 + _Time.y * 0.37), _GustVariation);
   float2 gustMove = dir * (gust * variation * _GustStrength);

   // Preserve the original wind control as an additional gentle sway layer.
   float legacyPhase = dot(rootWS.xz, float2(0.47, 0.89)) * _WindScale + _Time.y * _WindSpeed;
   float2 legacyMove = dir * (sin(legacyPhase) * _WindStrength * 0.35);
   world.xz += (breezeMove + gustMove + legacyMove) * mask;
   return world;
  }
  ENDHLSL
  Pass
  {
   Name "ForwardLit"
   Tags { "LightMode"="UniversalForward" }
   HLSLPROGRAM
   #pragma target 3.0
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   struct Varyings { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD1; float3 positionWS:TEXCOORD2; float4 shadowCoord:TEXCOORD3; float localHeight:TEXCOORD4; UNITY_VERTEX_INPUT_INSTANCE_ID };
   Varyings vert(Attributes v)
   {
    Varyings o; UNITY_SETUP_INSTANCE_ID(v); UNITY_TRANSFER_INSTANCE_ID(v,o);
    float3 ws=WindWorld(v.positionOS.xyz);
    o.positionWS=ws;
    o.localHeight=v.positionOS.y;
    o.positionCS=TransformWorldToHClip(ws);
    o.shadowCoord=TransformWorldToShadowCoord(ws);
    o.normalWS=TransformObjectToWorldNormal(v.normalOS);
    return o;
   }
   half4 frag(Varyings i, FRONT_FACE_TYPE face:FRONT_FACE_SEMANTIC):SV_Target
   {
    UNITY_SETUP_INSTANCE_ID(i);
    half3 n=normalize(i.normalWS)*IS_FRONT_VFACE(face,1,-1);
    Light mainLight=GetMainLight(i.shadowCoord);
    half ndl=saturate(dot(n,mainLight.direction));
    half shadow=lerp(1,mainLight.shadowAttenuation,_ShadowStrength);
    half3 lighting=SampleSH(n)+mainLight.color*(ndl*mainLight.distanceAttenuation*shadow);
    half height01=saturate((i.localHeight-_RootHeight)/max(abs(_LocalAOHeight),0.001));
    half rootMask=pow(1.0h-height01,_LocalAOPower);
    half localAO=1.0h-saturate(_LocalAOStrength)*rootMask;
    return half4(_BaseColor.rgb*max(lighting,half3(0.02,0.02,0.02))*localAO,1);
   }
   ENDHLSL
  }
  Pass
  {
   Name "ShadowCaster"
   Tags { "LightMode"="ShadowCaster" }
   ColorMask 0
   HLSLPROGRAM
   #pragma target 3.0
   #pragma vertex ShadowVert
   #pragma fragment ShadowFrag
   #pragma multi_compile_instancing
   #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
   float3 _LightDirection;
   float3 _LightPosition;
   struct ShadowVaryings { float4 positionCS:SV_POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
   ShadowVaryings ShadowVert(Attributes v)
   {
    ShadowVaryings o; UNITY_SETUP_INSTANCE_ID(v); UNITY_TRANSFER_INSTANCE_ID(v,o);
    float3 ws=WindWorld(v.positionOS.xyz);
    float3 n=TransformObjectToWorldNormal(v.normalOS);
    #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
     float3 lightDir=normalize(_LightPosition-ws);
    #else
     float3 lightDir=_LightDirection;
    #endif
    float4 clipPos=TransformWorldToHClip(ApplyShadowBias(ws,n,lightDir));
    #if UNITY_REVERSED_Z
     clipPos.z=min(clipPos.z,clipPos.w*UNITY_NEAR_CLIP_VALUE);
    #else
     clipPos.z=max(clipPos.z,clipPos.w*UNITY_NEAR_CLIP_VALUE);
    #endif
    o.positionCS=clipPos;
    return o;
   }
   half4 ShadowFrag(ShadowVaryings i):SV_Target
   {
    UNITY_SETUP_INSTANCE_ID(i);
    return 0;
   }
   ENDHLSL
  }
 }
 FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
