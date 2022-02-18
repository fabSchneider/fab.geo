using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Lua.Console
{
	public class History : IReadOnlyList<HistoryEntry>
	{
		private int maxEntries;
		public int MaxEntries => maxEntries;

		public History(int maxEntries)
		{
			this.maxEntries = maxEntries;
			codeHistory = new List<HistoryEntry>(maxEntries);
		}
		private List<HistoryEntry> codeHistory;
		public HistoryEntry this[int index] => codeHistory[index];
		public int Count => codeHistory.Count;
		public IEnumerator<HistoryEntry> GetEnumerator() => codeHistory.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => codeHistory.GetEnumerator();

		public void Clear()
		{
			codeHistory.Clear();
		}

		public void Add(string code, string output, Texture image)
		{
			if (codeHistory.Count > 0 && codeHistory.Count >= maxEntries)
				codeHistory.RemoveAt(0);

			codeHistory.Add(new HistoryEntry(code, output, image));
		}
	}
}
