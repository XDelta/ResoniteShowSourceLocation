# ShowSourceLocation
A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) that allows seeing where a field is being driven from and where a component is located.

## Installation
1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Place [ShowSourceLocation.dll](https://github.com/XDelta/ResoniteShowSourceLocation/releases/latest/download/ShowSourceLocation.dll) into your `rml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a default install. You can create it if it's missing, or if you launch the game once with ResoniteModLoader installed it will create the folder for you.
3. Start the game. If you want to verify that the mod is working you can check your Resonite logs.

## Usage
Click on the name of any driven field and a worker inspector on the driving source should open.
On the worker inspector, a button is also added on the top bar to open an inspector to the slot where the component is located

## Config Options

| Config Option     | Default | Description |
| ------------------ | ------- | ----------- |
| `enabled` | `true` | Should the mod be enabled |
| `showTextInWorld` | ShowTextInWorld | Show floating text in world |
