using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KRU.Game 
{
    public class CheckBiome : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100000f))
                {
                    Debug.DrawLine(ray.origin, hit.point, Color.green, 15f);

                    MeshCollider meshCollider = hit.collider as MeshCollider;
                    if (meshCollider == null || meshCollider.sharedMesh == null)
                        return;

                    Mesh mesh = meshCollider.sharedMesh;
                    Vector3[] vertices = mesh.vertices;
                    int[] triangles = mesh.triangles;
                    Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
                    Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
                    Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
                    Transform hitTransform = hit.collider.transform;
                    p0 = hitTransform.TransformPoint(p0);
                    p1 = hitTransform.TransformPoint(p1);
                    p2 = hitTransform.TransformPoint(p2);
                    Debug.DrawLine(p0, p1, Color.green, 15f);
                    Debug.DrawLine(p1, p2, Color.green, 15f);
                    Debug.DrawLine(p2, p0, Color.green, 15f);

                    Debug.Log($"{mesh.name} : {mesh.normals[triangles[hit.triangleIndex * 3 + 0]]}");

                    /*var color1 = mesh.colors[triangles[hit.triangleIndex * 3 + 0]];
                    var color2 = mesh.colors[triangles[hit.triangleIndex * 3 + 1]];
                    var color3 = mesh.colors[triangles[hit.triangleIndex * 3 + 2]];

                    var biome = "";
                    if (color1.r > 0)
                        biome = "Forest";
                    else if (color1.b > 0)
                        biome = "Desert";
                    else if (color1.g > 0)
                        biome = "Muddy Plains";

                    Debug.Log(biome);*/
                }
            }
        }
    }
}

