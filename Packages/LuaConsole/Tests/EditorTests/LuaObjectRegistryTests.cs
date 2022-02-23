using System;
using Fab.Lua.Core;
using NUnit.Framework;
public class LuaObjectRegistryTests 
{
    [Test]
    public void Registering_assembly_twice_throws()
    {
        LuaObjectRegistry registry = new LuaObjectRegistry();
        registry.RegisterAssembly();

        TestDelegate test = new TestDelegate(() => registry.RegisterAssembly());

        Assert.Throws<InvalidOperationException>(test);
    }

}
