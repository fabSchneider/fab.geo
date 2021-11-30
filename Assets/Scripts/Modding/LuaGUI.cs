using CommandTerminal;
using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [RequireComponent(typeof(LuaManager))]
    public class LuaGUI : MonoBehaviour
    {
        public Rect windowRect = new Rect(20, 20, 200, 300);

        private Vector2 scrollPos;

        private LuaManager manager;

        private List<Script> unloadScripts = new List<Script>();

        private void Start()
        {
            manager = GetComponent<LuaManager>();
        }

        void OnGUI()
        {
            Matrix4x4 originalXForm = GUI.matrix;
            GUI.matrix = Matrix4x4.identity * Matrix4x4.Scale(Vector3.one * 2);

            // Register the window. We create two windows that use the same function
            // Notice that their IDs differ
            windowRect = GUI.Window(0, windowRect, OnWindowGUI, "Lua Script Manager");

            GUI.matrix = originalXForm;
        }

        // Make the contents of the window
        void OnWindowGUI(int windowID)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            unloadScripts.Clear();
            foreach (var script in manager.LoadedScripts)
            {
                if (!GUILayout.Toggle(true, manager.GetScriptName(script)))
                    unloadScripts.Add(script);
            }

            foreach (var script in unloadScripts)
                manager.UnloadScript(script);

            GUILayout.EndScrollView();

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("close"))
                enabled = false;
            GUILayout.EndVertical();

            // Make the windows be draggable.
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        [RegisterCommand(command_name: "open_scripts_manager", Help = "Enables/Disables the lua script debugger")]
        private static void Command_Open_Scripts_Manager(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            LuaGUI gui = FindObjectOfType<LuaGUI>(true);
            gui.enabled = true;
        }
    }
}
