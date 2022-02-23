using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Lua.Console
{
	public class History : IReadOnlyList<HistoryEntry>
	{
		private int maxEntries;
		public int MaxEntries => maxEntries;

		private int maxTextLength;
		public int MaxTextLength => maxTextLength;

		public History(int maxEntries, int maxTextLength)
		{
			this.maxEntries = maxEntries;
			this.maxTextLength = maxTextLength;
			codeHistory = new List<HistoryEntry>(maxEntries);
		}
		private List<HistoryEntry> codeHistory;
		public HistoryEntry this[int index] => codeHistory[index];
		public int Count => codeHistory.Count;
		public IEnumerator<HistoryEntry> GetEnumerator() => codeHistory.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => codeHistory.GetEnumerator();

		private List<string> texts = new List<string>();

		private Texture image;

		public void Clear()
		{
			codeHistory.Clear();
		}

		public void AddHistoryEntry(string code)
		{
			if (codeHistory.Count > 0 && codeHistory.Count >= maxEntries)
				codeHistory.RemoveAt(0);

			codeHistory.Add(new HistoryEntry(code, string.Join(Environment.NewLine, texts), image));
			texts.Clear();
			image = null;
		}

		public void AddText(string output, bool trim = true)
		{
			if (trim && output.Length > maxTextLength)
			{
				//trim excess print output and add a message informing about the cut
				string cut = output.Substring(0, maxTextLength);
				cut += Environment.NewLine + $" ... ";
				texts.Add(cut);
			}
			else
				texts.Add(output);
		}

		public void AddImage(Texture2D image)
		{
			this.image = image;
		}

	}
}
