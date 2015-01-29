using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace PFUtilityAddon
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
			RecalculateTangents(meshfilter.sharedMesh);

			collider.radius = radius;
			scaled.transform.localScale = localScale;
		}

		public static ScaledPlanetMesh Load(string name)
		{
			var filepath = "GameData/KittopiaSpace/ScaledSpace/" + name + ".bin";

			if (!File.Exists(filepath))
			{
				return null;
			}
			else
			{
				var stream = File.OpenRead(filepath);
				var reader = new BinaryReader(stream);

				int vec_count = (int)(reader.BaseStream.Length - 4 * sizeof(float)) / (3 * sizeof(float));
				ScaledPlanetMesh mesh = new ScaledPlanetMesh(vec_count);

				mesh.radius = reader.ReadSingle();
				mesh.localScale.x = reader.ReadSingle();
				mesh.localScale.y = reader.ReadSingle();
				mesh.localScale.z = reader.ReadSingle();

				int i = 0;
				while (i < vec_count)
				{
					mesh.vertices[i].x = reader.ReadSingle();
					mesh.vertices[i].y = reader.ReadSingle();
					mesh.vertices[i].z = reader.ReadSingle();
					i++;
				}
				reader.Close();

				return mesh;
			}
		}

		public static void Save(string name, ScaledPlanetMesh planet_mesh)
		{
			var filepath = "GameData/KittopiaSpace/ScaledSpace/" + name + ".bin";

			Directory.CreateDirectory("GameData/KittopiaSpace/ScaledSpace");
			var stream = File.Open(filepath, FileMode.Create);
			var writer = new BinaryWriter(stream);

			writer.Write(planet_mesh.radius);
			writer.Write(planet_mesh.localScale.x);
			writer.Write(planet_mesh.localScale.y);
			writer.Write(planet_mesh.localScale.z);

			foreach (var vec in planet_mesh.vertices)
			{
				writer.Write(vec.x);
				writer.Write(vec.y);
				writer.Write(vec.z);
			}
			writer.Close();
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

		private static void RecalculateTangents(Mesh theMesh)
		{
			int vertexCount = theMesh.vertexCount;
			Vector3[] vertices = theMesh.vertices;
			Vector3[] normals = theMesh.normals;
			Vector2[] texcoords = theMesh.uv;
			int[] triangles = theMesh.triangles;
			int triangleCount = triangles.Length / 3;

			var tangents = new Vector4[vertexCount];
			var tan1 = new Vector3[vertexCount];
			var tan2 = new Vector3[vertexCount];

			int tri = 0;

			for (int i = 0; i < (triangleCount); i++)
			{
				int i1 = triangles[tri];
				int i2 = triangles[tri + 1];
				int i3 = triangles[tri + 2];

				Vector3 v1 = vertices[i1];
				Vector3 v2 = vertices[i2];
				Vector3 v3 = vertices[i3];

				Vector2 w1 = texcoords[i1];
				Vector2 w2 = texcoords[i2];
				Vector2 w3 = texcoords[i3];

				float x1 = v2.x - v1.x;
				float x2 = v3.x - v1.x;
				float y1 = v2.y - v1.y;
				float y2 = v3.y - v1.y;
				float z1 = v2.z - v1.z;
				float z2 = v3.z - v1.z;

				float s1 = w2.x - w1.x;
				float s2 = w3.x - w1.x;
				float t1 = w2.y - w1.y;
				float t2 = w3.y - w1.y;

				float r = 1.0f / (s1 * t2 - s2 * t1);
				var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
				var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

				tan1[i1] += sdir;
				tan1[i2] += sdir;
				tan1[i3] += sdir;

				tan2[i1] += tdir;
				tan2[i2] += tdir;
				tan2[i3] += tdir;

				tri += 3;
			}
			for (int i = 0; i < (vertexCount); i++)
			{
				Vector3 n = normals[i];
				Vector3 t = tan1[i];

				// Gram-Schmidt orthogonalize
				Vector3.OrthoNormalize(ref n, ref t);

				tangents[i].x = t.x;
				tangents[i].y = t.y;
				tangents[i].z = t.z;

				// Calculate handedness
				tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;
			}
			theMesh.tangents = tangents;
		}

	}
}
