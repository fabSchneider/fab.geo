using Fab.Lua.Core;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Lua.Interop
{
	internal static class RegisterAssembly
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void Register()
		{
			UserData.RegisterAssembly();
			LuaEnvironment.Registry.RegisterAssembly();
			ClrConversion.RegisterConverters();
		}
	}
}
