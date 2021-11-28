using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Platforms;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Fab.Geo.Modding
{
    public class LuaManager : MonoBehaviour
    {
        private static string DocumentsDirectory => Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), Application.productName);
        private static string ScriptsDirectory => Path.Combine(DocumentsDirectory, "Scripts");
        private static string DataDirectory => Path.Combine(DocumentsDirectory, "Data");

        private static readonly string LuaFileSearchPattern = "*.lua";

        private static readonly string initFuncKey = "init";
        private static readonly string updateFuncKey = "update";
        private static readonly string deltaTimeKey = "deltaTime";

        private List<Closure> updateFunctions;

        private void Start()
        {
            UserData.RegisterAssembly();
            Script.GlobalOptions.Platform = new StandardPlatformAccessor();

            LoadScripts();
        }

        [Button("Reload Scripts")]
        private void LoadScripts()
        {
            Dictionary<string, object> globals = GetGlobals();
            List<Script> scripts = LoadScriptsFromScriptDir(globals);

            foreach (Script script in scripts)
            {

            }
        }

        private void Update()
        {
            if (updateFunctions != null)
                foreach (var func in updateFunctions)
                {
                    func.OwnerScript.Globals[deltaTimeKey] = Time.deltaTime;
                    func.Call();
                }

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
                globals.Add("ui", new UIManagerProxy(uiManager));
            }

            return globals;
        }

        private List<Script> LoadScriptsFromScriptDir(Dictionary<string, object> globals)
        {
            string scriptsDir = ScriptsDirectory;
            string dataDir = DataDirectory;
            Directory.CreateDirectory(scriptsDir);
            Directory.CreateDirectory(dataDir);
            Debug.Log("Loading scripts from " + scriptsDir);
            string[] files = Directory.GetFiles(scriptsDir, LuaFileSearchPattern, SearchOption.AllDirectories);

            List<Script> scripts = new List<Script>(files.Length);

            updateFunctions = new List<Closure>();
            foreach (string file in files)
            {
                Script script = new Script();

                //set globals
                foreach (var global in globals)
                    script.Globals[global.Key] = global.Value;

                script.Options.DebugPrint = s => Debug.Log(s);

                //add constants
                script.Globals["SCRIPT_NAME"] = Path.GetFileNameWithoutExtension(file);
                script.Globals["SCRIPT_DIR"] = scriptsDir + +Path.DirectorySeparatorChar;
                script.Globals["DATA_DIR"] = dataDir + Path.DirectorySeparatorChar;

                using Stream fileStream = new FileStream(file, FileMode.Open);
                script.DoStream(fileStream);

                //call init function
                Closure initFunc = script.Globals.Get(initFuncKey).Function;
                if (initFunc != null)
                    initFunc.Call();

                //add update function
                Closure updateFunc = script.Globals.Get(updateFuncKey).Function;
                if (updateFunc != null)
                    updateFunctions.Add(updateFunc);

                scripts.Add(script);
            }
            Debug.Log($"Loaded {scripts.Count} scripts");

            return scripts;
        }
    }
}
