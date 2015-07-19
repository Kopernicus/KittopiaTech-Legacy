using Kopernicus.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        // Class to save a CelestialBody as a Kopernicus-configuration-file
        public class ConfigIO
        {
            // Call this on the Body you want to save
            public static void SaveCelestial(CelestialBody body)
            {
                // The conifg-node where we save the body
                ConfigNode root = new ConfigNode("@Kopernicus:AFTER[Kopernicus]");
                ConfigNode bodyNode = null;

                // Are we instanced?
                bool isCopy = false;

                // Determine if the body is already there, or if it is new.
                PSystemBody pBody = Utils.FindBody(PSystemManager.Instance.systemPrefab.rootBody, body.transform.name);
                if (pBody == null)
                {
                    bodyNode = new ConfigNode("Body");
                    pBody = Utils.FindBody(PSystemManager.Instance.systemPrefab.rootBody, PlanetUI.templates[body.transform.name]);
                    isCopy = true;
                }
                else
                {
                    bodyNode = new ConfigNode("@Body[" + body.transform.name + "]");
                }

                // Name-changes
                if (body.bodyName != pBody.name)
                {
                    if (isCopy)
                        bodyNode.AddValue("name", body.bodyName);
                    else
                        if (pBody.name == "Kerbin")
                            bodyNode.AddValue("cbNameLater", body.bodyName);
                        else
                            bodyNode.AddValue("@name", body.bodyName);
                }

                // If we're instanced, we need a Template definition
                if (isCopy)
                {
                    ConfigNode template = new ConfigNode("Template");
                    template.AddValue("name", pBody.name);
                    bodyNode.AddNode(template);
                }

                // Orbit
                ConfigNode orbit = (isCopy) ? new ConfigNode("Orbit") : new ConfigNode("@Orbit");

                // Discover members tagged with parser attributes
                foreach (MemberInfo member in typeof(OrbitLoader).GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    // Is this member a parser target?
                    ParserTarget[] attributes = member.GetCustomAttributes((typeof(ParserTarget)), true) as ParserTarget[];
                    if (attributes.Length > 0)
                    {
                        // Get the Parser Target
                        ParserTarget target = attributes.First();
                        string value = "";
                        string preset = "";

                        // Get matching Fields / Properties
                        IEnumerable<FieldInfo> fields = body.orbit.GetType().GetFields().Where(f => f.Name == Utils.GetField(target.fieldName));
                        IEnumerable<PropertyInfo> properties = body.orbit.GetType().GetProperties().Where(p => p.Name == Utils.GetField(target.fieldName));

                        // Get the current and the previous value
                        if (fields.Count() == 1) 
                        {
                            value = Format(fields.First().GetValue(body.orbit));
                            preset = Format(fields.First().GetValue(pBody.orbitDriver.orbit));
                        }
                        else if (properties.Count() == 1) 
                        {
                            value = Format(properties.First().GetValue(body.orbit, null));
                            preset = Format(properties.First().GetValue(pBody.orbitDriver.orbit, null));
                        }
                        else if (target.fieldName == "color")
                        {
                            value = Format(body.orbitDriver.orbitColor);
                            preset = Format(pBody.orbitRenderer.orbitColor);
                        }
                        else 
                        {
                            continue;
                        }

                        // Check if the value has changed
                        if (value != preset)
                            orbit.AddValue(target.fieldName, value);
                    }
                }

                // If the node has values, add it to the root
                bodyNode.AddNode(orbit);

                // Glue the nodes together
                root.AddNode(bodyNode);

                // Save the node
                Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "/GameData/KittopiaTech/Config/");
                ConfigNode save = new ConfigNode();
                save.AddNode(root);
                save.Save("GameData/KittopiaTech/Config/" + body.name + ".cfg", "KittopiaTech - a Kopernicus Visual Editor");
            }

            // Formats the Output of the object.ToString() function (=> removes the Type-Definition and some other things)
            private static string Format(object input)
            {
                if (input.GetType() == typeof(CelestialBody))
                    return (input as CelestialBody).transform.name;
                if (input.GetType() == typeof(Color))
                    return input.ToString().Replace("RGBA(", "").Replace(")", "");

                return input.ToString();
            }
        }
    }
}
