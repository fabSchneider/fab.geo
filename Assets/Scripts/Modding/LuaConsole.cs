using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Geo.Modding
{
    [RequireComponent(typeof(UIDocument))]
    public class LuaConsole : MonoBehaviour
    {
        private static readonly string className = "lua-console";
        private static readonly string textFieldClassName = className + "__text-field";
        private static readonly string historyClassName = className + "__history";
        private static readonly string historyItemClassName = className + "__history-item";

        private LuaManager manager;
        private UIDocument doc;
        private TextField consoleTextField;
        private VisualElement consoleHistory;

        private Script script;

        [SerializeField]
        [Tooltip("Maximum number of items in the history.")]
        private int maxHistoryEntries = 24;

        private void Start()
        {
            manager = FindObjectOfType<LuaManager>();

            script = manager.CreateScript("live-script");

            doc = GetComponent<UIDocument>();
            consoleTextField = doc.rootVisualElement.Q<TextField>(className: textFieldClassName);
            consoleTextField.RegisterValueChangedCallback(OnTextFieldChange);
            consoleHistory = doc.rootVisualElement.Q(className: historyClassName);
        }

        private void OnTextFieldChange(ChangeEvent<string> evt)
        {
            string consoleLine = evt.newValue;
            consoleTextField.SetValueWithoutNotify(string.Empty);
            consoleTextField.Focus();

            script.DoString(consoleLine);

            if (consoleHistory.childCount >= maxHistoryEntries && consoleHistory.childCount > 0)
                consoleHistory.RemoveAt(0);

            VisualElement historyItem = CreateHistoryItem(consoleLine);
            consoleHistory.Add(historyItem);
        }


        private VisualElement CreateHistoryItem(string content)
        {
            Label label = new Label(content);
            label.AddToClassList(historyItemClassName);
            return label;
        }
    }
}
