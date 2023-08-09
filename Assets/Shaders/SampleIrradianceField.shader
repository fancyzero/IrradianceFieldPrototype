Shader "IrradianceField/SampleField"
{
    Properties
    {
        _MainTex ("Texture", 3D) = "white" {}
        _VolumeCenter("Volume Center", Vector) = (0,0,0,0)
        _VolumeSize("Volume Size", Vector) = (1,1,1,1)
       
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 WorldPos:TEXCOORD1;
                float3 WorldNormal:TEXCOORD2;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler3D _MainTex;
            float4 _MainTex_ST;
            float3 _VolumeCenter;
            float3 _VolumeSize;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.WorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.WorldNormal = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //translate worldPos to texture coordinates by given volume size and offset
                
                //corner
                

                float3 texCoord = (i.WorldPos - _VolumeCenter + _VolumeSize/2) / _VolumeSize;
                // sample the texture
                fixed4 col = 0;
                col.xyz = tex3D(_MainTex, texCoord);



                    





                return col;
            }
            ENDCG
        }
    }
}
