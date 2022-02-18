using Fab.Geo.Lua.Core;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Lua.Interop
{
	[LuaName("rec")]
	[LuaHelpInfo("Module to record coordinates from mouse inputs")]
	public class Record : LuaObject, ILuaObjectInitialize
	{
		private WorldInputHandler worldInput;

		private Table recordTable;

		public void Initialize()
		{
			worldInput = Object.FindObjectOfType<WorldInputHandler>();

			if (worldInput == null)
				throw new LuaObjectInitializationException("Could not find world input");
		}

		[LuaHelpInfo("Starts recording clicks on the globe and appends the coordinate at the click position to the supplied table")]
		public void to(Table to)
		{
			recordTable = to;
			worldInput.clicked += WorldInput_OnClick;
		}

		[LuaHelpInfo("Stops recording clicks")]
		public void stop()
		{
			recordTable = null;
			worldInput.clicked -= WorldInput_OnClick;
		}

		private void WorldInput_OnClick(Coordinate coord)
		{
			Debug.Log("Recorded " + coord);
			recordTable.Append(DynValue.FromObject(recordTable.OwnerScript, coord));
		}
	}
}
