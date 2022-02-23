using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Fab.Lua.Editor
{
	/// <summary>
	/// Scripted importer for files with .lua extension.
	/// Will create a text asset from the lua script.
	/// </summary>
	[ScriptedImporter(1, "lua")]
	public class LuaScriptImporter : ScriptedImporter
	{
		public override void OnImportAsset(AssetImportContext ctx)
		{
			TextAsset textAsset = new TextAsset(File.ReadAllText(ctx.assetPath));
			ctx.AddObjectToAsset("lua script", textAsset);
			ctx.SetMainObject(textAsset);
		}
	}

	[CustomEditor(typeof(LuaScriptImporter))]
	internal class LuaScriptImporterEditor : AssetImporterEditor
	{
		protected override bool needsApplyRevert => false;
		public override void OnInspectorGUI() { }
	}
}
