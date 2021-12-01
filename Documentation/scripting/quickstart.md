# Getting started with FabGeo scripting

FabGeo supports modding with **Lua scripting**. Script files with the `.lua` extension in the ``Documents/FabGeo/Scripts/` folder are loaded after the application has started.

FabGeo is using a custom fork of [MoonSharp](https://www.moonsharp.org/), a Lua unterpreter written in C#. It's syntax is mostly the same to standard Lua with a few exceptions you can check out [here](https://www.moonsharp.org/moonluadifferences.html).

For more information on Lua check out their [documentation](https://www.lua.org/docs.html).

## Execution
Each script is run immediatly after it has been loaded. That means that everything not contained within a function will be executed right away. 

Here is an example:

```
--This function will be registered at startup but won't run until it is called.
function Add(a, b)
    return a + b
end

--This line of code will execute immediatly once the script is loaded.
print("Hello world")
```

## The init function

Define an `init` function to run code when the world has been created. From here you have access to different modules you can use to manipulate the world, add information and much more. 

```
-- The init function will be automatically called after the world has been created
function init()
    print("Initialize this script here!")
end
```

## The update function

Define an `update` function to run code that executes on every frame.

```
-- The update function will be called every frame
-- and print the elapsed time since the last frame to the console
function update()
    print(deltaTime)
end
```