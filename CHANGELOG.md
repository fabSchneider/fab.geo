# Changelog
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
## [0.4.0] - 2022-01-17
### Added
- Lua: Interactive lua console with history and preview of variable data
- Lua: Texture objects
- Lua: Vector, Coordinate and Color module
- Lua: Show/hide methods for the control panel
- Lua: help and list methods for lua console
- Lua: Random module
- Lua: Record module
- Lua: World module
- Feature get method
- Feature line
- Feature color properties
- Feature polyline
- World atmosphere and better graphics
- Camera control can be enabled/disabled
- Jump Flooding Distance compute shader
- Nil checks for proxy objects
- World clicks can now be add listeners to
- Reload button to reload the whole scene and all scripts
- Various new controls for the control panel
- Camera animate method

### Changed
- Font to Cascadia Nerd Font
- Terminal hotkey to `Ctrl + T`
- Controls are now added to the control panel using a path string

### Fixed
- Disabled buttons still applying hover effect
- Fix feature point setting name not setting text


## [0.3.0] - 2021-12-01
### Added
- Lua: update function
- Lua: geo module
- Lua: texture module
- Lua: loader module
- Lua: camera module
- Lua: controls module
- Lua: ui module
- Lua: vscode debugger support
- Control panel ui with the ability to add a range of controls to it during runtime
- Camera orbit control

### Changed
 - Camera controller's `SetPosition(...)` method has been renamed to `SetCoord(...)`

## [0.2.0] - 2021-11-27
### Added
- Lua scripting support
- Feature manager with basic support to add features (points) to the world
- Integrated command terminal with reload command to reload the whole world and scripts without the need to restart

## [0.1.0] - 2021-11-24
### Added
- World mesh generation using burst and untiy's job system
- Basic LOD system for world chunks
- World camera controller to pan around the globe and zoom in and out
- Compute shaders to generate city density and distance field textures
- Basic world shader with ocean and height offset

[Unreleased]: https://github.com/fabSchneider/fab.geo/compare/v0.3.0...HEAD
[0.3.0]: https://github.com/fabSchneider/fab.geo/releases/tag/v0.3.0
[0.2.0]: https://github.com/fabSchneider/fab.geo/releases/tag/v0.2.0
[0.1.0]: https://github.com/fabSchneider/fab.geo/releases/tag/v0.1.0