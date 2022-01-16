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
using MoonSharp.Interpreter.Loaders;
using Fab.Geo.UI;

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

        public IEnumerable<Script> LoadedScripts => loadedScripts;

        public TextAsset[] luaModules;

        private Dictionary<Script, Closure> updateFunctions = new Dictionary<Script, Closure>();

        private LuaDebugger debugger;

        private Dictionary<string, object> globals;
        public IEnumerable<ProxyBase> Proxies => globals.Values.OfType<ProxyBase>();

        public void SetDebugger(LuaDebugger debugger)
        {
            if (debugger == null)
            {
                this.debugger = null;
                return;
            }

            // don't allow to 'overwrite' an existing debugger
            if (this.debugger != null)
                Debug.LogError("Debugger has already been set");

            this.debugger = debugger;

            //attach all loaded scripts to the debugger
            foreach (Script script in loadedScripts)
                debugger.AttachScript(script);
        }

        private void Awake()
        {
            UserData.RegisterAssembly();
            ClrConversion.RegisterConverters();
            Script.GlobalOptions.Platform = new StandardPlatformAccessor();
        }

        private void Start()
        {
            GetGlobals();
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
        /// Returns the name the script is registered with
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public string GetScriptName(Script script)
        {
            if (loadedScripts.Contains(script))
                return (string)script.Globals[scriptNameKey];

            Debug.LogError("The script is not registered");
            return null;
        }

        /// <summary>
        /// Returns the path the script was loaded from
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public string GetScriptLoadPath(Script script)
        {
            return Path.Combine((string)script.Globals[scriptDirKey], (string)script.Globals[scriptNameKey] + ".lua");
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

            foreach (string scriptPath in files)
                LoadScript(scriptPath);

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
            if (debugger != null)
                debugger.DetachScript(script);

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


        private void LoadScript(string path)
        {
            string scriptName = Path.GetFileNameWithoutExtension(path);
            Script script = CreateScript(scriptName);

            script.Options.DebugPrint = s => Debug.Log(s);

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

        /// <summary>
        /// Creates a script with the given script name
        /// </summary>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        public Script CreateScript(string scriptName)
        {
            Script script = new Script(CoreModules.Preset_SoftSandbox | CoreModules.LoadMethods);

            //set script loader
            UnityAssetsScriptLoader scriptLoader = new UnityAssetsScriptLoader();

            //we need to set the module path to '?' for it tor load the resource correctly
            scriptLoader.ModulePaths = new string[] { "?" };
            script.Options.ScriptLoader = scriptLoader;

            //set globals
            foreach (var global in globals)
                script.Globals[global.Key] = global.Value;

            script.Options.DebugPrint = s => Debug.Log(s);

            //add constants
            script.Globals[scriptNameKey] = Path.GetFileNameWithoutExtension(scriptName);
            script.Globals[scriptDirKey] = ScriptsDirectory + Path.DirectorySeparatorChar;
            script.Globals[dataDirKey] = DataDirectory + Path.DirectorySeparatorChar;

            //attach to debugger
            if (debugger != null)
                debugger.AttachScript(script);

            LoadLuaModulesForScript(script);

            return script;
        }

        private void GetGlobals()
        {
            globals = new Dictionary<string, object>();

            //foreach (var t in UserData.GetRegisteredTypes())
            //{
            //    Debug.Log(t.Name);
            //}

            FeatureManager featureManager = FindObjectOfType<FeatureManager>();
            if (featureManager)
            {
                FeatureManagerProxy proxy = new FeatureManagerProxy(featureManager);
                globals.Add(proxy.Name, proxy);
            }

            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager)
            {
                PopupProxy popupProxy = new PopupProxy(uiManager.Popup);
                globals.Add(popupProxy.Name, popupProxy);
                ControlPanelProxy controlProxy = new ControlPanelProxy(uiManager.ControlPanel);
                globals.Add(controlProxy.Name, controlProxy);
            }

            WorldCameraController cameraController = FindObjectOfType<WorldCameraController>();
            if (cameraController)
            {
                WorldCameraControllerProxy proxy = new WorldCameraControllerProxy(cameraController);
                globals.Add(proxy.Name, proxy);
            }

            GeoProxy geoProxy = new GeoProxy();
            globals.Add(geoProxy.Name, geoProxy);

            UserData.RegisterProxyType<TextureProxy, Texture2D>(t => new TextureProxy(t));

            IOProxy ioProxy = new IOProxy(DataDirectory);
            globals.Add(ioProxy.Name, ioProxy);

            DateTime now = DateTime.UtcNow;
            uint seed =
                (uint)now.Year * (uint)31557600 +
                (uint)now.Month * (uint)2629800 +
                (uint)now.Day * (uint)86400 +
                (uint)now.Hour * (uint)3600 +
                (uint)now.Minute * (uint)60 +
                (uint)now.Second;

            RandomProxy randProxy = new RandomProxy(seed);
            globals.Add(randProxy.Name, randProxy);

            WorldInputHandler inputHandler = FindObjectOfType<WorldInputHandler>();
            if (inputHandler)
            {
                RecordProxy recordProxy = new RecordProxy(inputHandler);
                globals.Add(recordProxy.Name, recordProxy);

                WorldProxy worldProxy = new WorldProxy(inputHandler);
                globals.Add(worldProxy.Name, worldProxy);
            }
        }

        private void LoadLuaModulesForScript(Script script)
        {
            //load all modules
            for (int i = 0; i < luaModules.Length; i++)
            {
                script.DoString($"require \'{luaModules[i].name}\'");
            }
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

        #endregion
    }
}
