using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        public class FileBrowser : MonoBehaviour
        {
            public static string location = "";
            public static Vector2 directoryScroll;
            public static Vector2 fileScroll;
            public static bool builtin = false;
            public static object value;
            public static Type type;

            static int mode = 0;

            // Class that renders a file-browser for Texture-Loading
            // Code from the Unify-Wiki, adapted by Thomas P.
            private static bool Show()
            {
                bool complete;
                DirectoryInfo directoryInfo;
                DirectoryInfo directorySelection;
                FileInfo fileSelection;
                int contentWidth;

                // Our return state - altered by the "Select" button
                complete = false;

                // Get the directory info of the current location
                fileSelection = new FileInfo(location);
                if ((fileSelection.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    directoryInfo = new DirectoryInfo(location);
                }
                else
                {
                    directoryInfo = fileSelection.Directory;
                }

                mode = GUI.Toolbar(new Rect(10, 20, 510, 20), mode, new[] { "Files", "Builtin" });
                builtin = mode == 1;

                if (!builtin)
                {
                    if (!location.EndsWith("GameData") && GUI.Button(new Rect(10, 45, 510, 20), "Up one level"))
                    {
                        directoryInfo = directoryInfo.Parent;
                        location = directoryInfo.FullName;
                    }
                    else if (location.EndsWith("GameData"))
                    {
                        GUI.Label(new Rect(10, 45, 510, 20), "Up one level", GUI.skin.button); // Design-Hack, hehe :D
                    }

                    // Handle the directories list
                    GUILayout.BeginArea(new Rect(10, 70, 250, 300));
                    GUILayout.Label("Directories:");
                    directoryScroll = GUILayout.BeginScrollView(directoryScroll);

                    directorySelection = SelectList(directoryInfo.GetDirectories(), null, GUI.skin.button, GUI.skin.button) as DirectoryInfo;
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();

                    if (directorySelection != null)
                    // If a directory was selected, jump there
                    {
                        location = directorySelection.FullName;
                    }

                    // Handle the files list
                    GUILayout.BeginArea(new Rect(270, 70, 250, 300));
                    GUILayout.Label("Files:");
                    fileScroll = GUILayout.BeginScrollView(fileScroll);
                    fileSelection = SelectList(directoryInfo.GetFiles(), null, GUI.skin.button, GUI.skin.button) as FileInfo;
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();

                    if (fileSelection != null)
                    // If a file was selected, update our location to it
                    {
                        location = fileSelection.FullName;
                    }
                }
                else
                {
                    // Design
                    GUI.Label(new Rect(10, 45, 510, 20), "Up one level", GUI.skin.button);
                    // Handle the directories list
                    GUILayout.BeginArea(new Rect(10, 70, 510, 300));
                    GUILayout.Label("Builtin files for type " + type.Name + ":");
                    directoryScroll = GUILayout.BeginScrollView(directoryScroll);
                    value = SelectList(Resources.FindObjectsOfTypeAll(type), value, GUI.skin.button, GUI.skin.button);
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                }

                // The manual location box and the select button
                GUILayout.BeginArea(new Rect(10, 375, 510, 25));
                GUILayout.BeginHorizontal();
                GUILayout.Label(builtin ? value != null ? value.ToString() : "" : location.Replace(Path.Combine(Directory.GetCurrentDirectory(), "GameData") + Path.DirectorySeparatorChar, ""), GUI.skin.textArea, new GUILayoutOption[0]);

                contentWidth = (int)GUI.skin.GetStyle("Button").CalcSize(new GUIContent("Select")).x;
                if (GUILayout.Button("Select", GUILayout.Width(contentWidth)))
                {
                    complete = true;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndArea();

                return complete;
            }

            // Return the GUI-Window
            public static Rect Render(Rect rect, string title)
            {
                return GUI.Window(223456, rect, RenderFuction, title);
            }

            private static void RenderFuction(int windowID)
            {
                if (location == "")
                    location = Path.Combine(Directory.GetCurrentDirectory(), "GameData") + Path.DirectorySeparatorChar;

                if (Show())
                {
                    UIController.Instance.isFileBrowser = false;
                }
            }

            // List-Utils
            private static object SelectList(ICollection list, object selected, GUIStyle defaultStyle, GUIStyle selectedStyle)
            {
                foreach (object item in list)
                {
                    if (builtin)
                    {
                        if (item != null && !String.IsNullOrEmpty((item as UnityEngine.Object).name))
                        {
                            if (GUILayout.Button((item as UnityEngine.Object).name, (selected == item) ? selectedStyle : defaultStyle))
                            {
                                if (selected == item)
                                // Clicked an already selected item. Deselect.
                                {
                                    selected = null;
                                }
                                else
                                {
                                    selected = item;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(item.ToString().Replace(Path.Combine(Directory.GetCurrentDirectory(), "GameData") + Path.DirectorySeparatorChar, ""), (selected == item) ? selectedStyle : defaultStyle))
                        {
                            if (selected == item)
                            // Clicked an already selected item. Deselect.
                            {
                                selected = null;
                            }
                            else
                            {
                                selected = item;
                            }
                        }
                    }
                }

                return selected;
            }
        }
    }
}