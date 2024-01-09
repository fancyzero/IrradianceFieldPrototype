Shader "Unlit/PRT"
{
    Properties
    {
        _MainTex ("Texture", Cube) = "white" {}
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
                float3 n: NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldNormal:TEXCOORD1;
                float4 vertex : SV_POSITION;  
            };

            samplerCUBE _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.worldNormal = v.n;
                return o;
            }

            #define PI 3.1415926
float3 AlignWithWorldNormal(float3 direction, float3 worldNormal)
{
    float3 up = float3(0, 1, 0); // Standard up direction
    if (all(worldNormal == up)) // Avoid division by zero when normals are aligned
    {
        return direction;
    }

    float3 rotationAxis = cross(up, worldNormal);
    float rotationAngle = acos(dot(normalize(up), normalize(worldNormal)));

    float4x4 rotationMatrix = float4x4( // Constructing a rotation matrix
        cos(rotationAngle) + pow(rotationAxis.x, 2.0f) * (1 - cos(rotationAngle)), 
        rotationAxis.x * rotationAxis.y * (1 - cos(rotationAngle)) - rotationAxis.z * sin(rotationAngle), 
        rotationAxis.x * rotationAxis.z * (1 - cos(rotationAngle)) + rotationAxis.y * sin(rotationAngle), 
        0,
        
        rotationAxis.y * rotationAxis.x * (1 - cos(rotationAngle)) + rotationAxis.z * sin(rotationAngle), 
        cos(rotationAngle) + pow(rotationAxis.y, 2.0f) * (1 - cos(rotationAngle)), 
        rotationAxis.y * rotationAxis.z * (1 - cos(rotationAngle)) - rotationAxis.x * sin(rotationAngle), 
        0,

        rotationAxis.z * rotationAxis.x * (1 - cos(rotationAngle)) - rotationAxis.y * sin(rotationAngle), 
        rotationAxis.z * rotationAxis.y * (1 - cos(rotationAngle)) + rotationAxis.x * sin(rotationAngle), 
        cos(rotationAngle) + pow(rotationAxis.z, 2.0f) * (1 - cos(rotationAngle)), 
        0,

        0, 
        0, 
        0, 
        1
    );

    float4 direction4 = float4(direction, 1.0f); // Convert direction to 4D vector for matrix multiplication
    float4 rotatedDirection4 = mul(rotationMatrix, direction4); // Apply rotation
    return rotatedDirection4.xyz; // Convert back to 3D vector
}
            float3 CalculateSampleDirection(int sampleIndex, int totalSamples, float3 worldNormal)
            {
                float goldenRatio = 1.618033988749895;
                float theta = 2 * PI * goldenRatio * sampleIndex; // angle around the spiral
                float phi = acos(1 - 2 * (sampleIndex + 0.5) / totalSamples); // elevation angle

                float3 direction;
                direction.x = sin(phi) * cos(theta);
                direction.y = sin(phi) * sin(theta);
                direction.z = cos(phi);

                // Align the direction with the world normal
                direction = AlignWithWorldNormal(direction, worldNormal);

                return direction;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                // Define the number of samples
                const int N = 1000; // Change this value as needed

                // Initialize the color accumulator
                fixed4 accumulatedCol = fixed4(0, 0, 0, 0);

                float3 brdf = 1;
                // Loop to sample N points from the cube map
                for (int j = 0; j < N; ++j)
                {
                    // Calculate the direction for the current sample
                    // Replace this with your method of generating sample directions
                    float3 sampleDir = CalculateSampleDirection(j, N, i.worldNormal);



                    
                    float3 Li = texCUBElod(_MainTex, float4(normalize(sampleDir), 0));
                    fixed3 sampleCol = Li * max(0,dot(sampleDir,i.worldNormal))*brdf;

                    // Accumulate the sampled color
                    accumulatedCol += sampleCol.xyzz;
                }

                // Average the accumulated color
                fixed4 avgCol = accumulatedCol / N;

                return avgCol;
            }
            ENDCG
        }
    }
}
