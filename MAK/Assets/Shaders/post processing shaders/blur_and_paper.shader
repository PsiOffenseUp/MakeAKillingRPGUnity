Shader "Unlit/blur_and_paper"
{
    Properties
    {
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define SAMPLE_DIST 0.00085f
            #define OVERLAY_OPACITY 0.185f

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _OverlayTex;
            float4 _MainTex_ST;
            float4 _OverlayTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                //Sample the texture and 8 adjacent values for simulated Gaussian blur
                fixed4 col = 0.25f * tex2D(_MainTex, i.uv);
                col += 0.125f * tex2D(_MainTex, i.uv + float2(0, SAMPLE_DIST));
                col += 0.125f * tex2D(_MainTex, i.uv + float2(0, -SAMPLE_DIST));
                col += 0.125f * tex2D(_MainTex, i.uv + float2(SAMPLE_DIST, 0));
                col += 0.125f * tex2D(_MainTex, i.uv + float2(-SAMPLE_DIST, 0));
                col += 0.0625f * tex2D(_MainTex, i.uv + float2(-SAMPLE_DIST, SAMPLE_DIST));
                col += 0.0625f * tex2D(_MainTex, i.uv + float2(SAMPLE_DIST, SAMPLE_DIST));
                col += 0.0625f * tex2D(_MainTex, i.uv + float2(SAMPLE_DIST, -SAMPLE_DIST));
                col += 0.0625f * tex2D(_MainTex, i.uv + float2(-SAMPLE_DIST, -SAMPLE_DIST));

                //Overlay effect
                /*
                fixed4 overlayCol = tex2D(_OverlayTex, i.uv);
                fixed luminance = dot(overlayCol, fixed4(0.2126, 0.7152, 0.0722, 0));
                fixed oldAlpha = overlayCol.a;

                if (luminance < 0.45f)
                    overlayCol *= 2;
                else
                    overlayCol = 1 - 2 * (1 - overlayCol);

                col *= overlayCol;
                col.a = oldAlpha;
                */

                fixed4 overlayCol = tex2D(_OverlayTex, i.uv);
                float4 effect = lerp(1 - (2 * (1 - col)) * (1 - overlayCol), (2 * col) * overlayCol, step(col, 0.5f));

                return lerp(col, effect, (overlayCol.w * OVERLAY_OPACITY));

                //return col;
            }
            ENDCG
        }
    }
}
