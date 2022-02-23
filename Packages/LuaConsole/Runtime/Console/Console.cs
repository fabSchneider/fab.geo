using System;
using Fab.Lua.Core;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Lua.Console
{
	/// <summary>
	/// Class managing a lua script for live coding
	/// </summary>
	public class Console
	{
		private Script script;

		private History history;
		public int MaxHistoryEntries { get; set; } = 24;
		public int MaxPrintOutputLength { get; set; } = 1024;

		public Console(string name, IScriptFactory scriptFactory, History history)
		{
			script = scriptFactory.CreateScript(name);
			this.history = history;

			if (history == null)
				script.Options.DebugPrint = print => Debug.Log(print);
			else
				script.Options.DebugPrint = print => history.AddText(print);
		}

		/// <summary>
		/// Registers a method that can be executed by typing the supplied name into the console
		/// </summary>
		/// <param name="name"></param>
		/// <param name="action"></param>
		public void RegisterMethod(string name, Action action)
		{
			if (script.Globals.Get(name) != DynValue.Nil)
				throw new InvalidOperationException($"Cannot register method with the name \"{name}\" as that name is already taken");
			script.Globals[name] = action;
		}

		/// <summary>
		/// Registers a method that can be executed by typing the supplied name into the console
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="action"></param>
		public void RegisterMethod<T>(string name, Action<T> action)
		{
			if (script.Globals.Get(name) != DynValue.Nil)
				throw new InvalidOperationException($"Cannot register method with the name \"{name}\" as that name is already taken");

			script.Globals[name] = action;
		}

		public EvaluateResult Evaluate(string code)
		{
			try
			{
				DynValue returnVal = script.DoString(code);
				if (returnVal.IsNotNil())
				{
					if (history != null)
						history.AddText(returnVal.ToPrintString());
					else
						Debug.Log(returnVal.ToPrintString());
				}
			}
			catch (Exception e)
			{
				//try getting a variable from the code
				DynValue val = script.Globals.RawGet(code);

				if (val != null)
				{
					if (history != null)
					{
						try
						{
							var texProxy = val.ToObject<LuaProxy<Texture2D>>();
							if (texProxy != null)
								history.AddImage(texProxy.Target);
						}
						catch (Exception) { }
					}

					script.DoString($"print({code})");
				}
				else
				{
					return new EvaluateResult() { success = false, errorMsg = e.Message };
				}
			}

			if(history != null)
				history.AddHistoryEntry(code);
			return new EvaluateResult() { success = true };
		}

		public void Print(string print)
		{
			if (history == null)
				Debug.Log(print);
			else
				history.AddText(print);
		}

		public struct EvaluateResult
		{
			public bool success;
			public string errorMsg;
		}
	}
}
