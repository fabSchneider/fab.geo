# Changelog
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
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
- Feature manager with basic support to add features (points) to the world
- Integrated command terminal with reload command to reload the whole world and scripts without the need to restart

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