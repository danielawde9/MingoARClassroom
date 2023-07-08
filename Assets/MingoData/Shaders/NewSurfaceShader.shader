Shader "Custom/NewSurfaceShader"
{
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include <UnityLightingCommon.cginc>

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float3 worldPos : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                float3 worldLightPos = _WorldSpaceLightPos0.xyz;
                float3 lightDir = normalize(worldLightPos - i.worldPos);
                float ndotl = max(0, dot(i.normal, lightDir));

                // Calculate custom attenuation here
                float dist = distance(i.worldPos, worldLightPos);
                float attenuation = 1.0 / (1.0 + 0.1 * dist); // Adjust 0.1 to control the attenuation rate

                float3 lightColor = _LightColor0.rgb;
                fixed4 col = fixed4(ndotl * lightColor * attenuation, 1.0);
                return col;
            }
            ENDCG
        }
    }
}
