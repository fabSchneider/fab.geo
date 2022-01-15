using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    // NOTE: Not really a proxy as it does not stand in for anything. Should maybe rename later
    [MoonSharpUserData]
    [LuaHelpInfo("Module to record coordinates from mouse inpute")]
    public class RecordProxy : ProxyBase
    {
        public override string Name => "record";

        WorldInputHandler worldInput;

        Table recordTable;

        [MoonSharpHidden]
        public RecordProxy(WorldInputHandler worldInput)
        {
            this.worldInput = worldInput;
        }


        [LuaHelpInfo("Starts recording clicks on the globe and appends the coordinate at the click position to the supplied table")]
        public void start(Table to)
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
