using Fab.Lua.Core;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Fab.Geo.Lua.Interop
{
	[LuaHelpInfo("Module for loading image and text")]
	public class IO : LuaObject, ILuaObjectInitialize
	{
		private static readonly HashSet<string> imageExtensions = new HashSet<string>() { ".jpg", ".jpeg", ".png" };
		private static readonly HashSet<string> textExtensions = new HashSet<string>() { ".txt", ".json", ".geojson" };

		[LuaHelpInfo("Returns the directory path that data can be loaded from (read only)")]
		public string data_dir => LuaEnvironment.DataDirectory;

		public void Initialize()
		{

		}

		[LuaHelpInfo("Loads a text(txt, json, geojson) or image file(jpg, png) from the data path")]
		public object load(string file)
		{
			string loadPath = Path.Combine(LuaEnvironment.DataDirectory, file);

			if (!File.Exists(loadPath))
				throw new ArgumentException("Loading failed. The path does not exist");

			string ext = Path.GetExtension(file);

			if (string.IsNullOrEmpty(ext))
				throw new ArgumentException($"Loading failed. The path is missing a file extension");

			if (imageExtensions.Contains(ext))
			{
				//load image
				Texture2D tex = new Texture2D(2, 2);
				tex.LoadImage(File.ReadAllBytes(loadPath));
				tex.name = Path.GetFileNameWithoutExtension(loadPath);
				var proxy = new ImageProxy();
				proxy.SetTarget(tex);
				return proxy;
			}
			else if (textExtensions.Contains(ext))
			{
				//load text
				return File.ReadAllText(Path.Combine(LuaEnvironment.DataDirectory, file));
			}

			throw new ArgumentException($"Loading failed. The file extension \"{ext}\" is not supported");
		}
	}
}
