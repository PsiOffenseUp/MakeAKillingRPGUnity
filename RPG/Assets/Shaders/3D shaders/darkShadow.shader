Shader "Custom/darkShadow" {
    Properties{
        _MainTex("Texture", 2D) = "white" {}
    }

        SubShader{
            Tags { 
                "RenderType" = "Opaque" 
            }
            CGPROGRAM
            #pragma surface surf Lambert fullforwardshadows 

            

            // in v2f struct;
             // replace 0 and 1 with the next available TEXCOORDs in your shader, don't put a semicolon at the end of this line.

            // in vert shader;
             // Calculates shadow and light attenuation and passes it to the frag shader.

            //in frag shader;
             // This is a float for your shadow/attenuation value, multiply your lighting value by this to get shadows. Replace i with whatever you've defined your input struct to be called (e.g. frag(v2f [b]i[/b]) : COLOR { ... ).


            struct Input {
                float2 uv_MainTex;
            };

            sampler2D _MainTex;

            void surf(Input IN, inout SurfaceOutput o) {
                o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
            }
            ENDCG
    }
        Fallback "Diffuse"
}