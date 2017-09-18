/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using System.Linq;
using Kopernicus.UI.Enumerations;
using UnityEngine;
using System.Reflection;
using KSP.Localization;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// Stores Localization strings
        /// </summary>
        public class Localization
        {
            public static String LOC_KITTOPIATECH_BIOMEWINDOW { get; private set; }
            public static String LOC_KITTOPIATECH_BIOMEWINDOW_ADD { get; private set; }
            public static String LOC_KITTOPIATECH_BIOMEWINDOW_REMOVE { get; private set; }
            public static String LOC_KITTOPIATECH_COLORWINDOW { get; private set; }
            public static String LOC_KITTOPIATECH_CURVEWINDOW { get; private set; }
            public static String LOC_KITTOPIATECH_ENUMWINDOW { get; private set; }
            public static String LOC_KITTOPIATECH_FILEWINDOW { get; private set; }
            public static String LOC_KITTOPIATECH_FILEWINDOW_FILES { get; private set; }
            public static String LOC_KITTOPIATECH_FILEWINDOW_BUILTIN { get; private set; }
            public static String LOC_KITTOPIATECH_FILEWINDOW_UP { get; private set; }
            public static String LOC_KITTOPIATECH_FILEWINDOW_DIRECTORIES { get; private set; }
            public static String LOC_KITTOPIATECH_FILEWINDOW_FOR_TYPE { get; private set; }
            public static String LOC_KITTOPIATECH_FILEWINDOW_SELECT { get; private set; }
            public static String LOC_KITTOPIATECH_LANDCLASSWINDOW { get; private set; }
            public static String LOC_KITTOPIATECH_LANDCLASSWINDOW_ADD { get; private set; }
            public static String LOC_KITTOPIATECH_LANDCLASSWINDOW_REMOVE { get; private set; }
            public static String LOC_KITTOPIATECH_LERPRANGEWINDOW { get; private set; }
            public static String LOC_KITTOPIATECH_MATERIALWINDOW { get; private set; }
            public static String LOC_KITTOPIATECH_NOISEMODWINDOW { get; private set; }
            public static String LOC_KITTOPIATECH_PLANETWINDOW { get; private set; }
            public static String LOC_KITTOPIATECH_PLANETWINDOW_CURRENT { get; private set; }
            public static String LOC_KITTOPIATECH_PLANETWINDOW_NOCURRENT { get; private set; }
            public static String LOC_KITTOPIATECH_PLANETWINDOW_AFG_EDITOR { get; private set; }
            public static String LOC_KITTOPIATECH_PLANETWINDOW_CB_EDITOR { get; private set; }
            public static String LOC_KITTOPIATECH_PLANETWINDOW_PQS_EDITOR { get; private set; }
            public static String LOC_KITTOPIATECH_PLANETWINDOW_ORBIT_EDITOR { get; private set; }
            public static String LOC_KITTOPIATECH_PLANETWINDOW_SCALED_EDITOR { get; private set; }
            public static String LOC_KITTOPIATECH_PLANETWINDOW_LIGHT_EDITOR { get; private set; }
            public static String LOC_KITTOPIATECH_PLANETWINDOW_RING_EDITOR { get; private set; }
            public static String LOC_KITTOPIATECH_PLANETWINDOW_PARTICLES_EDITOR { get; private set; }
            public static String LOC_KITTOPIATECH_PLANETWINDOW_SAVE { get; private set; }
            public static String LOC_KITTOPIATECH_PLANETWINDOW_INSTANTIATE { get; private set; }
            public static String LOC_KITTOPIATECH_AFGEDITOR_ADD { get; private set; }
            public static String LOC_KITTOPIATECH_AFGEDITOR_UPDATE { get; private set; }
            public static String LOC_KITTOPIATECH_ORBITEDITOR_UPDATE { get; private set; }
            public static String LOC_KITTOPIATECH_PARTICLEEDITOR_ADD { get; private set; }
            public static String LOC_KITTOPIATECH_PARTICLEEDITOR_UPDATE { get; private set; }
            public static String LOC_KITTOPIATECH_PARTICLEEDITOR_REMOVE { get; private set; }
            public static String LOC_KITTOPIATECH_PARTICLEEDITOR_COLOR { get; private set; }
            public static String LOC_KITTOPIATECH_PQSEDITOR_ADD { get; private set; }
            public static String LOC_KITTOPIATECH_PQSEDITOR_ADD_MOD { get; private set; }
            public static String LOC_KITTOPIATECH_PQSEDITOR_ADD_OCEAN { get; private set; }
            public static String LOC_KITTOPIATECH_PQSEDITOR_ADD_HAZOCEAN { get; private set; }
            public static String LOC_KITTOPIATECH_PQSEDITOR_REBUILD { get; private set; }
            public static String LOC_KITTOPIATECH_PQSEDITOR_REMOVE_OCEAN { get; private set; }
            public static String LOC_KITTOPIATECH_PQSEDITOR_REMOVE_MOD { get; private set; }
            public static String LOC_KITTOPIATECH_RINGEDITOR_ADD { get; private set; }
            public static String LOC_KITTOPIATECH_RINGEDITOR_UPDATE { get; private set; }
            public static String LOC_KITTOPIATECH_RINGEDITOR_REMOVE { get; private set; }
            public static String LOC_KITTOPIATECH_SCALEDEDITOR_UPDATEMESH { get; private set; }
            public static String LOC_KITTOPIATECH_SCALEDEDITOR_UPDATETEX { get; private set; }
            public static String LOC_KITTOPIATECH_STARLIGHTEDITOR_NOSTAR { get; private set; }
            public static String LOC_KITTOPIATECH_SELECTORWINDOW { get; private set; }
            public static String LOC_KITTOPIATECH_SIMPLEXWINDOW { get; private set; }
            public static String LOC_KITTOPIATECH_EXIT { get; private set; }
            public static String LOC_KITTOPIATECH_APPLY { get; private set; }
            public static String LOC_KITTOPIATECH_EDIT { get; private set; }
            public static String LOC_KITTOPIATECH_REMOVE { get; private set; }
            public static String LOC_KITTOPIATECH_EDIT_COLOR { get; private set; }
            public static String LOC_KITTOPIATECH_LOAD_CBMAP { get; private set; }
            public static String LOC_KITTOPIATECH_EDIT_BIOMES { get; private set; }
            public static String LOC_KITTOPIATECH_LOAD_TEXTURE { get; private set; }
            public static String LOC_KITTOPIATECH_EDIT_LANDCLASSES { get; private set; }
            public static String LOC_KITTOPIATECH_EDIT_LERPRANGE { get; private set; }
            public static String LOC_KITTOPIATECH_EDIT_SIMPLEX { get; private set; }
            public static String LOC_KITTOPIATECH_EDIT_NOISEMOD { get; private set; }
            public static String LOC_KITTOPIATECH_LOAD_MAPSO { get; private set; }
            public static String LOC_KITTOPIATECH_EDIT_SPHERE { get; private set; }
            public static String LOC_KITTOPIATECH_EDIT_BODY { get; private set; }
            public static String LOC_KITTOPIATECH_EDIT_MATERIAL { get; private set; }
            public static String LOC_KITTOPIATECH_EDIT_CURVE { get; private set; }
            public static String LOC_KITTOPIATECH_EDIT_MESH { get; private set; }

            static Localization()
            {
                PropertyInfo[] properties = typeof(Localization).GetProperties(BindingFlags.Static | BindingFlags.Public);
                foreach (PropertyInfo info in properties)
                {
                    info.SetValue(null, Localizer.Format($"#{info.Name}"), null);
                }
            }
        }
    }
}