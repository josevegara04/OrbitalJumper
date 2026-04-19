Shader "Custom/PlanetSharp"
{
    Properties
    {
        _MainTex        ("Texture",          2D)            = "white" {}
        _Sharpness      ("Texture Sharpness", Range(0, 4))  = 1.5

        [Header(Lighting)]
        _SunDir         ("Sun Direction",    Vector)        = (1, 0.5, 0.5, 0)
        _Ambient        ("Ambient Light",    Range(0, 1))   = 0.08
        _SpecPower      ("Specular Power",   Range(1, 256)) = 32
        _SpecStrength   ("Specular Strength",Range(0, 1))   = 0.25

        [Header(Atmosphere)]
        _AtmoColor      ("Atmosphere Color", Color)         = (0.4, 0.6, 1.0, 1)
        _AtmoStrength   ("Atmosphere Rim",   Range(0, 2))   = 0.8
        _AtmoWidth      ("Rim Width",        Range(0.1, 5)) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            // Usamos Cull Back y ZWrite normales para objetos opacos
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // ----------------------------------------------------------------
            // Uniforms
            // ----------------------------------------------------------------
            sampler2D _MainTex;
            float4    _MainTex_ST;
            float4    _MainTex_TexelSize; // Unity lo llena automáticamente:
                                           // .xy = (1/w, 1/h), .zw = (w, h)

            float     _Sharpness;

            float4    _SunDir;
            float     _Ambient;
            float     _SpecPower;
            float     _SpecStrength;

            float4    _AtmoColor;
            float     _AtmoStrength;
            float     _AtmoWidth;

            // ----------------------------------------------------------------
            // Estructuras
            // ----------------------------------------------------------------
            struct appdata
            {
                float4 vertex  : POSITION;
                float3 normal  : NORMAL;    // <-- normal del vértice (nuevo)
                float2 uv      : TEXCOORD0;
            };

            struct v2f
            {
                float4 clipPos  : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float3 worldNormal : TEXCOORD1; // normal en espacio mundo
                float3 worldPos    : TEXCOORD2; // posición en espacio mundo
            };

            // ----------------------------------------------------------------
            // Vertex Shader
            // ----------------------------------------------------------------
            v2f vert(appdata v)
            {
                v2f o;

                // Transformar posición a clip space (igual que antes)
                o.clipPos = UnityObjectToClipPos(v.vertex);

                // UV con tiling/offset del inspector
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // Transformar la normal al espacio mundo.
                // UnityObjectToWorldNormal ya aplica la transpuesta inversa
                // del modelo (importante si el objeto tiene escala no uniforme).
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                // Posición en espacio mundo (para calcular viewDir en el frag)
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                return o;
            }

            // ----------------------------------------------------------------
            // Función de sharpness por mipmap manual
            //
            // tex2D muestrea el nivel de mipmap automáticamente según la
            // derivada de UV en el quad (ddx/ddy). Al desplazar ese nivel
            // hacia abajo (_Sharpness > 0) forzamos un mip más detallado,
            // lo que da sensación de nitidez. Valores negativos = blur.
            //
            // tex2Dbias(sampler, float4(uv, 0, bias)) es la función estándar
            // HLSL para esto. El 4to componente es el bias.
            // ----------------------------------------------------------------
            float4 SampleSharp(sampler2D tex, float2 uv, float sharpness)
            {
                return tex2Dbias(tex, float4(uv, 0, -sharpness));
            }

            // ----------------------------------------------------------------
            // Fragment Shader
            // ----------------------------------------------------------------
            fixed4 frag(v2f i) : SV_Target
            {
                // --- 1. Textura con sharpness controlado ---
                float4 col = SampleSharp(_MainTex, i.uv, _Sharpness);

                // --- 2. Normalizar vectores (pueden llegar interpolados) ---
                float3 N = normalize(i.worldNormal);

                // Dirección hacia el sol (normalizamos por si acaso)
                float3 L = normalize(_SunDir.xyz);

                // Dirección hacia la cámara (para specular y rim)
                float3 V = normalize(_WorldSpaceCameraPos - i.worldPos);

                // --- 3. Diffuse (Lambert) ---
                // dot(N, L) da 1 en el punto que apunta directo al sol,
                // 0 en el terminator, negativo en la cara oscura.
                // saturate() lo clampea a [0, 1].
                float NdotL = saturate(dot(N, L));

                // Lado nocturno: oscuro pero no negro puro (luz ambiental)
                float diffuse = max(NdotL, _Ambient);

                // --- 4. Specular (Blinn-Phong) ---
                // H = half-vector entre L y V. Más eficiente que reflect().
                // pow(dot(N,H), power) da el lóbulo especular.
                float3 H        = normalize(L + V);
                float  NdotH    = saturate(dot(N, H));
                float  specular = pow(NdotH, _SpecPower) * _SpecStrength;

                // Solo hay specular donde hay luz directa (no en la noche)
                specular *= step(0.0, dot(N, L));

                // --- 5. Rim / Atmospheric glow ---
                // El rim light simula el halo de la atmósfera en el borde.
                // 1 - dot(N, V) es máximo en los bordes (limb) y 0 en el centro.
                // pow(..., width) controla qué tan fino o ancho es el halo.
                float rimFactor = pow(1.0 - saturate(dot(N, V)), _AtmoWidth);

                // Solo mostrar rim en el lado iluminado (NdotL > 0)
                // Lerp suave para que no haya corte abrupto
                float rimMask = saturate(dot(N, L) + 0.4);
                float rim     = rimFactor * rimMask * _AtmoStrength;

                // --- 6. Composición final ---
                float3 litColor  = col.rgb * diffuse + specular;
                float3 finalColor = lerp(litColor, litColor + _AtmoColor.rgb, rim);

                return float4(finalColor, 1.0);
            }

            ENDHLSL
        }
    }

    Fallback "Diffuse"
}
