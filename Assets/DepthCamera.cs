using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthCamera : MonoBehaviour
{
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        var matCopyDepth = new Material(Shader.Find("Hidden/FetchDepth"));
        Graphics.Blit(source, destination,matCopyDepth);
    }

}
