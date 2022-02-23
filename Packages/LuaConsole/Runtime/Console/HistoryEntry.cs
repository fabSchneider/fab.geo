using UnityEngine;

namespace Fab.Lua.Console
{
	public class HistoryEntry
	{
		private string code;

		private string print;

		public Texture image;

		public string Code { get => code; }
		public string Print { get => print; }
		public Texture Image { get => image; }

		public HistoryEntry(string code, string print, Texture image = null)
		{
			this.code = code;
			this.print = print;
			this.image = image;
		}
	}
}
