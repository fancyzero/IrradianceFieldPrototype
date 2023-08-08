using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Windows;
//using UnityEnigne.Rendering;

public class CaptureProbe : EditorWindow
{


    [MenuItem("Window/Capture Cubemap")]
    static void Init()
    {
        CaptureProbe window = (CaptureProbe)EditorWindow.GetWindow(typeof(CaptureProbe));
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Capture Cubemap", EditorStyles.boldLabel);



        if (GUILayout.Button("Capture Cubemap"))
        {
            CaptureCubemap();
        }
    }
    
    //Convert Cubemap to texture array by given cubemap
    private RenderTexture ConvertCubemapToTextureArray(Cubemap cubeMap)
    {
        RenderTextureDescriptor desc = new RenderTextureDescriptor();
        desc.autoGenerateMips = false;
        desc.bindMS = false;
        desc.colorFormat = RenderTextureFormat.ARGBFloat;
        desc.depthBufferBits = 0;
        desc.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
        desc.enableRandomWrite = false;
        desc.height = cubeMap.height;
        desc.width = cubeMap.width;
        desc.msaaSamples = 1;
        desc.sRGB = true;
        desc.useMipMap = false;
        desc.volumeDepth = 6;
        RenderTexture converted_input = new RenderTexture(desc);
        converted_input.Create();

        for (int face = 0; face < 6; ++face)
            Graphics.CopyTexture(cubeMap, face, 0, converted_input, face, 0);
        return converted_input;
    }

    //Create 3D Texture fomr 3d array of floats
    private Texture3D Create3DTexture(SkyVisibilityProbeData SkyVisData)
    {
        
        int xSize = SkyVisData.xSize;
        int ySize = SkyVisData.ySize;
        int zSize = SkyVisData.zSize;


        Texture3D tex = new Texture3D(xSize, ySize, ySize, TextureFormat.RGBAFloat, false);

        Color[] colors = new Color[xSize * ySize * zSize];
        int index = 0;
        for (int x = 0; x < xSize; x++)
        {
            for (int y =0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    var probe = SkyVisData.probes[x+ y * xSize+ z * xSize * ySize];
                    colors[index] = new Color(probe.coeffs[0], probe.coeffs[0], probe.coeffs[0]);
                    index++;
                }
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    private void CaptureCubemap()
    {
        var camerObj = new GameObject();
        var camera = camerObj.AddComponent<Camera>();
        camerObj.AddComponent<DepthCamera>();
        camera.fieldOfView = 90;

        var volumes = FindObjectsOfType<GIVolume>();
        int i = 0;
        try
        {
            foreach (var v in volumes)
            {
                var probeCount = v.GetProbeCount();
                SkyVisibilityProbeData.SkyVisibilityProbe[,,] skyVisibilityProbeArray = new SkyVisibilityProbeData.SkyVisibilityProbe[probeCount.x, probeCount.y, probeCount.z];
                var probes = v.GetProbePositions();
                for ( int x =0; x < probeCount.x; x++)
                {
                    for (int y = 0; y < probeCount.y; y++)
                    {
                        for (int z = 0; z < probeCount.z; z++)
                        {
                            Vector3 probPos = probes[x,y,z];
                            if (EditorUtility.DisplayCancelableProgressBar("Cancelable", string.Format("Caputring {0}/{1}", i, probes.Length), (float)(i) / probes.Length))
                                break;
                            var cubeMap = new Cubemap(128, TextureFormat.RGBAFloat, false);
                            camera.gameObject.transform.position = probPos;
                            //camera.SetReplacementShader(Shader.Find("Unlit/CaptureDepth"), "RenderType");
                            camera.RenderToCubemap(cubeMap);
                            Vector4[] output = new Vector4[9];
                            GPU_Project_Uniform_9Coeff(cubeMap, output);

                            var skyVisibilityCoeffs = new float[9];
                            for (int j = 0; j < 9; j++)
                            {
                                skyVisibilityCoeffs[j] = output[j].x;
                            }
                            skyVisibilityProbeArray[x, y, z].coeffs = skyVisibilityCoeffs;
                            AssetDatabase.CreateAsset(cubeMap, string.Format("Assets/CubeMaps/{0}.asset", i));
                            i += 1;
                        }
                    }
                }

                // save sky visibility probe array to asset
                var asset = ScriptableObject.CreateInstance<SkyVisibilityProbeData>();
                asset.probes = new List<SkyVisibilityProbeData.SkyVisibilityProbe>();
                asset.xSize = probeCount.x;
                asset.ySize = probeCount.y;
                asset.zSize = probeCount.z;
                for (int x = 0; x < probeCount.x; x++)
                {
                    for (int y = 0; y < probeCount.y; y++)
                    {
                        for (int z = 0; z < probeCount.z; z++)
                        {
                            asset.probes.Add(skyVisibilityProbeArray[x, y, z]);
                        }
                    }
                }
                AssetDatabase.CreateAsset(asset, string.Format("Assets/VolumeData/SkyVis_{0}.asset", v.name));

                var newTexture = Create3DTexture(asset);
                AssetDatabase.CreateAsset(newTexture, string.Format("Assets/VolumeData/SkyVis_{0}_Texture.asset", v.name));






                break;
            }
        }
        finally
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        DestroyImmediate(camerObj);
        Debug.Log("Cubemap saved as an asset.");


    }

    public static bool GPU_Project_Uniform_9Coeff(Cubemap input, Vector4[] output)
    {
        //the starting number of groups 
        int ceiled_size = Mathf.CeilToInt(input.width / 8.0f);

        ComputeBuffer output_buffer = new ComputeBuffer(9, 16);  //the output is a buffer with 9 float4
        ComputeBuffer ping_buffer = new ComputeBuffer(ceiled_size * ceiled_size * 6, 16);
        ComputeBuffer pong_buffer = new ComputeBuffer(ceiled_size * ceiled_size * 6, 16);

        ComputeShader reduce = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/SH/Reduce_Uniform.compute");

        //can't have direct access to the cubemap in the compute shader (I think), so i copy the cubemap faces onto a texture2d array
        RenderTextureDescriptor desc = new RenderTextureDescriptor();
        desc.autoGenerateMips = false;
        desc.bindMS = false;
        desc.colorFormat = RenderTextureFormat.ARGBFloat;
        desc.depthBufferBits = 0;
        desc.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
        desc.enableRandomWrite = false;
        desc.height = input.height;
        desc.width = input.width;
        desc.msaaSamples = 1;
        desc.sRGB = true;
        desc.useMipMap = false;
        desc.volumeDepth = 6;
        RenderTexture converted_input = new RenderTexture(desc);
        converted_input.Create();

        for (int face = 0; face < 6; ++face)
            Graphics.CopyTexture(input, face, 0, converted_input, face, 0);

        //cycle 9 coefficients
        for (int c = 0; c < 9; ++c)
        {
            ceiled_size = Mathf.CeilToInt(input.width / 8.0f);

            int kernel = reduce.FindKernel("sh_" + c.ToString());
            reduce.SetInt("coeff", c);

            //first pass, I compute the integral and make a first pass of reduction
            reduce.SetTexture(kernel, "input_data", converted_input);
            reduce.SetBuffer(kernel, "output_buffer", ping_buffer);
            reduce.SetBuffer(kernel, "coefficients", output_buffer);
            reduce.SetInt("ceiled_size", ceiled_size);
            reduce.SetInt("input_size", input.width);
            reduce.SetInt("row_size", ceiled_size);
            reduce.SetInt("face_size", ceiled_size * ceiled_size);
            reduce.Dispatch(kernel, ceiled_size, ceiled_size, 1);

            //second pass, complete reduction
            kernel = reduce.FindKernel("Reduce");

            int index = 0;
            ComputeBuffer[] buffers = { ping_buffer, pong_buffer };
            while (ceiled_size > 1)
            {
                reduce.SetInt("input_size", ceiled_size);
                ceiled_size = Mathf.CeilToInt(ceiled_size / 8.0f);
                reduce.SetInt("ceiled_size", ceiled_size);
                reduce.SetBuffer(kernel, "coefficients", output_buffer);
                reduce.SetBuffer(kernel, "input_buffer", buffers[index]);
                reduce.SetBuffer(kernel, "output_buffer", buffers[(index + 1) % 2]);
                reduce.Dispatch(kernel, ceiled_size, ceiled_size, 1);
                index = (index + 1) % 2;
            }
        }

        Vector4[] data = new Vector4[9];
        output_buffer.GetData(data);
        for (int c = 0; c < 9; ++c)
            output[c] = data[c];

        pong_buffer.Release();
        ping_buffer.Release();
        output_buffer.Release();
        return true;
    }
}
