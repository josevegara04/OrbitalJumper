Shader "Custom/SatelliteRealistic"
{
    Properties
    {
        _BaseMap ("Albedo", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Metallic ("Metallic", Range(0,1)) = 0.8
        _Smoothness ("Smoothness", Range(0,1)) = 0.6
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            float _Metallic;
            float _Smoothness;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;

                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.normalWS = normalize(normalWS);

                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDirWS = normalize(GetCameraPositionWS() - posWS);

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 normalWS = normalize(IN.normalWS);

                // Normal map
                float3 normalTex = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv).xyz * 2 - 1;
                normalWS = normalize(normalWS + normalTex * 0.5);

                // Light
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);

                float NdotL = saturate(dot(normalWS, lightDir));

                // Albedo
                float3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb;

                // Diffuse
                float3 diffuse = albedo * NdotL;

                // Specular (Blinn-Phong mejorado)
                float3 halfDir = normalize(lightDir + IN.viewDirWS);
                float spec = pow(saturate(dot(normalWS, halfDir)), _Smoothness * 128);

                float3 specular = _Metallic * spec * mainLight.color;

                // Final color
                float3 color = diffuse + specular;

                return float4(color, 1.0);
            }

            ENDHLSL
        }
    }
}