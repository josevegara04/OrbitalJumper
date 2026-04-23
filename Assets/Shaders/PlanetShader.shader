Shader "Custom/PlanetSimpleLit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            float4 _MainTex_ST;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;

                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.normalWS = TransformObjectToWorldNormal(v.normalOS);

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 N = normalize(i.normalWS);

                // Obtener la luz principal (Directional Light)
                Light light = GetMainLight();

                float3 L = normalize(light.direction);

                // iluminación básica
                float NdotL = saturate(dot(N, L)) + 0.05;

                float3 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgb;

                float3 finalColor = color * NdotL * light.color;

                return float4(finalColor, 1.0);
            }

            ENDHLSL
        }
    }
}