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

        private History history;
        public History ConsoleHistory => history;

        [SerializeField]
        [Tooltip("Maximum number of items in the history.")]
        private int maxHistoryEntries = 24;

        public int MaxHistoryEntries => maxHistoryEntries;

        private string printOutput;

        private void Awake()
        {
            manager = FindObjectOfType<LuaManager>();
            script = manager.CreateScript("live-script");
            script.Options.DebugPrint = print => printOutput = print;
            history = new History(maxHistoryEntries);
        }

        public Result Execute(string code)
        {
            try
            {
                DynValue returnVal = script.DoString(code);
                if (returnVal.IsNotNil())
                {
                    printOutput = returnVal.ToPrintString();
                }
            }
            catch (System.Exception e)
            {
                //try getting a variable from the code
                DynValue val = script.Globals.Get(code);
                if(val != null && val.IsNotNil())
                    printOutput = val.ToPrintString();
                else
                    return new Result() { success = false, errorMsg = e.Message };
            }

            history.Add(code, printOutput);
            printOutput = null;
            return new Result() { success = true };
        }


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

            public void Add(string code, string output)
            {
                if (codeHistory.Count > 0 && codeHistory.Count >= maxEntries)
                    codeHistory.RemoveAt(0);

                codeHistory.Add(new HistoryEntry(code, output));
            }
        }

        public struct Result
        {
            public bool success;
            public string errorMsg;
        }

        public class HistoryEntry
        {
            private string code;

            private string print;
            public string Code { get => code; }
            public string Print { get => print; }

            public HistoryEntry(string code, string print)
            {
                this.code = code;
                this.print = print;
            }
        }
    }
}