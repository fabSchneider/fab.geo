# Lua Modules

## camera
Module for controlling the world camera

### Methods:
####  set_zoom(zoom) 
Sets the camera's zoom level [0-1]

####  enable_control() 
Enables the camera's input control

####  disable_control() 
Disables the camera's input control

####  animate(coords speed loop) 
Moves the camera from one coordinate to the next in a list of coordinates

####  on_animation_finished(evt) 
Called when a camera animation finished

### Properties:
####  coord 
Gets/Sets the camera's position in coordinates

## controls
Module for adding controls to the control panel

### Methods:
####  show() 
Shows the control panel

####  hide() 
Hides the control panel

####  get(path) 
Gets the control at the given path from the panel

####  remove(control) 
Removes the control at the given path from the panel

####  remove_all() 
Removes all controls from the panel

####  label(path text) 
Adds a label to the control panel

####  separator(path) 
Adds a separator to the control panel

####  slider(path min max value) 
Adds a slider to the control panel

####  range(path min max minLimit maxLimit) 
Adds a ranged slider to the control panel.

####  choice(path choices value) 
Adds a choice field to the control panel

####  button(path text on_click) 
Adds a button to the control panel. You can pass in a function that will be called when the button was pressed

## features
Module for adding features (points, lines...) to the world

### Methods:
####  point(name coord) 
Adds a point at the given coordinates

####  line(name coord1 coord2) 
Adds a line between two coordinates

####  line(name feature_1 feature_2) 
Adds a line between two features

####  polyline(name coords closed) 
Adds a polyline through a list of coordinates

####  get(name) 
Gets the first feature with the given name

####  get_all(name) 
Gets all feature with the given name

####  remove(feature) 
Removes a feature from the world

####  remove_all() 
Removes all features from the world

## geo
Module for geo operations

### Methods:
####  distance(coord1 coord2) 
Calculates the distance in kilometer between two coordinates

## io
Module for loading image and text

### Methods:
####  load(file) 
Loads a text(txt, json, geojson) or image file(jpg, png) from the data path

### Properties:
####  data_dir 
Returns the directory path that data can be loaded from (read only)

## popup
Module to show a popup on the screen

### Methods:
####  show(title text) 
Shows a popup with some text

####  show(title image) 
Shows a popup with an image

####  close() 
Closes any open popup

####  button(text on_click) 
Adds a button to the popup

## random
Module for generating random numbers and more

### Methods:
####  set_seed(seed) 
Sets the seed of the random generator

####  number() 
returns a random number between 0 [inclusive] and 1 (exclusive)

####  number(min max) 
returns a random number between min [inclusive] and max (exclusive)

####  whole_number(min max) 
returns a random whole number between min [inclusive] and max (exclusive)

####  coord() 
returns a random coordinate

####  color(saturation brightness) 
returns a color with a random hue

## rec
Module to record coordinates from mouse inputs

### Methods:
####  to(to) 
Starts recording clicks on the globe and appends the coordinate at the click position to the supplied table

####  stop() 
Stops recording clicks

## world
Module for interacting with the world

### Methods:
####  on_click(action) 
Event function that is called when the world is clicked

