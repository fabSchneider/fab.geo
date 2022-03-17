using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Lua.Console
{
	[RequireComponent(typeof(UIDocument))]
	[AddComponentMenu("FabGeo/Lua/Lua Console")]
	public class ConsoleComponent : MonoBehaviour
	{
		[Tooltip("Maximum number of items in the history.")]
		[SerializeField]
		private int maxHistoryEntries = 24;

		[SerializeField]
		[Tooltip("Maximum character length after which a print output will be cut.")]
		private int maxPrintOutputLength = 1024;

		private Console console;
		public Console Console => console;

		private History history;
		public History History => history;

		private ConsoleHelp help;

		private void Awake()
		{
			history = new History(maxHistoryEntries, maxPrintOutputLength);
			console = new Console(name, new ConsoleScriptFactory(), history);
			help = new ConsoleHelp(new ConsoleHelpFormatter());
		}

		private void Start()
		{
			console.Initialize();
			console.RegisterCommand(help);
		}

		public void ResetConsole()
		{
			console.Reset();
			console.RegisterCommand(help);
		}
	}
}
