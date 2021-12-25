using MoonSharp.Interpreter;
using System;
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

        [SerializeField]
        [Tooltip("Maximum character length after which a print output will be cut.")]
        private int maxPrintOutputLength = 1024;

        public int MaxHistoryEntries => maxHistoryEntries;

        private List<string> printOutput = new List<string>();

        private Texture2D imageOutput;

        private void Awake()
        {
            history = new History(maxHistoryEntries);
        }

        private void Start()
        {
            manager = FindObjectOfType<LuaManager>();
            script = manager.CreateScript("live-script");
            script.Globals["help"] = (Action<DynValue>)help;
            script.Globals["list"] = (Action)list;

            script.Options.DebugPrint = print => AddToPrintOutput(print);
        }

        private void AddToPrintOutput(string output, bool trim = true)
        {
            if (trim && output.Length > maxPrintOutputLength)
            {
                //trim excess print output and add a message informing about the cut
                string cut = output.Substring(0, maxPrintOutputLength);
                cut += $"\n ... ";
                printOutput.Add(cut);
            }
            else
                printOutput.Add(output);
        }

        private void help(DynValue value)
        {
            object obj = value.ToObject();
            if (obj is ProxyBase proxy)
            {
                if (proxy.IsNil())
                    AddToPrintOutput("Nil", false);
                else
                    AddToPrintOutput(proxy.GetFullDescription(), false);
            }
            else
                AddToPrintOutput("No help information available", false);
        }

        private void list()
        {
            foreach (var proxy in manager.Proxies)
            {
                AddToPrintOutput($"{proxy.Name.PadRight(18, ' ')} \t<i>{proxy.Description}</i>\n", false);
            }
        }

        public Result Execute(string code)
        {
            try
            {
                DynValue returnVal = script.DoString(code);
                if (returnVal.IsNotNil())
                {
                    AddToPrintOutput(returnVal.ToPrintString());
                }
            }
            catch (Exception e)
            {
                //try getting a variable from the code
                DynValue val = script.Globals.RawGet(code);

                if (val != null)
                {
                    object obj = val.ToObject();

                    if (obj is TextureProxy tex)
                        imageOutput = tex.Value;

                    script.DoString($"print({code})");
                }
                else
                {
                    Debug.LogException(e);
                    return new Result() { success = false, errorMsg = e.Message };
                }
            }

            history.Add(code, string.Join('\n', printOutput), imageOutput);
            printOutput.Clear();
            imageOutput = null;
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

            public void Add(string code, string output, Texture2D image)
            {
                if (codeHistory.Count > 0 && codeHistory.Count >= maxEntries)
                    codeHistory.RemoveAt(0);

                codeHistory.Add(new HistoryEntry(code, output, image));
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

            public Texture2D image;

            public string Code { get => code; }
            public string Print { get => print; }
            public Texture2D Image { get => image; }

            public HistoryEntry(string code, string print, Texture2D image = null)
            {
                this.code = code;
                this.print = print;
                this.image = image;
            }
        }
    }
}