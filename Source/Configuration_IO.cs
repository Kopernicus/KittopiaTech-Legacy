using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace PFUtilityAddon
{
	public class ConfigIO
	{
		private static bool IsTypeHasFields(Type type)
		{
			return type.GetFields().Length > 0;
		}

		public static void LoadConfigNode(object obj, ConfigNode node)
		{
			foreach (FieldInfo key in obj.GetType().GetFields())
			{
				Type keytype = key.FieldType;

				//MonoBehaviour.print(key.Name);
				//MonoBehaviour.print(IsTypeHasFields(keytype));
				try
				{
					if (node.HasValue(key.Name))
					{
						string val = node.GetValue(key.Name);

						if (keytype == typeof(PQS))
						{
							string FixName = val;
							FixName = FixName.Replace(" (PQS)", "");
							key.SetValue(obj, Utils.FindPQS(FixName));
						}
						else if (keytype == typeof(UnityEngine.Vector3))
						{
							val = val.Replace("(", "");
							val = val.Replace(")", "");

							key.SetValue(obj, ConfigNode.ParseVector3(val));
						}
						else if (keytype == typeof(UnityEngine.Color))
						{
							val = val.Replace("RGBA(", "");
							val = val.Replace(")", "");
							key.SetValue(obj, ConfigNode.ParseColor(val));
						}
						else if (keytype == typeof(MapSO))
						{
							val = val.Replace(" (MapSO)", "");
							if (Utils.FileExists(val))
							{
								Texture2D texture = null;
								bool localload = Utils.LoadTexture(val, ref texture, false, false, false);
								MapSO ReturnedMapSo = (MapSO)ScriptableObject.CreateInstance(typeof(MapSO));
								if (key.Name.Equals("heightMap"))
									ReturnedMapSo.CreateMap(MapSO.MapDepth.Greyscale, texture);
								else
									ReturnedMapSo.CreateMap(MapSO.MapDepth.RGB, texture);
								key.SetValue(obj, ReturnedMapSo);
								if (localload)
								{
									UnityEngine.Object.DestroyImmediate(texture);
									texture = null;
								}
							}
						}
						else
						{
							key.SetValue(obj, Convert.ChangeType((System.Object)val, keytype));
							//MonoBehaviour.print("Generic " + val);
						}
					}
					else if (node.HasNode(key.Name))
					{
						ConfigNode subnode = node.GetNode(key.Name);

						if (keytype.IsArray)
						{
							//MonoBehaviour.print("Array");
							//MonoBehaviour.print(subnode.nodes.Count);
							for (int i = 0; i < subnode.nodes.Count; i++)
							{
								ConfigNode sub2node = subnode.nodes[i];
								LoadConfigNode(((object[])key.GetValue(obj))[i], sub2node);
							}
						}
						else if (IsTypeHasFields(keytype))
						{
							//MonoBehaviour.print("Subnode");
							LoadConfigNode(key.GetValue(obj), subnode);
						}
					}
				}
				catch (Exception ex)
				{
					MonoBehaviour.print("PlanetUI: Failed to load: " + obj + ", Exception: " + ex);
					MonoBehaviour.print("Stacktrace: " + ex.StackTrace);
					continue;
				}
			}
		}
	}
}
