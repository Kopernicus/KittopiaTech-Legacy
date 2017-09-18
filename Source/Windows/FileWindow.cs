/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Kopernicus.UI.Enumerations;
using UnityEngine;
using Object = System.Object;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class renders a window to load files
        /// </summary>
        [Position(500, 20, 530, 410)]
        public class FileWindow : Window<String>
        {
            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected override String Title()
            {
                return $"KittopiaTech - {Localization.LOC_KITTOPIATECH_FILEWINDOW}";
            }
            
            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                if (Current == "")
                    Current = Path.Combine(Directory.GetCurrentDirectory(), "GameData") + Path.DirectorySeparatorChar;
                if (!Show()) return;
                Callback(Current);
                UIController.Instance.DisableWindow(KittopiaWindows.Files);
            }

            // Variables for the browser
            public static String location = "";
            public static Vector2 directoryScroll;
            public static Vector2 fileScroll;
            public static Boolean builtin = false;
            public static Object value;
            public static Type type;

            static Int32 mode = 0;

            // Class that renders a file-browser for Texture-Loading
            // Code from the Unify-Wiki, adapted by Thomas P.
            private Boolean Show()
            {
                // Our return state - altered by the "Select" button
                Boolean complete = false;

                mode = GUI.Toolbar(new Rect(10, 20, 510, 20), mode, new[] { Localization.LOC_KITTOPIATECH_FILEWINDOW_FILES, Localization.LOC_KITTOPIATECH_FILEWINDOW_BUILTIN });
                builtin = mode == 1;

                if (!builtin)
                {
                    // Get the directory info of the current location
                    FileInfo fileSelection = String.IsNullOrEmpty(location) ? null : new FileInfo(location);
                    DirectoryInfo directoryInfo = fileSelection == null ? new DirectoryInfo(KSPUtil.ApplicationRootPath + "GameData/") : (fileSelection.Attributes & FileAttributes.Directory) == FileAttributes.Directory ? new DirectoryInfo(location) : fileSelection.Directory;

                    if (!location.EndsWith("GameData") && GUI.Button(new Rect(10, 45, 510, 20), Localization.LOC_KITTOPIATECH_FILEWINDOW_UP))
                    {
                        directoryInfo = directoryInfo?.Parent;
                        location = directoryInfo?.FullName;
                    }
                    else if (location.EndsWith("GameData"))
                    {
                        GUI.Label(new Rect(10, 45, 510, 20), Localization.LOC_KITTOPIATECH_FILEWINDOW_UP, GUI.skin.button); // Design-Hack, hehe :D
                    }

                    // Handle the directories list
                    GUILayout.BeginArea(new Rect(10, 70, 250, 300));
                    GUILayout.Label(Localization.LOC_KITTOPIATECH_FILEWINDOW_DIRECTORIES + ":");
                    directoryScroll = GUILayout.BeginScrollView(directoryScroll);

                    DirectoryInfo directorySelection = SelectList(directoryInfo?.GetDirectories(), null, GUI.skin.button, GUI.skin.button) as DirectoryInfo;
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();

                    if (directorySelection != null)
                    // If a directory was selected, jump there
                    {
                        location = directorySelection.FullName;
                    }

                    // Handle the files list
                    GUILayout.BeginArea(new Rect(270, 70, 250, 300));
                    GUILayout.Label(Localization.LOC_KITTOPIATECH_FILEWINDOW_FILES + ":");
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
                    GUI.Label(new Rect(10, 45, 510, 20), Localization.LOC_KITTOPIATECH_FILEWINDOW_UP, GUI.skin.button);
                    // Handle the directories list
                    GUILayout.BeginArea(new Rect(10, 70, 510, 300));
                    GUILayout.Label(Localization.LOC_KITTOPIATECH_FILEWINDOW_FOR_TYPE + " " + type.Name + ":");
                    directoryScroll = GUILayout.BeginScrollView(directoryScroll);
                    value = SelectList(Resources.FindObjectsOfTypeAll(type), value, GUI.skin.button, GUI.skin.button);
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                }

                // The manual location box and the select button
                GUILayout.BeginArea(new Rect(10, 375, 510, 25));
                GUILayout.BeginHorizontal();
                GUILayout.Label(builtin ? value?.ToString() ?? "" : location.Replace(Path.Combine(Directory.GetCurrentDirectory(), "GameData") + Path.DirectorySeparatorChar, ""), GUI.skin.textArea);

                Int32 contentWidth = (int)GUI.skin.GetStyle("Button").CalcSize(new GUIContent(Localization.LOC_KITTOPIATECH_FILEWINDOW_SELECT)).x;
                if (GUILayout.Button(Localization.LOC_KITTOPIATECH_FILEWINDOW_SELECT, GUILayout.Width(contentWidth)))
                {
                    complete = true;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndArea();

                Current = builtin ? value?.ToString() : location;
                return complete;
            }

            // List-Utils
            private static Object SelectList(IEnumerable list, Object selected, GUIStyle defaultStyle, GUIStyle selectedStyle)
            {
                foreach (Object item in list)
                {
                    if (builtin)
                    {
                        if (String.IsNullOrEmpty((item as UnityEngine.Object)?.name)) continue;
                        if (!GUILayout.Button((item as UnityEngine.Object).name, (selected == item) ? selectedStyle : defaultStyle)) continue;
                        selected = selected == item ? null : item;
                    }
                    else
                    {
                        if (!GUILayout.Button(item.ToString().Replace(Path.Combine(Directory.GetCurrentDirectory(), "GameData") + Path.DirectorySeparatorChar, ""), (selected == item) ? selectedStyle : defaultStyle)) continue;
                        selected = selected == item ? null : item;
                    }
                }
                return selected;
            }
        }
    }
}