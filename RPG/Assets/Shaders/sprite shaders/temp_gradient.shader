Shader "Unlit/temp_gradient"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _EffectTex("Effect", 2D) = "white" {}
        _GradientColor("Gradient Color", Color) = (0.5, 0.5, 0.5, 1)
        _ShadowColor("Shadow Color", Color) = (0, 0, 0, 1)
    }
        SubShader
        {
            Tags { "RenderType" = "Transparent"
            "Queue" = "Transparent"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            LOD 100

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                // make fog work
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float2 uvS : TEXCOORD1;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                sampler2D _EffectTex;
                float4 _MainTex_ST;
                float4 _EffectTex_ST;
                float4 _ShadowColor;
                float4 _GradientColor;

                #define SHAD_OFFSET 0.6
                #define WAVELENGTH 12.0

                float clampSine()
                {
                    return (_SinTime + 1) / 2.0;
                }

                float clampSineWithWave(float wL)
                {
                    return (sin(wL * _Time) + 1) / 2.0;
                }

                float funnySine(v2f i)
                {
                    return i.uv.y*((sin(WAVELENGTH * _Time) + 1) / 2.0);
                }

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.uvS = TRANSFORM_TEX(v.uv, _EffectTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // sample the texture
                    fixed4 col = lerp(_ShadowColor, _GradientColor, funnySine(i));
                    fixed2 sampleCoord;
                    fixed normCos = (_CosTime + 1.0) / 2.0;
                    sampleCoord.x = 0.5 * i.uvS.x;
                    sampleCoord.y = 0.5 * i.uvS.y - 0.2*_Time + 0.4*normCos;
                    fixed4 imageSample = tex2D(_EffectTex, sampleCoord);
                
                    col.a = tex2D(_MainTex, i.uv).a;
                    return col * imageSample;
                }
            ENDCG
        }
    }
}
