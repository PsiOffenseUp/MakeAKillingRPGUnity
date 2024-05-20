Shader "Unlit/sprite_shadow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GradientColor("Gradient Color", Color) = (0.5, 0.5, 0.5, 1)
        _ShadowColor("Shadow Color", Color) = (0, 0, 0, 1)
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _MaskColor("MaskColor", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        Tags { 
        "RenderType"="Transparent" 
        "Queue" = "Transparent"
        "LightMode" = "ForwardBase"
        "ForceNoShadowCasting" = "False"
        }
        //Cull Back
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                LIGHTING_COORDS(0, 1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ShadowColor;
            float4 _GradientColor;
            float4 _OutlineColor;
            float4 _MaskColor;

            #define COLOR_EQUAL_THRESHOLD 0.08f

            bool colorsEqual(float4 color1, float4 color2)
            {
                return abs(color1.r - color2.r) < COLOR_EQUAL_THRESHOLD &&
                    abs(color1.g - color2.g) < COLOR_EQUAL_THRESHOLD &&
                    abs(color1.b - color2.b) < COLOR_EQUAL_THRESHOLD &&
                    abs(color1.a - color2.a) < COLOR_EQUAL_THRESHOLD;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //TRANSFER_VERTEX_TO_FRAGMENT(o);

                return o;
            }

            #define SHADING_EFFECT_OFFSET 0.6

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed alpha = col.a; //Preserve original alpha
                float atten = LIGHT_ATTENUATION(i);

               // if (colorsEqual(col, _MaskColor)) //Replace the masked outline with the desired color
                 //   col = _OutlineColor;

                col = lerp(_ShadowColor, col, saturate(i.uv.y + SHADING_EFFECT_OFFSET));
                col.a = alpha;

                //col = fixed4(atten, 0, 0, 1);

                return col;
            }
            ENDCG
        }
    } Fallback "VertexLit"
}
