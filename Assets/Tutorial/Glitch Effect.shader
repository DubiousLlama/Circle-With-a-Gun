Shader "UI/GlitchPopup"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlitchStrength ("Glitch Strength", Range(0,1)) = 1
        _StripSize ("Strip Size", Range(0.01,0.2)) = 0.05
        _ShiftAmount ("Shift Amount", Range(0,0.2)) = 0.1
        _JitterSpeed ("Jitter Speed", Range(0,50)) = 20
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _GlitchStrength;
            float _StripSize;
            float _ShiftAmount;
            float _JitterSpeed;
            float _TimeParameters; // Unity built-in (_Time)

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float rand(float x)
            {
                return frac(sin(x));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Each horizontal strip
                float stripIndex = floor(i.uv.y / _StripSize);
                float rnd = rand(stripIndex + 2);

                // Time-based jitter (oscillation)
                float jitter = sin((_Time.y * _JitterSpeed) + rnd * 10.0);

                // Horizontal displacement decays as strength approaches 0
                float shift = jitter * (rnd - 0.5) * 2.0 * _ShiftAmount * _GlitchStrength;

                float2 uv = i.uv;
                uv.x += shift;

                fixed4 col = tex2D(_MainTex, uv);

                float visibility = 1 - _GlitchStrength;

                if (rnd > visibility) {
                    col.a = 0;
                }

                return col;
            }
            ENDCG
        }
    }
}
