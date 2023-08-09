using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEditor;

public class GIVolume : MonoBehaviour
{
    public bool drawLabels;
    public bool drawCubes;
    public UnityEngine.Color gizmoColor = UnityEngine.Color.yellow;
    public Vector3 volumeSize = new Vector3(1f, 1f, 1f);
    public float probeSize = 1;//in unit squre
    //a 3d array of vector3
    //function get list of probe positions that fills the whole volume with probeSize
    public Vector3Int GetProbeCount()
    {
        return new Vector3Int(
                       Mathf.FloorToInt(volumeSize.x / probeSize),
                                  Mathf.FloorToInt(volumeSize.y / probeSize),
                                             Mathf.FloorToInt(volumeSize.z / probeSize)
                                                        );
    }
    public Vector3[,,] GetProbePositions()
    {

        // Calculate the number of cubes needed in each dimension
        int cubesX = Mathf.FloorToInt(volumeSize.x / probeSize);
        int cubesY = Mathf.FloorToInt(volumeSize.y / probeSize);
        int cubesZ = Mathf.FloorToInt(volumeSize.z / probeSize);
        // the result array
        Vector3[,,] result = new Vector3[cubesX, cubesY, cubesZ];
        // Calculate the step size
        float stepX = probeSize;
        float stepY = probeSize;
        float stepZ = probeSize;
        // Calculate the start offset which is half of the size of the plane
        // Because the cubes are positioned at the center, we need to start at the half size of the plane to be on the edge
        float startX = -(volumeSize.x / 2) + (stepX / 2);
        float startY = -(volumeSize.y / 2) + (stepY / 2);
        float startZ = -(volumeSize.z / 2) + (stepZ / 2);
        // Create the cubes
        for (int x = 0; x < cubesX; x++)
        {
            for (int y = 0; y < cubesY; y++)
            {
                for (int z = 0; z < cubesZ; z++)
                {
                    result[x, y, z] = new Vector3(startX + (x * stepX), startY + (y * stepY), startZ + (z * stepZ)) + transform.position;
                }
            }
        }
        return result;
    }
    

    private void OnDrawGizmos()
    {
        
        Gizmos.color = new UnityEngine.Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1);
        


        var probes = GetProbePositions();
        var cnt = GetProbeCount();
        for( int x = 0; x < cnt.x; x++)
        {
            for (int y = 0; y < cnt.y; y++)
            {
                for (int z = 0; z < cnt.z; z++)
                {
                    if (drawCubes)
                        Gizmos.DrawCube(probes[x, y, z], new Vector3(0.1f, 0.1f, 0.1f));
                    //draw label on each gizmo
                    if (drawLabels)
                        Handles.Label(probes[x, y, z]+new Vector3(0,0.1f,0), string.Format("{0},{1},{2}",x,y,z));
                }
            }
        }   

      
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, volumeSize);
    }
}
