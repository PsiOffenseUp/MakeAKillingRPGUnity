Shader "Unlit/diamond_ui"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale ("Scale", Range(0.0, 1.0)) = 0.5
        _Color ("Color", Color) = (0.0, 0.0, 0.0)
        _BGColor("BGColor", Color) = (1.0, 0.1, 0.6, 0.0)
    }
    SubShader
    {
        Tags { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent" 
        }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Scale;
            float4 _BGColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //Draw a diamond of the given scale
                float halfScale = 0.5 * _Scale;

                return (i.uv.y < i.uv.x + halfScale && i.uv.y < -i.uv.x + halfScale + 1.0 &&
                    i.uv.y > i.uv.x - halfScale && i.uv.y > -i.uv.x - halfScale + 1.0)
                    ? _Color : _BGColor;
            }
            ENDCG
        }
    }
}
