using Fab.Lua.Core;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Geo.UI
{
	[RequireComponent(typeof(UIDocument))]
	public class ReloadUI : MonoBehaviour
	{
		private UIDocument document;

		void Start()
		{
			document = GetComponent<UIDocument>();
			document.rootVisualElement.Q<Button>(name: "reload-btn").clicked += App.Reload;


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            document.rootVisualElement.Q<Button>(name: "open-scripts-btn").clicked += () => OpenExplorer(LuaEnvironment.ScriptsDirectory);
#else
			document.rootVisualElement.Q<Button>(name: "open-scripts-btn").RemoveFromHierarchy();
#endif
		}

		private void OpenExplorer(string directory)
		{
			if (!Directory.Exists(directory))
				return;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

#endif

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				Arguments = directory,
				FileName = "explorer.exe"
			};

			Process.Start(startInfo);

		}
	}
}
