# Example 1 - Adding Features to the world

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