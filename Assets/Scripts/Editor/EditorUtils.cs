using UnityEngine;
using UnityEditor;
using System.IO;

namespace Fab.Geo.Editor
{

	public static class EditorUtils
	{
		public static Texture2D SaveTexturePNG(Texture2D texture)
		{
			string path = EditorUtility.SaveFilePanelInProject("Save texture", texture.name, "png",
		   "Please enter a file name to save the texture to");
			if (path.Length != 0)
			{
				var bytes = texture.EncodeToPNG();
				File.WriteAllBytes(path, bytes);
				AssetDatabase.Refresh();
				return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
			}
			return null;
		}

		public static Texture2D SaveTextureEXR(Texture2D texture, Texture2D.EXRFlags exRFlags)
		{
			string path = EditorUtility.SaveFilePanelInProject("Save texture", texture.name, "exr",
		   "Please enter a file name to save the texture to");
			if (path.Length != 0)
			{
				var bytes = texture.EncodeToEXR(exRFlags);
				File.WriteAllBytes(path, bytes);
				AssetDatabase.Refresh();
				return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
			}

			return null;
		}

		public static Texture2D ToTexture2D(this RenderTexture rTex)
		{
			Texture2D tex = new Texture2D(rTex.width, rTex.height, rTex.graphicsFormat, 0);
			tex.name = rTex.name;
			var old_rt = RenderTexture.active;
			RenderTexture.active = rTex;

			tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
			tex.Apply();

			RenderTexture.active = old_rt;
			return tex;
		}

	}
}
