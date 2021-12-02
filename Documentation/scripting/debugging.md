# Debugging with VS Code

## 1. Setup VS Code editor

Install the [MoonSharp Debug Extension](https://marketplace.visualstudio.com/items?itemName=xanathar.moonsharp-debug) for VS Code.

Add this configuration to the launch.json file in `Scripts/.vscode/`
```
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "MoonSharp Attach",
            "type": "moonsharp-debug",
            "request": "attach",
            "debugServer": 41912,
        }
    ]
}
```

After these steps you can start debugging by pressing `F5` or by selecting Run -> Start Debugging.
You can set breakpoints inside your Lua code and inspect variables while the app is running.


## 2. Enable script debugging inside the app

Execute `debug_scripts true` in the command line to enable the script debugger.

Execute `reload` to refresh all scripts. 

