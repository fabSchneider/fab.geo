using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NaughtyAttributes;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Platforms;
using MoonSharp.VsCodeDebugger;
using CommandTerminal;
using System.Linq;
using MoonSharp.Interpreter.Debugging;

namespace Fab.Geo.Modding
{
    public class LuaManager : MonoBehaviour
    {
        private static string DocumentsDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Application.productName);
        private static string ScriptsDirectory => Path.Combine(DocumentsDirectory, "Scripts");
        private static string DataDirectory => Path.Combine(DocumentsDirectory, "Data");

        private static readonly string LuaFileSearchPattern = "*.lua";

        private static readonly string scriptNameKey = "SCRIPT_NAME";
        private static readonly string scriptDirKey = "SCRIPT_DIR";
        private static readonly string dataDirKey = "DATA_DIR";

        private static readonly string initFuncKey = "init";
        private static readonly string updateFuncKey = "update";
        private static readonly string deltaTimeKey = "deltaTime";

        private List<Script> loadedScripts = new List<Script>();
        private Dictionary<Script, Closure> updateFunctions = new Dictionary<Script, Closure>();

        private MoonSharpVsCodeDebugServer debuggerServer;

        private void Start()
        {
            UserData.RegisterAssembly();
            Script.GlobalOptions.Platform = new StandardPlatformAccessor();

            LoadScripts();
        }

        private void Update()
        {
            foreach (var func in updateFunctions.Values)
            {
                func.OwnerScript.Globals[deltaTimeKey] = Time.deltaTime;
                func.Call();
            }
        }

        /// <summary>
        /// (Re)loads all scripts found in the scripts directory
        /// </summary>
        [Button("Reload Scripts")]
        public void LoadScripts()
        {
            string scriptsDir = ScriptsDirectory;
            string dataDir = DataDirectory;
            Directory.CreateDirectory(scriptsDir);
            Directory.CreateDirectory(dataDir);

            Debug.Log("Loading scripts from " + scriptsDir);
            string[] files = Directory.GetFiles(scriptsDir, LuaFileSearchPattern, SearchOption.AllDirectories);

            UnloadAllScripts();

            Dictionary<string, object> globals = GetGlobals();

            foreach (string scriptPath in files)
                LoadScript(scriptPath, globals);

            Debug.Log($"Loaded {loadedScripts.Count} scripts");
        }

        /// <summary>
        /// Unloads all loaded scripts
        /// </summary>
        public void UnloadAllScripts()
        {
            for (int i = loadedScripts.Count - 1; i >= 0; i--)
                UnloadScript(loadedScripts[i]);
        }

        /// <summary>
        /// Unloads the specified script
        /// </summary>
        /// <param name="script"></param>
        public void UnloadScript(Script script)
        {
            if (debuggerServer != null)
                debuggerServer.Detach(script);

            loadedScripts.Remove(script);
            updateFunctions.Remove(script);
        }

        /// <summary>
        /// Lists all loaded scripts
        /// </summary>
        /// <returns></returns>
        public string[] GetAllLoadedScripts()
        {
            string[] list = new string[loadedScripts.Count];
            for (int i = 0; i < loadedScripts.Count; i++)
                list[i] = loadedScripts[i].Globals[scriptNameKey].ToString();

            return list;
        }


        private void LoadScript(string path, Dictionary<string, object> globals)
        {
            Script script = new Script();

            //set globals
            foreach (var global in globals)
                script.Globals[global.Key] = global.Value;

            script.Options.DebugPrint = s => Debug.Log(s);

            string scriptName = Path.GetFileNameWithoutExtension(path);

            //add constants
            script.Globals[scriptNameKey] = Path.GetFileNameWithoutExtension(path);
            script.Globals[scriptDirKey] = ScriptsDirectory + Path.DirectorySeparatorChar;
            script.Globals[dataDirKey] = DataDirectory + Path.DirectorySeparatorChar;

            ////attach to debugger
            //if (debuggerServer != null)
            //    debuggerServer.AttachToScript(script, scriptName);

            using Stream fileStream = new FileStream(path, FileMode.Open);

            //execute script
            script.DoStream(fileStream);

            //add update function
            Closure updateFunc = script.Globals.Get(updateFuncKey).Function;
            if (updateFunc != null)
                updateFunctions.Add(script, updateFunc);

            //add script to loaded script list
            loadedScripts.Add(script);

            //call init function
            Closure initFunc = script.Globals.Get(initFuncKey).Function;
            if (initFunc != null)
                initFunc.Call();
        }

        private static Dictionary<string, object> GetGlobals()
        {
            Dictionary<string, object> globals = new Dictionary<string, object>();

            FeatureManager featureManager = FindObjectOfType<FeatureManager>();
            if (featureManager)
                globals.Add("features", new FeatureManagerProxy(featureManager));

            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager)
            {
                globals.Add("popup", new PopupProxy(uiManager.Popup));
                globals.Add("controls", new ControlPanelProxy(uiManager.ControlPanel));
            }

            WorldCameraController cameraController = FindObjectOfType<WorldCameraController>();
            if (cameraController)
            {
                globals.Add("camera", new WorldCameraControllerProxy(cameraController));
            }

            globals.Add("geo", new GeoProxy());

            UserData.RegisterProxyType<TextureProxy, Texture2D>(t => new TextureProxy(t));

            globals.Add("loader", new IOProxy(DataDirectory));

            return globals;
        }

        #region Commands

        [RegisterCommand(command_name: "list_scripts", Help = "Lists all loaded lua scripts")]
        private static void Command_List_Scripts(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            LuaManager manager = FindObjectOfType<LuaManager>();
            if (manager)
                foreach (string script in manager.GetAllLoadedScripts())
                    Terminal.Log(TerminalLogType.ShellMessage, script);
        }

        [RegisterCommand(command_name: "unload_scripts", Help = "Unloads all lua scripts")]
        private static void Command_Unload_Scripts(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            LuaManager manager = FindObjectOfType<LuaManager>();
            if (manager)
                manager.UnloadAllScripts();
        }

        [RegisterCommand(command_name: "unload_script", Help = "Unloads a lua scripts", MaxArgCount = 1, MinArgCount = 1)]
        private static void Command_Unload_Script(CommandArg[] args)
        {
            string scriptName = args[0].String;

            if (Terminal.IssuedError) return;

            LuaManager manager = FindObjectOfType<LuaManager>();
            if (manager)
            {
                Script script = manager.loadedScripts.FirstOrDefault(s => (string)s.Globals[scriptNameKey] == scriptName);
                if (script != null)
                    manager.UnloadScript(script);
            }
        }

        [RegisterCommand(command_name: "debug_scripts", Help = "Enables the lua script debugger")]
        private static void Command_Debug_Scripts(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            LuaManager manager = FindObjectOfType<LuaManager>();
            if (manager)
            {
                if (manager.debuggerServer == null)
                    manager.debuggerServer = new MoonSharpVsCodeDebugServer(41912);
                else
                {
                    Terminal.Log(TerminalLogType.Error, "Lua Debugger is already enabled");
                    return;
                }

                foreach (Script script in manager.loadedScripts)
                {
                    string scriptName = (string)script.Globals[scriptNameKey];
                    manager.debuggerServer.AttachToScript(script, scriptName, FindDebuggerSource);
                }

                manager.debuggerServer.Start();
                Debug.Log("Lua debug server started on port 41912");
            }
        }

        private static string FindDebuggerSource(SourceCode s)
        {
            string path = Path.Combine((string)s.OwnerScript.Globals[scriptDirKey], (string)s.OwnerScript.Globals[scriptNameKey] + ".lua");
            return path;
        }

        #endregion
    }
}
