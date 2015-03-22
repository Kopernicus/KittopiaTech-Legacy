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
		// Currently unsupported types; will add support to some of them in the future.
		private static Type[] unsupported_type =
		{
			typeof(AnimationCurve),
			typeof(CelestialBody),
			typeof(List<>),
			typeof(PQS),
			typeof(PQS.ModiferRequirements),
			typeof(Stack<>),
			typeof(Texture2D),
			typeof(UnityEngine.Cubemap),
			typeof(UnityEngine.GameObject),
			typeof(UnityEngine.Gradient),
			typeof(UnityEngine.HideFlags),
			typeof(UnityEngine.Material),
			typeof(UnityEngine.Mesh),
		};

		// List of types that only needs to be saved as a string.
		private static Type[] string_save =
		{
			typeof(MapSO),
			typeof(string), // Duh!
			typeof(UnityEngine.Color),
			typeof(UnityEngine.Vector3)
		};

		private static bool IsTypeHasFieldsOrProperties(Type type)
		{
			return type.GetFields().Length > 0 || type.GetProperties().Length > 0;
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

						if (keytype == typeof(UnityEngine.Vector3))
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
						else if (keytype.IsEnum)
						{
							key.SetValue(obj, Enum.Parse(keytype, val));
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
						else if (IsTypeHasFieldsOrProperties(keytype))
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

		public static void SaveConfigNode(object obj, ConfigNode node)
		{
			//MonoBehaviour.print(obj.GetType());
			//MonoBehaviour.print("----------------");

			foreach (PropertyInfo key in obj.GetType().GetProperties())
			{
				try
				{
					Type keytype = key.PropertyType;
					if (keytype.IsGenericType) keytype = keytype.GetGenericTypeDefinition();

					//MonoBehaviour.print(key.Name + " " + keytype + " " + key.GetValue(obj, null));
					//MonoBehaviour.print(key.DeclaringType);

					// Ignore properties that came from some Unity base class.
					if (key.DeclaringType == typeof(GameObject) || key.DeclaringType == typeof(Component)
						|| key.DeclaringType == typeof(Behaviour) || key.DeclaringType == typeof(MonoBehaviour))
					{
						continue;
					}

					// Only support modifiable properties.
					if (!key.CanRead || !key.CanWrite) continue;

					// Don't save unsupported type.
					if (unsupported_type.Contains(keytype)) continue;

					// Save some class members in string form.
					if (string_save.Contains(keytype))
					{
						//MonoBehaviour.print("Property String save");
						node.AddValue(key.Name, key.GetValue(obj, null));
					}
					else if (keytype.IsEnum)
					{
						//MonoBehaviour.print("Property Enum");
						node.AddValue(key.Name, Enum.GetName(keytype, key.GetValue(obj, null)));
					}
					else if (IsTypeHasFieldsOrProperties(keytype) && !keytype.IsPrimitive)
					{
						//MonoBehaviour.print("Property Subnode");
						ConfigNode subnode = new ConfigNode(key.Name);
						SaveConfigNode(key.GetValue(obj, null), subnode);

						if (subnode.HasNode() || subnode.HasValue()) node.AddNode(subnode);
					}
					else
					{
						//MonoBehaviour.print("Property Generic");
						node.AddValue(key.Name, key.GetValue(obj, null));
					}
				}
				catch (Exception ex)
				{
					//MonoBehaviour.print(ex.Message + ex.StackTrace);
				}
			}

			foreach (FieldInfo key in obj.GetType().GetFields())
			{
				try
				{
					Type keytype = key.FieldType;
					if (keytype.IsGenericType) keytype = keytype.GetGenericTypeDefinition();

					//MonoBehaviour.print(key.Name + " " + keytype + " " + key.GetValue(obj));

					// Don't save unsupported type.
					if (unsupported_type.Contains(keytype)) continue;
				
					// Save some class members in string form.
					if (string_save.Contains(keytype))
					{
						//MonoBehaviour.print("String save");
						node.AddValue(key.Name, key.GetValue(obj));
					}
					else if (keytype.IsArray)
					{
						//MonoBehaviour.print("Array");

						if (!unsupported_type.Contains(keytype.GetElementType()))
						{
							ConfigNode subnode = new ConfigNode(key.Name);

							object[] objarray = (object[])key.GetValue(obj);

							foreach (object subobj in objarray)
							{
								ConfigNode sub2node = new ConfigNode(subobj.GetType().ToString());
								SaveConfigNode(subobj, sub2node);
								subnode.AddNode(sub2node);
							}

							if (subnode.HasNode() || subnode.HasValue()) node.AddNode(subnode);
						}
					}
					else if (keytype.IsEnum)
					{
						//MonoBehaviour.print("Enum");
						node.AddValue(key.Name, Enum.GetName(keytype, key.GetValue(obj)));
					}
					else if (IsTypeHasFieldsOrProperties(keytype) && !keytype.IsPrimitive)
					{
						//MonoBehaviour.print("Subnode");
						ConfigNode subnode = new ConfigNode(key.Name);
						SaveConfigNode(key.GetValue(obj), subnode);

						if (subnode.HasNode() || subnode.HasValue()) node.AddNode(subnode);
					}
					else
					{
						//MonoBehaviour.print("Generic");
						node.AddValue(key.Name, key.GetValue(obj));
					}
				}
				catch (Exception ex)
				{
					//MonoBehaviour.print(ex.Message + ex.StackTrace);
				}
			}

			//MonoBehaviour.print("----------------");
		}
	}
}
