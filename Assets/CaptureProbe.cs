using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

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

    private void CaptureCubemap()
    {
        var camerObj = new GameObject();
        var camera = camerObj.AddComponent<Camera>();
        camera.AddComponent<DepthCamera>();
        camera.fieldOfView = 90;

        var volumes = FindObjectsOfType<GIVolume>();
        int i = 0;
        try
        {
            foreach (var v in volumes)
            {
                var probes = v.GetProbePositions();
                foreach (var prob in probes)
                {

                    if (EditorUtility.DisplayCancelableProgressBar("Cancelable", string.Format("Caputring {0}/{1}", i , probes.Count), (float)(i) / probes.Count))
                        break;
                    if (i > 10)
                        break;
                    var cubeMap = new Cubemap(128, TextureFormat.RGBAFloat, false);
                    camera.gameObject.transform.position = prob;
                    //camera.SetReplacementShader(Shader.Find("Unlit/CaptureDepth"), "RenderType");
                    camera.RenderToCubemap(cubeMap);
                    ProjectCubeToSH(cubeMap);
                    AssetDatabase.CreateAsset(cubeMap, string.Format("Assets/CubeMaps/{0}.asset", i));
                    i += 1;
                }
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

    void ProjectCubeToSH(Cubemap cube)
    {
        ComputeShader shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/ProjectCubeToSH.compute");
        shader.SetTexture(0, "cube", cube);
        var outputBuffer = new ComputeBuffer(1024, 12);

        shader.SetBuffer(0, "output", outputBuffer);
        shader.Dispatch(0, 1024/8, 1, 1);
        Vector3[] output = new Vector3[1024];
        outputBuffer.GetData(output);
        outputBuffer.Release();
        Vector3 sum = Vector3.zero;
        foreach( var o in output)
        {
            sum += o;
        }
        Debug.Log(sum);
        

    }
}
