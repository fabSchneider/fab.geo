using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace Fab.Lua.Console
{
	[RequireComponent(typeof(ConsoleComponent))]
	[RequireComponent(typeof(UIDocument))]
	[AddComponentMenu("FabGeo/Lua/Lua Console View")]
	public class ConsoleView : MonoBehaviour
	{
		private static readonly string className = "lua-console";
		private static readonly string hiddenClassName = "lua-console--hidden";
		private static readonly string containerClassName = className + "__container";
		private static readonly string textFieldClassName = className + "__text-field";
		private static readonly string historyClassName = className + "__history";
		private static readonly string historyContainerName = "history-container";
		private static readonly string errorMsgClassName = className + "__error-msg";

		private static readonly string toggleBtnName = "toggle-btn";
		private static readonly string resetBtnName = "reset-btn";

		private ConsoleComponent consoleComp;
		private UIDocument doc;

		private VisualElement consoleElem;
		private TextField consoleTextField;
		private ScrollView consoleHistory;
		private Label errorMsg;

		private ObjectPool<HistoryEntryElement> historyEntryPool;

		bool isNavigationEvent;

		/// <summary>
		/// The current position in the history triggered through the up arrow
		/// </summary>
		private int selectedHistoryEntry = 0;

		private void Start()
		{
			consoleComp = GetComponent<ConsoleComponent>();

			doc = GetComponent<UIDocument>();

			consoleElem = doc.rootVisualElement.Q(className: className);

			consoleElem.Q<Button>(name: toggleBtnName).clicked += () => consoleElem.ToggleInClassList(hiddenClassName);
			consoleElem.Q<Button>(name: resetBtnName).clicked += () =>
			{
				consoleComp.ResetConsole();
				UpdateHistory();
				consoleTextField.SetValueWithoutNotify(string.Empty);
				errorMsg.style.display = DisplayStyle.None;
			};

			consoleTextField = consoleElem.Q<TextField>(className: textFieldClassName);

			consoleTextField.RegisterCallback<KeyDownEvent>(OnTextFieldKeyDown);
			consoleTextField.RegisterCallback<NavigationSubmitEvent>(OnTextFieldSubmit);

			consoleTextField.RegisterValueChangedCallback(OnValueChanged);

			consoleHistory = consoleElem.Q<ScrollView>(name: historyContainerName);

			historyEntryPool = new ObjectPool<HistoryEntryElement>(
				createFunc: () => new HistoryEntryElement(),
				actionOnRelease: entry => entry.Reset(),
				maxSize: consoleComp.History.MaxEntries);

			errorMsg = consoleElem.Q<Label>(className: errorMsgClassName);
			errorMsg.style.display = DisplayStyle.None;
		}


		private void OnTextFieldSubmit(NavigationSubmitEvent evt)
		{
			consoleTextField.Focus();
			string code = consoleTextField.text;
			if (string.IsNullOrWhiteSpace(code))
				return;

			Console.EvaluateResult res = consoleComp.Console.Evaluate(code);
			if (res.success)
			{
				errorMsg.style.display = DisplayStyle.None;
				consoleTextField.SetValueWithoutNotify(string.Empty);
				UpdateHistory();
			}
			else
			{
				errorMsg.text = res.errorMsg;
				errorMsg.style.display = DisplayStyle.Flex;
			}
		}

		private void OnValueChanged(ChangeEvent<string> evt)
		{
			if (isNavigationEvent)
			{
				consoleTextField.SelectAll();
			}
			isNavigationEvent = false;
		}

		private void OnTextFieldKeyDown(KeyDownEvent evt)
		{
			if (consoleComp.History.Count == 0)
				return;

			//navigate through history
			if (evt.keyCode == KeyCode.UpArrow && selectedHistoryEntry - 1 >= 0)
			{
				//deselect last
				if (selectedHistoryEntry < consoleHistory.childCount)
					((HistoryEntryElement)consoleHistory[selectedHistoryEntry]).SetSelected(false);

				//select next
				selectedHistoryEntry--;
				HistoryEntry entry = consoleComp.History[selectedHistoryEntry];
				var historyElem = ((HistoryEntryElement)consoleHistory[selectedHistoryEntry]);
				historyElem.SetSelected(true);
				isNavigationEvent = true;
				consoleTextField.value = entry.Code;
				consoleHistory.ScrollTo(historyElem);

				evt.StopPropagation();
			}
			else if (evt.keyCode == KeyCode.DownArrow && selectedHistoryEntry + 1 < consoleComp.History.Count)
			{
				//deselect last
				if (selectedHistoryEntry >= 0)
					((HistoryEntryElement)consoleHistory[selectedHistoryEntry]).SetSelected(false);

				//select next
				selectedHistoryEntry++;
				HistoryEntry entry = consoleComp.History[selectedHistoryEntry];
				var historyElem = ((HistoryEntryElement)consoleHistory[selectedHistoryEntry]);
				historyElem.SetSelected(true);
				isNavigationEvent = true;
				consoleTextField.value = entry.Code;
				consoleHistory.ScrollTo(historyElem);

				evt.StopPropagation();
			}
			else
			{
				//deselect last 
				if (selectedHistoryEntry >= 0 && selectedHistoryEntry < consoleHistory.childCount)
					((HistoryEntryElement)consoleHistory[selectedHistoryEntry]).SetSelected(false);
			}
		}

		private void UpdateHistory()
		{
			for (int i = consoleHistory.childCount - 1; i >= 0; i--)
				historyEntryPool.Release((HistoryEntryElement)consoleHistory[i]);

			foreach (var entry in consoleComp.History)
			{
				HistoryEntryElement entryElement = historyEntryPool.Get();
				entryElement.Set(entry.Code, entry.Print, entry.image);
				entryElement.SetSelected(false);
				consoleHistory.Add(entryElement);
			}

			consoleHistory.contentContainer.RegisterCallback<GeometryChangedEvent>(OnHistoryUpdated);
			selectedHistoryEntry = consoleComp.History.Count;
		}

		private void OnHistoryUpdated(GeometryChangedEvent evt)
		{
			VisualElement elem = (VisualElement)evt.target;
			elem.UnregisterCallback<GeometryChangedEvent>(OnHistoryUpdated);
			consoleHistory.verticalScroller.value = consoleHistory.contentContainer.layout.height;
		}
	}

	public class HistoryEntryElement : VisualElement
	{
		private static readonly string className = "history-entry";
		private static readonly string selectedClassName = className + "--selected";
		private static readonly string codeClassName = "history-entry__code";
		private static readonly string imgClassName = "history-entry__image";
		private static readonly string imageHiddenClassName = imgClassName + "--hidden";
		private static readonly string printClassName = "history-entry__print";
		private static readonly string printHiddenClassName = printClassName + "--hidden";

		private Label codeText;
		private Label printText;
		private UnityEngine.UIElements.Image img;

		public HistoryEntryElement()
		{
			AddToClassList(className);
			codeText = new Label();
			codeText.AddToClassList(codeClassName);
			img = new UnityEngine.UIElements.Image();
			img.AddToClassList(imgClassName);
			printText = new Label();
			printText.enableRichText = true;
			printText.AddToClassList(printClassName);

			Add(codeText);
			Add(img);
			Add(printText);
		}

		public void Set(string code, string print, Texture image)
		{
			codeText.text = code;

			if (string.IsNullOrEmpty(print))
			{
				printText.text = string.Empty;
				printText.AddToClassList(printHiddenClassName);
			}
			else
			{
				printText.text = print;
				printText.RemoveFromClassList(printHiddenClassName);
			}

			if (image)
			{
				img.image = image;
				img.RemoveFromClassList(imageHiddenClassName);
			}
			else
			{
				img.image = null;
				img.AddToClassList(imageHiddenClassName);
			}
		}

		public void Reset()
		{
			codeText.text = string.Empty;
			printText.text = string.Empty;
			img.image = null;
			RemoveFromHierarchy();
		}

		public void SetSelected(bool selected)
		{
			if (selected)
				AddToClassList(selectedClassName);
			else
				RemoveFromClassList(selectedClassName);
		}
	}
}
