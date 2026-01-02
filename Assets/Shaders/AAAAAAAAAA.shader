Shader "Hidden/AtmosphereRaymarchFullscreen_WithAlpha"
{
    Properties
    {
        _PlanetRadius("Planet Radius", Float) = 1.0
        _AtmosRadius("Atmosphere Radius", Float) = 1.1
        _Steps("Raymarch Steps", Int) = 64
        _BaseColor("Base Color (visual)", Color) = (0.2,0.5,1,1)
        _AlphaScale("Alpha Scale", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            // blending para usar alpha correctamente
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            int _Steps;
            float _PlanetRadius;
            float _AtmosRadius;
            float4 _BaseColor;
            float _AlphaScale;

            float4x4 _CameraInverseProjection;
            float4x4 _CameraToWorld;
            float3 _CameraPos;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(appdata_full v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            int RaySphereIntersect(float3 ro, float3 rd, float R, out float t0, out float t1)
            {
                float b = dot(ro, rd);
                float c = dot(ro, ro) - R * R;
                float disc = b*b - c;
                if (disc < 0.0) { t0 = t1 = 0; return 0; }
                float s = sqrt(disc);
                t0 = -b - s;
                t1 = -b + s;
                return 1;
            }

            // ahora la función devuelve color RGB y alpha separado
            void RaymarchAtmosphere(float3 ro, float3 rd, float Rplanet, float Ratmo, int steps, out float3 outColor, out float outAlpha)
            {
                float t0, t1;
                outColor = float3(0,0,0);
                outAlpha = 0.0;

                if (RaySphereIntersect(ro, rd, Ratmo, t0, t1) == 0)
                    return;

                if (t1 <= 0.0) return;
                if (t0 < 0.0) t0 = 0.0;

                float len = t1 - t0;
                float dt = len / steps;

                float accum = 0.0; // acumulador escalar para alpha
                float3 accumCol = float3(0,0,0);

                float t = t0;
                for (int i = 0; i < steps; i++)
                {
                    float3 pos = ro + rd * (t + 0.5 * dt);
                    float height = length(pos) - Rplanet;
                    // señal simple: 1 en la base de la atmósfera, 0 en la cima
                    float v = saturate(1.0 - height / (Ratmo - Rplanet));
                    accum += v * (1.0/steps);        // contribución al alpha (normalizada)
                    accumCol += v * (1.0/steps) * _BaseColor.rgb; // contribución al color
                    t += dt;
                }

                // escala alpha para que controles intensidad visual
                outAlpha = saturate(accum * _AlphaScale);
                outColor = accumCol; // ya incluye base color
            }

            float4 frag(v2f i) : SV_Target
            {
                // reconstrucción del rayo
                float2 uv = i.uv * 2.0 - 1.0;
                float4 clip = float4(uv.x, uv.y, 1.0, 1.0);
                float4 view = mul(_CameraInverseProjection, clip);
                view /= view.w;
                float3 viewDirWorld = normalize( mul(_CameraToWorld, float4(view.xyz, 0.0)).xyz );

                float3 ro = _CameraPos;
                float3 rd = viewDirWorld;

                float3 atmCol;
                float atmAlpha;
                RaymarchAtmosphere(ro, rd, _PlanetRadius, _AtmosRadius, _Steps, atmCol, atmAlpha);

                // fondo
                float3 bg = tex2D(_MainTex, i.uv).rgb;

                // compositing: lerp usando alpha y devolver alpha en el canal A
                float3 outc = lerp(bg, atmCol, atmAlpha);
                return float4(outc, atmAlpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
