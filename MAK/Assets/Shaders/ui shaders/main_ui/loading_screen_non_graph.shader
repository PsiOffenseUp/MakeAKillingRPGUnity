Shader "Unlit/loading_screen"
{
    Properties
    {
        _CutOutTex("Cut Out Texture", 2D) = "white" {}
        _Size("Size", Range(0.0, 2.0)) = 0.5
        _XPos("X Position", Range(0.0, 1.0)) = 0.5
        _YPos("Y Position", Range(0.0, 1.0)) = 0.5
    }
        SubShader
        {
            Tags {
            "RenderType" = "Transparent"
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
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _CutOutTex;
                float4 _CutOutTex_ST;
                float _Size;
                float _XPos;
                float _YPos;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);

                    o.uv = TRANSFORM_TEX(v.uv, _CutOutTex);
                    o.uv.x -= _XPos - 0.5; //Offset position by given offset
                    o.uv.y -= _YPos - 0.5; //Offset position by given offset
                    o.uv -= 0.5;
                    o.uv /= _Size;
                    o.uv += 0.5;
                    //TRANSFER_VERTEX_TO_FRAGMENT(o);

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = fixed4(0.0, 0.0, 0.0, 1.0);

                    //Clamp the uv values
                    if (i.uv.x < 0.0 || i.uv.x > 1.0 || i.uv.y < 0.0 || i.uv.y > 1.0)
                        col.a = 1.0;
                    else
                        col.a = 1.0 - tex2D(_CutOutTex, i.uv).a;

                     return col;
                 }
                 ENDCG
             }
        } Fallback "VertexLit"
}
