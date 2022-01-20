using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Fab.Geo.Modding.Editor
{
    public class LuaHelpMarkdownGenerator
    {
        /// <summary>
        /// Generates the Lua Help Markdown file and prompts the save dialog
        /// </summary>
        [MenuItem("FabGeo/Lua/Generate Help Docs")]
        public static void GenerateAndSaveHelp()
        {
            LuaHelpMarkdownFormatter formatter = new LuaHelpMarkdownFormatter();
            LuaHelpInfoExtractor extactor = new LuaHelpInfoExtractor();

            StringBuilder sb = new StringBuilder();

            LuaObjectRegistry.RegisterAssembly();
            foreach (Type t in LuaObjectRegistry.GetRegisteredTypes(true))
            {
                LuaHelpInfo info = extactor.GetHelpInfoForType(t);

                sb.Append(formatter.Format(info));
            }

            string path = EditorUtility.SaveFilePanel("Save Help Doc", "", "LuaHelp.md", "md");

            if (path.Length != 0)
            {
                File.WriteAllText(path, sb.ToString());
                Debug.Log("Lua Help Documentation successfully written to " + path);
            }
        }
    }
}
