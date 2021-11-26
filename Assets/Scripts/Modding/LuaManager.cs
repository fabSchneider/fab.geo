using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Platforms;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Fab.Geo.Modding
{
    public class LuaManager : MonoBehaviour
    {
        private static string ScriptFolderPath =>
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), Application.productName, "Scripts");

        private static readonly string LuaFileSearchPattern = "*.lua";

        private static readonly string initFuncKey = "init";

        private List<Script> scripts;

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
            string scriptDir = ScriptFolderPath;
            globals.Add("workingDir", scriptDir + Path.DirectorySeparatorChar);
            scripts = LoadScriptsFromDisk(scriptDir, globals);

            foreach (Script script in scripts)
            {
                Closure initFunc = script.Globals.Get(initFuncKey).Function;
                if (initFunc != null)
                    script.Call(initFunc);
            }
        }

        private static Dictionary<string, object> GetGlobals()
        {
            Dictionary<string, object> globals = new Dictionary<string, object>();
            FeatureManager featureManager = FindObjectOfType<FeatureManager>();
            if (featureManager)
            {
                globals.Add("features", new FeatureManagerProxy(featureManager));
            }
            return globals;
        }

        private static List<Script> LoadScriptsFromDisk(string scriptsDir, Dictionary<string, object> globals)
        {
            Directory.CreateDirectory(scriptsDir);
            Debug.Log("Loading scripts from " + scriptsDir);
            string[] files = Directory.GetFiles(scriptsDir, LuaFileSearchPattern, SearchOption.AllDirectories);

            List<Script> scripts = new List<Script>(files.Length);
            foreach (string file in files)
            {
                using Stream fileStream = new FileStream(file, FileMode.Open);
                Script script = CreateScript(globals);
                script.DoStream(fileStream);
                scripts.Add(script);
            }
            Debug.Log($"Loaded {scripts.Count} scripts");

            return scripts;
        }

        private static Script CreateScript(Dictionary<string, object> globals)
        {
            Script script = new Script();
            foreach (var global in globals)
                script.Globals[global.Key] = global.Value;
            script.Options.DebugPrint = s => Debug.Log(s);
            return script;
        }
    }
}
