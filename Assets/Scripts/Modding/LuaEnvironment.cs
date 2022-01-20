using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Fab.Geo.Modding
{
    public static class LuaEnvironment 
    {
        public static string DocumentsDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Application.productName);
        public static string ScriptsDirectory => Path.Combine(DocumentsDirectory, "Scripts");
        public static string DataDirectory => Path.Combine(LuaEnvironment.DocumentsDirectory, "Data");
    }
}
