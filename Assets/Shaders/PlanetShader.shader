Shader "Custom/PlanetShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            float4 _MainTex_ST;
            float3 _SunPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float3 worldPos : TEXCOORD4;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.worldPos = TransformObjectToWorld(v.positionOS);

                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.normalWS = TransformObjectToWorldNormal(v.normalOS);

                float3 tangentWS = normalize(TransformObjectToWorldDir(v.tangentOS.xyz));
                float3 normalWS = normalize(o.normalWS);
                float3 bitangentWS = normalize(cross(normalWS, tangentWS) * v.tangentOS.w);

                o.tangentWS = tangentWS;
                o.bitangentWS = bitangentWS;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv));

                float3x3 TBN = float3x3(
                    normalize(i.tangentWS),
                    normalize(i.bitangentWS),
                    normalize(i.normalWS)
                );

                float3 N = normalize(mul(normalTS, TBN));
                /* float3 N = normalize(i.normalWS); */

                // Obtener la luz principal (Directional Light)
                float3 lightDir = _SunPosition - i.worldPos;
                float distance = length(lightDir);
                float3 L = normalize(lightDir);

                // 🔥 atenuación (esto hace que parezca real)
                float attenuation = 1.0 / (1.0 + 0.0001 * distance * distance);

                // iluminación básica
                float NdotL = saturate(dot(N, L)) + 0.1;

                float3 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgb;

                float3 finalColor = color * NdotL * attenuation * 15.0;

                return float4(finalColor, 1.0);
            }

            ENDHLSL
        }
    }
}