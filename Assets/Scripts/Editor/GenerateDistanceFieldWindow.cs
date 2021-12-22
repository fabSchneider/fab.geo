using UnityEngine;
using UnityEditor;
using Fab.Geo.Gen;

namespace Fab.Geo.Editor
{
    public class GenerateDistanceFieldWindow : EditorWindow
    {
        private static readonly int MinResolution = 8;
        private static readonly int MaxResolution = 16384;

        public enum OutputFormat
        {
            PNG,
            EXR
        }


        [SerializeField]
        Texture2D mask;

        [SerializeField]
        bool invertMask;

        [SerializeField]
        OutputFormat format;

        [SerializeField]
        bool customOutputResolution;

        [SerializeField]
        Vector2Int resolution = new Vector2Int(512, 512);

        [MenuItem("FabGeo/Generate/Distance Field")]
        private static void Init()
        {
            GenerateDistanceFieldWindow window = (GenerateDistanceFieldWindow)GetWindow(typeof(GenerateDistanceFieldWindow));
            window.Show();
        }

        private GenerateDistanceField generator;


        private void OnEnable()
        {
            generator = new GenerateDistanceField();
        }

        private void OnDisable()
        {
        }

        private void OnGUI()
        {
            SerializedObject sObj = new SerializedObject(this);

            GUILayout.Label("Generator Settings", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField(sObj.FindProperty(nameof(mask)));
            EditorGUILayout.PropertyField(sObj.FindProperty(nameof(invertMask)));

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
                if (mask)
                    res = new Vector2Int(mask.width, mask.height);
                else
                    res = new Vector2Int(MinResolution, MinResolution);
            }

            sObj.ApplyModifiedProperties();
            GUILayout.Space(13);
            EditorGUI.BeginDisabledGroup(!mask);
            if (GUILayout.Button("Generate"))
            {
                RenderTexture dst = new RenderTexture(
                    new RenderTextureDescriptor(
                        res.x, res.y, 
                        format == OutputFormat.EXR ? RenderTextureFormat.RFloat : RenderTextureFormat.R8));

                dst.wrapMode = TextureWrapMode.Repeat;
                dst.enableRandomWrite = true;
                dst.name = mask.name + "_distance";

                generator.invertMask = invertMask;
                generator.Generate(mask, dst);

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
