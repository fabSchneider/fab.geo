using MoonSharp.Interpreter;
using System.IO;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class IOProxy 
    {
        private string dataDirectory;

        [MoonSharpHidden]
        public IOProxy(string dataDirectory)
        {
            this.dataDirectory = dataDirectory;
        }

        public TextureProxy load_image(string path)
        {
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(File.ReadAllBytes(Path.Combine(dataDirectory, path)));
            tex.name = Path.GetFileNameWithoutExtension(path);
            return new TextureProxy(tex);
        }
    }
}
