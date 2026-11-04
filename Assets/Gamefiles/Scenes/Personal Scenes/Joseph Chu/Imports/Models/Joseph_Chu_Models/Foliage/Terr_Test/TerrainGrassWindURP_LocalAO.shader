Shader "Custom/Terrain Grass Wind URP Local AO"
{
 Properties
 {
  [MainColor] _BaseColor("Base Color",Color)=(1,1,1,1)
  _WindStrength("Wind Strength",Range(0,2))=0.18
  _WindSpeed("Wind Speed",Range(0,10))=2
  _WindScale("Wind Wave Scale",Range(0.01,5))=0.8
  _WindDirection("Wind Direction XZ",Vector)=(1,0,0.3,0)
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
  CBUFFER_END
  struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
  float3 WindWorld(float3 posOS)
  {
   float3 world=TransformObjectToWorld(posOS);
   float3 rootWS=TransformObjectToWorld(float3(0,_RootHeight,0));
   float mask=pow(saturate((posOS.y-_RootHeight)/max(abs(_GrassHeight),0.001)),_BendExponent);
   float2 dir=_WindDirection.xz/max(length(_WindDirection.xz),0.001);
   float phase=dot(rootWS.xz,float2(0.71,1.13))*_WindScale+_Time.y*_WindSpeed;
   float wave=sin(phase)*0.65+sin(phase*1.73+1.2)*0.35;
   world.xz+=dir*(wave*_WindStrength*mask);
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
