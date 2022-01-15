using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    [LuaHelpInfo("Module for loading image and text")]
    public class IOProxy : ProxyBase
    {
        private static readonly HashSet<string> imageExtensions = new HashSet<string>() { ".jpg", ".jpeg", ".png" };
        private static readonly HashSet<string> textExtensions = new HashSet<string>() { ".txt", ".json", ".geojson" };
        public override string Name => "io";

        private string dataDirectory;

        [LuaHelpInfo("Returns the directory path that data can be loaded from (read only)")]
        public string data_dir => dataDirectory;

        [MoonSharpHidden]
        public IOProxy(string dataDirectory)
        {
            this.dataDirectory = dataDirectory;
        }

        [LuaHelpInfo("Loads a text(txt, json, geojson) or image file(jpg, png) from the data path")]
        public object load(string file)
        {
            string loadPath = Path.Combine(dataDirectory, file);

            if (!File.Exists(loadPath))
                throw new ArgumentException("Loading failed. The path does not exist");

            string ext = Path.GetExtension(file);

            if(string.IsNullOrEmpty(ext))
                throw new ArgumentException($"Loading failed. The path is missing a file extension");

            if (imageExtensions.Contains(ext))
            {
                //load image
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(File.ReadAllBytes(loadPath));
                tex.name = Path.GetFileNameWithoutExtension(loadPath);
                return new TextureProxy(tex);
            }
            else if (textExtensions.Contains(ext))
            {
                //load text
                return File.ReadAllText(Path.Combine(dataDirectory, file));
            }

            throw new ArgumentException($"Loading failed. The file extension \"{ext}\" is not supported");
        }
    }
}
