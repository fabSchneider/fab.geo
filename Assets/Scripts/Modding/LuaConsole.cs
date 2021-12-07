using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Geo.Modding
{
    [RequireComponent(typeof(UIDocument))]
    public class LuaConsole : MonoBehaviour
    {
        private LuaManager manager;
        private Script script;

        private LuaConsoleHistory history;
        public LuaConsoleHistory History => history;

        [SerializeField]
        [Tooltip("Maximum number of items in the history.")]
        private int maxHistoryEntries = 24;

        public int MaxHistoryEntries => maxHistoryEntries;

        private void Awake()
        {
            manager = FindObjectOfType<LuaManager>();
            script = manager.CreateScript("live-script");
            history = new LuaConsoleHistory(maxHistoryEntries);
        }

        public void Execute(string code)
        {
            script.DoString(code);
            history.Add(code);
        }
    }

    public class LuaConsoleHistory : IReadOnlyList<string>
    {

        private int maxEntries;
        public int MaxEntries => maxEntries;

        public LuaConsoleHistory(int maxEntries)
        {
            this.maxEntries = maxEntries;
            history = new List<string>(maxEntries);
        }

        private List<string> history;
        public string this[int index] => history[index];
        public int Count => history.Count;
        public IEnumerator<string> GetEnumerator() => history.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => history.GetEnumerator();

        public void Clear()
        {
            history.Clear();
        }

        public void Add(string entry)
        {
            if (history.Count > 0 && history.Count >= maxEntries)
                history.RemoveAt(0);

            history.Add(entry);
        }
    }
}
