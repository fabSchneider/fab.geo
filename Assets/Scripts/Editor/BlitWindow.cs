using UnityEngine;
using UnityEditor;
using Fab.Geo.Gen;
using System.Linq;
using System.Collections.Generic;

namespace Fab.Geo.Editor
{
	public class BlitWindow : EditorWindow
	{
		private static readonly int MinResolution = 8;
		private static readonly int MaxResolution = 16384;

		public enum OutputFormat
		{
			PNG,
			EXR
		}


		[SerializeField]
		List<Texture2D> textures;

		[SerializeField]
		Material blitMaterial;

		[SerializeField]
		OutputFormat format;

		[SerializeField]
		bool customOutputResolution;

		[SerializeField]
		Vector2Int resolution = new Vector2Int(512, 512);

		[MenuItem("FabGeo/Generate/Blit")]
		private static void Init()
		{
			BlitWindow window = (BlitWindow)GetWindow(typeof(BlitWindow));
			window.Show();
		}

		SerializedObject sObj;

		private void OnEnable()
		{
			ScriptableObject target = this;
			sObj = new SerializedObject(target);
		}

		private void OnGUI()
		{
			sObj.Update();

			GUILayout.Label("Generator Settings", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(sObj.FindProperty(nameof(textures)));
			EditorGUILayout.PropertyField(sObj.FindProperty(nameof(blitMaterial)));

			EditorGUILayout.PropertyField(sObj.FindProperty(nameof(format)));

			SerializedProperty customResFlagProp = sObj.FindProperty(nameof(customOutputResolution));
			EditorGUILayout.PropertyField(customResFlagProp);

			Vector2Int res;

			if (customResFlagProp.boolValue)
			{
				SerializedProperty outputResProp = sObj.FindProperty(nameof(resolution));
				EditorGUILayout.PropertyField(outputResProp);
				res = outputResProp.vector2IntValue;
				res = new Vector2Int(Mathf.Clamp(res.x, MinResolution, MaxResolution), Mathf.Clamp(res.y, MinResolution, MaxResolution));
				outputResProp.vector2IntValue = res;
			}
			else
			{
				res = new Vector2Int(MinResolution, MinResolution);
			}

			sObj.ApplyModifiedProperties();
			GUILayout.Space(13);
			EditorGUI.BeginDisabledGroup(textures.Count < 1 || !blitMaterial);
			if (GUILayout.Button("Generate"))
			{
				if (!customOutputResolution)
				{
					int[] width = textures.Select(t => t == null ? MinResolution : t.width).ToArray();
					int[] heigth = textures.Select(t => t == null ? MinResolution : t.height).ToArray();

					res = new Vector2Int(Mathf.Min(width), Mathf.Min(heigth));
				}

				RenderTexture dst = new RenderTexture(
					new RenderTextureDescriptor(
						res.x, res.y, format == OutputFormat.EXR ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.ARGB32));

				dst.wrapMode = TextureWrapMode.Repeat;
				dst.enableRandomWrite = true;
				dst.name = $"{textures[0].name}_{blitMaterial.name}";

				for (int i = 0; i < textures.Count; i++)
				{
					blitMaterial.SetTexture($"_Tex{i + 1}", textures[i]);
				}

				Graphics.Blit(textures[0], dst, blitMaterial, 0);

				switch (format)
				{
					case OutputFormat.PNG:
						EditorUtils.SaveTexturePNG(dst.ToTexture2D());
						break;
					case OutputFormat.EXR:
						EditorUtils.SaveTextureEXR(dst.ToTexture2D(), Texture2D.EXRFlags.OutputAsFloat);
						break;
					default:
						break;
				}
			}
			EditorGUI.EndDisabledGroup();
		}
	}
}
