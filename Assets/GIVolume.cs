using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GIVolume : MonoBehaviour
{
    public Color gizmoColor = Color.yellow;
    public Vector3 size = new Vector3(1f, 1f, 1f);
    public Vector3 probeDensity = new Vector3(1, 1, 1);
    private void OnDrawGizmos()
    {
        
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1);
        


        var probes = GetProbePositions();
        int cubesX = Mathf.FloorToInt(size.x);
        int cubesY = Mathf.FloorToInt(size.y);
        int cubesZ = Mathf.FloorToInt(size.z);

        float stepX = size.x / cubesX;
        float stepY = size.y / cubesY;
        float stepZ = size.z / cubesZ;

        foreach ( var prob in probes)
        {
            Gizmos.DrawCube(prob, new Vector3(stepX * 0.1f, stepY * 0.1f, stepZ * 0.1f));
        }
      
    }

    public List<Vector3> GetProbePositions()
    {
        List < Vector3 > positions = new List < Vector3 >();
       // Calculate the number of cubes needed in each dimension
        int cubesX = Mathf.FloorToInt(size.x);
        int cubesY = Mathf.FloorToInt(size.y);
        int cubesZ = Mathf.FloorToInt(size.z);

        // Calculate the step size for placing cubes
        float stepX = size.x / cubesX;
        float stepY = size.y / cubesY;
        float stepZ = size.z / cubesZ;

        for (int x = 0; x < cubesX; x++)
        {
            for (int y = 0; y < cubesY; y++)
            {
                for (int z = 0; z < cubesZ; z++)
                {
                    // Calculate the position of the current cube
                    float posX = (x * stepX) - (size.x * 0.5f) + (stepX * 0.5f);
                    float posY = (y * stepY) - (size.y * 0.5f) + (stepY * 0.5f);
                    float posZ = (z * stepZ) - (size.z * 0.5f) + (stepZ * 0.5f);
                    Vector3 cubePosition = new Vector3(posX, posY, posZ);

                    // Draw a cube at the calculated position
                    positions.Add(transform.TransformPoint(cubePosition));
                }
            }
        }
        return positions;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, size);
    }
}
