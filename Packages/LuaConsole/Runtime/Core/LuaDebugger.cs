using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;
using UnityEngine;

namespace Fab.Lua.Core
{
	[AddComponentMenu("FabGeo/Lua/Lua Debugger")]
	public class LuaDebugger : MonoBehaviour
	{
		private LuaManager manager;

		[SerializeField]
		private int port = 41912;

		private MoonSharpVsCodeDebugServer debuggerServer;

		private void OnEnable()
		{
			Debug.Log("Starting debug server on port " + port);

			debuggerServer = new MoonSharpVsCodeDebugServer(port);
			manager = GetComponent<LuaManager>();

			// attach already loaded scripts
			foreach (var script in manager.LoadedScripts)
				AttachScript(script);

			// add listeners to attach newly loaded and detach unloaded scripts 
			manager.AfterScriptLoaded += AttachScript;
			manager.BeforeScriptUnloaded += DetachScript;

			debuggerServer.Start();
		}

		private void OnDisable()
		{
			debuggerServer?.Dispose();
			debuggerServer = null;

			manager.AfterScriptLoaded -= AttachScript;
			manager.BeforeScriptUnloaded -= DetachScript;

			Debug.Log("Debug server has been stopped");
		}


		/// <summary>
		/// Attaches a script to the debugger
		/// </summary>
		/// <param name="script"></param>
		public void AttachScript(Script script)
		{
			Debug.Log("Attaching debugger to " + LuaEnvironment.GetScriptName(script));
			debuggerServer.AttachToScript(script, LuaEnvironment.GetScriptName(script), s => LuaEnvironment.GetScriptLoadPath(s.OwnerScript));
		}

		/// <summary>
		/// Detaches a script from the debugger
		/// </summary>
		/// <param name="script"></param>
		public void DetachScript(Script script)
		{
			Debug.Log("Detaching debugger from " + LuaEnvironment.GetScriptName(script));
			debuggerServer.Detach(script);
		}
	}
}
