# Custom Scripting

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

## Initialization

Define an `init` function to run code when the world has been created. From here you have access to different modules you can use to manipulate the world, add information and much more. 

```
--The init function will be automatically called after the world has been created
function init()
    print("Initialize this script here!")
end
```

## Example

In this example we are adding features to the world based on a custom dataset. First we are loading a `.geojson` file using the [io module](https://www.tutorialspoint.com/lua/lua_file_io.htm). We are then picking out the relevant data and use that data to add point features to the world using the `feature` class inside the `init` function. Last we are adding a click listener to each feature to print the name of the feature to the console. 

```
--We run the code in the init function 
--This ensures that all the neccessary modules have already been intialized.
function init()

    --workingDir is a global variable that holds the folder path of this script file. 
    --We have placed the capitals.geojson file alongside this script
    --and are loading it in with the help of the io module
    file = io.open(workingDir .. "capitals.geojson")
    
    content = file:read( "*all" )
    
    --once we have read the content of the file we should close it again
    file:close()
    
    --we use the json module to parse the content to a dataTable  
    data = json.parse(content)

    --we then pick out the relevant data and iterate through each feature
    capitals = data["features"];

    for _, c in next, capitals do
        name = c["properties"]["city"]
        if name != nil and name != '' then
            coord = c["geometry"]["coordinates"]

            --we use the feature class to add a point to the world
            pt = features.addPoint(name, coord[2], coord[1])

            --we also add a click listener 
            --which will execute the on click method every time it is clicked
            pt.addClickListener(onClick)
        end
    end
end

function onClick(pt)   
    print(pt.name)
end
```

This is the content of the example `capitals.geojson` file containing three cities with coordinates:
```
{
    "type": "FeatureCollection",
    "features": [{
        "properties": {
            "country": "Germany",
            "city": "Berlin",
            "tld": "de",
            "iso3": "DEU",
            "iso2": "DE"
        },
        "geometry": {
            "coordinates": [13.24, 52.31],
            "type": "Point"
        },
        "id": "DE"
    }, {
        "properties": {
            "country": "Yemen",
            "city": "Sanaa",
            "tld": "ye",
            "iso3": "YEM",
            "iso2": "YE"
        },
        "geometry": {
            "coordinates": [44.12, 15.21],
            "type": "Point"
        },
        "id": "YE"
    }, {
        "properties": {
            "country": "Algeria",
            "city": "Algiers",
            "tld": "dz",
            "iso3": "DZA",
            "iso2": "DZ"
        },
        "geometry": {
            "coordinates": [3.03, 36.45],
            "type": "Point"
        },
        "id": "DZ"
    }]
}
```