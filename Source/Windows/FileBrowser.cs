using UnityEngine;
using System.Collections;
using System.IO;

namespace Kopernicus
{
    namespace UI
    {
        public class FileBrowser : MonoBehaviour
        {
            public static string location = "";
            public static Vector2 directoryScroll;
            public static Vector2 fileScroll;

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


                if (!location.EndsWith("GameData") && GUI.Button(new Rect(10, 20, 510, 20), "Up one level"))
                {
                    directoryInfo = directoryInfo.Parent;
                    location = directoryInfo.FullName;
                }
                else if (location.EndsWith("GameData"))
                {
                    GUI.Label(new Rect(10, 20, 510, 20), "Up one level", GUI.skin.button); // Design-Hack, hehe :D
                }


                // Handle the directories list
                GUILayout.BeginArea(new Rect(10, 45, 250, 300));
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
                GUILayout.BeginArea(new Rect(270, 45, 250, 300));
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


                // The manual location box and the select button
                GUILayout.BeginArea(new Rect(10, 350, 510, 20));
                GUILayout.BeginHorizontal();
                GUILayout.Label(location.Replace(Path.Combine(Directory.GetCurrentDirectory(), "GameData") + Path.DirectorySeparatorChar, ""), GUI.skin.textArea, new GUILayoutOption[0]);

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

                return selected;
            }
        }
    }
}