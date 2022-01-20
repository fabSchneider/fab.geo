using CommandTerminal;
using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [RequireComponent(typeof(LuaManager))]
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
            manager.SetDebugger(this);
            debuggerServer.Start();         
        }

        private void OnDisable()
        {
            debuggerServer = null;
            manager.SetDebugger(null);
            Debug.Log("Debug server has been stopped");
        }

        /// <summary>
        /// Attaches a script to the debugger
        /// </summary>
        /// <param name="script"></param>
        public void AttachScript(Script script)
        {
            debuggerServer.AttachToScript(script, manager.GetScriptName(script), s => manager.GetScriptLoadPath(s.OwnerScript));
        }

        /// <summary>
        /// Detaches a script from the debugger
        /// </summary>
        /// <param name="script"></param>
        public void DetachScript(Script script)
        {
            debuggerServer.Detach(script);
        }

        [RegisterCommand(command_name: "debug_scripts", Help = "Enables/Disables the lua script debugger", MinArgCount = 1, MaxArgCount = 1)]
        private static void Command_Debug_Scripts(CommandArg[] args)
        {
            bool toggle = args[0].Bool;

            if (Terminal.IssuedError) return;

            LuaDebugger debugger = FindObjectOfType<LuaDebugger>(true);

            if (debugger)
                debugger.enabled = toggle;
        }
    }
}
