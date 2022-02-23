using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Fab.Geo.Lua.Core;

namespace Fab.Geo.Lua.Console
{
	[RequireComponent(typeof(UIDocument))]
	[AddComponentMenu("FabGeo/Lua/Lua Console")]
	public class Console : MonoBehaviour
	{
		private Script script;

		private History history;
		public History ConsoleHistory => history;

		private LuaHelpInfoExtractor luaHelpInfo;
		private LuaHelpConsoleFormatter luaHelpInfoFormatter;

		[SerializeField]
		[Tooltip("Maximum number of items in the history.")]
		private int maxHistoryEntries = 24;

		[SerializeField]
		[Tooltip("Maximum character length after which a print output will be cut.")]
		private int maxPrintOutputLength = 1024;

		public int MaxHistoryEntries => maxHistoryEntries;

		private List<string> printOutput = new List<string>();

		private Texture imageOutput;

		private void Awake()
		{
			history = new History(maxHistoryEntries);
			luaHelpInfo = new LuaHelpInfoExtractor();
			luaHelpInfoFormatter = new LuaHelpConsoleFormatter();
		}

		private void Start()
		{
			script = LuaEnvironment.CreateScript("REPL");
			script.Globals["help"] = (Action<DynValue>)help;
			script.Globals["list"] = (Action)list;
			script.Options.DebugPrint = print => AddToPrintOutput(print);
		}

		private void AddToPrintOutput(string output, bool trim = true)
		{
			if (trim && output.Length > maxPrintOutputLength)
			{
				//trim excess print output and add a message informing about the cut
				string cut = output.Substring(0, maxPrintOutputLength);
				cut += $"\n ... ";
				printOutput.Add(cut);
			}
			else
				printOutput.Add(output);
		}

		private void help(DynValue value)
		{
			switch (value.Type)
			{
				case DataType.Nil:
					AddToPrintOutput("Nil", false);
					break;
				case DataType.Void:
					AddToPrintOutput("Type <b>help(<i>module</i>)</b> to show help information for that module." + Environment.NewLine +
						"Type <b>list()</b> to get a list of all available modules.", false);
					break;
				case DataType.UserData:
					IUserDataDescriptor descriptor = value.UserData.Descriptor;
					if (descriptor is ProxyUserDataDescriptor proxyDescriptor)
						descriptor = proxyDescriptor.InnerDescriptor;

					LuaHelpInfo helpInfo = luaHelpInfo.GetHelpInfoForType((StandardUserDataDescriptor)descriptor);
					string formatted = luaHelpInfoFormatter.Format(helpInfo);
					AddToPrintOutput(formatted, false);

					break;
				case DataType.ClrFunction:
					break;
				default:
					AddToPrintOutput("No help information available", false);
					break;
			}
		}

		[LuaHelpInfo("Lists all available modules")]
		private void list()
		{
			foreach (StandardUserDataDescriptor descriptor in LuaEnvironment.Registry.GetRegisteredTypes(true))
			{
				LuaHelpInfo helpInfo = luaHelpInfo.GetHelpInfoForType(descriptor);
				AddToPrintOutput($"{helpInfo.name.PadRight(8, ' ')} <i>{helpInfo.description}</i>" + Environment.NewLine, false);
			}
		}

		public Result Execute(string code)
		{
			try
			{
				DynValue returnVal = script.DoString(code);
				if (returnVal.IsNotNil())
				{
					AddToPrintOutput(returnVal.ToPrintString());
				}
			}
			catch (Exception e)
			{
				//try getting a variable from the code
				DynValue val = script.Globals.RawGet(code);

				if (val != null)
				{
					try
					{
						var texProxy = val.ToObject<LuaProxy<Texture2D>>();
						if (texProxy != null)
							imageOutput = texProxy.Target;
					}
					catch (Exception) { }

					script.DoString($"print({code})");
				}
				else
				{
					return new Result() { success = false, errorMsg = e.Message };
				}
			}

			history.Add(code, string.Join('\n', printOutput), imageOutput);
			printOutput.Clear();
			imageOutput = null;
			return new Result() { success = true };
		}

		public struct Result
		{
			public bool success;
			public string errorMsg;
		}
	}
}
