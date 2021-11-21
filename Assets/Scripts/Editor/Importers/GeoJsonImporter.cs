using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

namespace Fab.Geo.Editor
{

    [ScriptedImporter(1, "cube")]
    public class GeoJsonImporter : ScriptedImporter
    {
        public float m_Scale = 1;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var position = JsonUtility.FromJson<Vector3>(File.ReadAllText(ctx.assetPath));

            cube.transform.position = position;
            cube.transform.localScale = new Vector3(m_Scale, m_Scale, m_Scale);

            // 'cube' is a a GameObject and will be automatically converted into a prefab
            // (Only the 'Main Asset' is elligible to become a Prefab.)
            ctx.AddObjectToAsset("main obj", cube);
            ctx.SetMainObject(cube);

            var material = new Material(Shader.Find("Standard"));
            material.color = Color.red;

            // Assets must be assigned a unique identifier string consistent across imports
            ctx.AddObjectToAsset("my Material", material);

            // Assets that are not passed into the context as import outputs must be destroyed
            var tempMesh = new Mesh();
            DestroyImmediate(tempMesh);
        }
    }
}s