using Fab.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Geo.Modding
{
    [RequireComponent(typeof(LuaConsole))]
    [RequireComponent(typeof(UIDocument))]
    public class LuaConsoleUI : MonoBehaviour
    {
        private static readonly string className = "lua-console";
        private static readonly string textFieldClassName = className + "__text-field";
        private static readonly string historyClassName = className + "__history";
        private static readonly string historyItemClassName = className + "__history-item";

        private LuaConsole console;
        private UIDocument doc;
        private TextField consoleTextField;
        private VisualElement consoleHistory;

        private ObjectPool<Label> historyEntryPool;

        /// <summary>
        /// The current position in the history triggered through the up arrow
        /// </summary>
        private int historyPosition = 0;

        private void Start()
        {
            console = GetComponent<LuaConsole>();

            doc = GetComponent<UIDocument>();
            consoleTextField = doc.rootVisualElement.Q<TextField>(className: textFieldClassName);
            
            consoleTextField.RegisterCallback<KeyDownEvent>(OnTextFieldKeyDown);
            consoleTextField.RegisterCallback<NavigationSubmitEvent>(OnTextFieldSubmit);

            consoleHistory = doc.rootVisualElement.Q(className: historyClassName);
            historyEntryPool = new ObjectPool<Label>(console.History.MaxEntries, false,
                () =>
                {
                    Label entry = new Label();
                    entry.AddToClassList(historyItemClassName);
                    return entry;
                },
                entry =>
                {
                    entry.text = string.Empty;
                    entry.RemoveFromHierarchy();
                });
        }

        private void OnTextFieldSubmit(NavigationSubmitEvent evt)
        {
            string code = consoleTextField.text;
            consoleTextField.SetValueWithoutNotify(string.Empty);
            consoleTextField.Focus();

            console.Execute(code);
            UpdateHistory();
        }

        private void OnTextFieldKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.UpArrow)
            {
                historyPosition = 0;
                return;
            }

            if (historyPosition >= console.History.Count)
                return;

            historyPosition++;
            consoleTextField.SetValueWithoutNotify(console.History[console.History.Count - historyPosition]);
            consoleTextField.SelectRange(consoleTextField.text.Length - 1, consoleTextField.text.Length - 1);
            evt.StopPropagation();
        }

        private void UpdateHistory()
        {
            for (int i = consoleHistory.childCount - 1; i >= 0; i--)
                historyEntryPool.ReturnToPool((Label)consoleHistory[i]);

            foreach (var entry in console.History)
            {
                Label item = historyEntryPool.GetPooled();
                item.text = entry;
                consoleHistory.Add(item);
            }
        }
    }
}
