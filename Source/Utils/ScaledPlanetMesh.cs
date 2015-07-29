using System;
using System.Linq;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        public class ScaledPlanetMesh
        {
            public Vector3[] vertices;
            public Vector3 localScale;
            public float radius;

            public ScaledPlanetMesh(int size)
            {
                vertices = new Vector3[size];
            }

            public void ApplyToScaledSpace(GameObject scaled)
            {
                var meshfilter = scaled.GetComponent<MeshFilter>();
                var collider = scaled.GetComponent<SphereCollider>();

                meshfilter.sharedMesh.vertices = vertices;
                meshfilter.sharedMesh.RecalculateNormals();
                Utils.RecalculateTangents(meshfilter.sharedMesh);

                collider.radius = radius;
                scaled.transform.localScale = localScale;
            }

            public static ScaledPlanetMesh Generate(PQS bodyPQS, Mesh meshinput)
            {
                float joolScaledRad = 1000f;
                float joolRad = 6000000f;
                float scale = (float)bodyPQS.radius / joolScaledRad;

                ScaledPlanetMesh mesh = new ScaledPlanetMesh(meshinput.vertices.Count());

                // One could use pqs.radiusMin and pqs.radiusMax to determine minimum and maximum height.
                // But to be safe, the height limit values will be determined manually.
                float radiusMin = 0;
                float radiusMax = 0;

                bodyPQS.isBuildingMaps = true;
                for (int i = 0; i < meshinput.vertices.Count(); i++)
                {
                    var vertex = meshinput.vertices[i];
                    var rootrad = (float)Math.Sqrt(vertex.x * vertex.x +
                                    vertex.y * vertex.y +
                                    vertex.z * vertex.z);
                    var radius = (float)bodyPQS.GetSurfaceHeight(vertex) / scale;
                    mesh.vertices[i] = vertex * (radius / rootrad);

                    if (i == 0)
                    {
                        radiusMin = radiusMax = radius;
                    }
                    else
                    {
                        if (radiusMin > radius) radiusMin = radius;
                        if (radiusMax < radius) radiusMax = radius;
                    }
                }
                bodyPQS.isBuildingMaps = false;

                // Adjust the mesh so the maximum radius has 1000 unit in scaled space.
                // (so the planets will fit in the science archive list)
                float r = radiusMax / 1000;
                for (int i = 0; i < mesh.vertices.Count(); i++)
                {
                    mesh.vertices[i] /= r;
                }

                // Use the lowest radius as collision radius.
                mesh.radius = radiusMin / r;

                // Calculate the local scale.
                mesh.localScale = Vector3.one * ((float)bodyPQS.radius / joolRad) * r;

                return mesh;
            }

        }
    }
}