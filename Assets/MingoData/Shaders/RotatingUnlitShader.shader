Shader "Custom/RotatingUnlitShader" {
Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _RotationX ("Rotation X (Radians)", Range(-3.14, 3.14)) = 0
    }

    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        float _RotationX;

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            float2 uv = IN.uv_MainTex;
            float v = uv.y * 2.0 - 1.0;
            float s = sin(_RotationX);
            float c = cos(_RotationX);
            float2 rotatedUV;
            rotatedUV.x = uv.x;
            rotatedUV.y = (v * c + 1.0) / 2.0 + (uv.y - 0.5) * s;

            o.Albedo = tex2D(_MainTex, rotatedUV).rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}