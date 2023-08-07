// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/DepthCopy" 
{
	Subshader {
	
	Tags {"RenderType"="Opaque"}
   
// -- DepthTextureCopy
        Pass {
            Fog { Mode Off }
 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

 
            #include "UnityCG.cginc"
           
            sampler2D _CameraDepthTexture;
           
            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };
 
            struct v2f {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };
 
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord=v.texcoord;
                return o;
            }
 
            half4 frag (v2f i) : COLOR
            {
                float depth=Linear01Depth(tex2D(_CameraDepthTexture, i.texcoord).r);
                return depth;
            }
            ENDCG
 
        }
 
	}
	
 	Fallback off
}