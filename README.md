# Server_Ropes
[![GitHub release](https://img.shields.io/github/release/zapk/Server_Ropes.svg)]() [![GitHub issues](https://img.shields.io/github/issues/zapk/Server_Ropes.svg)](https://github.com/zapk/Server_Ropes/issues) [![GitHub downloads](https://img.shields.io/github/downloads/zapk/Server_Ropes/total.svg)]()

Aesthetic ropes for [Blockland](http://blockland.us/).

## Installation
Download Server_Ropes.zip from the [latest release](https://github.com/zapk/Server_Ropes/releases) and put it in Blockland\\Add-Ons.

### Disclaimer

**Do not** straight-up download the repository unless you know what you're doing as it's most likely partly or mostly broken.

## Commands
- **/ropeHelp**
	- lists all rope commands
- **/clearRopes**
	-	clears your own ropes
- **/ropeTool**
	-	equips a Rope Tool
	- aliases: /r, /ro, /rop, /rope, /ropeT, /ropeTo, /ropeToo

## Admin Commands
- **/clearAllRopes**
	- clears all ropes on the server
- **/saveRopes [saveName]**
	- saves all ropes created with the Rope Tool
- **/loadRopes [saveName]**
	- loads ropes from a file; automatically clears ropes beforehand
- **/listRopeSaves**
	- lists all the rope save files on the server

## Wrench Events
- **ropeToBricks**
	-	`[brick names]` `[colour]` `[diameter]` `[slack]`
	- Seperate brick names with spaces to create multiple ropes.
- **ropeClearAll**
	- Clears all ropes generated by the brick.

## Preferences
- **Slacked Rope Shapes**
	-	Number of shapes to make up ropes with > 0 slack
- **Rope Tool Admin Only**
	-	Whether or not only admins should have access to the Rope Tool.
- **Max Ropes for Non-Admins**
	-	Max amount of ropes players can create with the Rope Tool.
- **Rope Vertices**
	- How many edges the rope cylinders should have.

## Media
[![Video](https://i.imgur.com/th9viQK.png)](https://www.youtube.com/watch?v=ul0RnL0D1xs)
![Many Ropes](https://i.imgur.com/m4mXstz.png)
![Cool Patterns](https://i.imgur.com/87ChrXG.jpg)
![One Big ol' Rope](https://i.imgur.com/Vm6ngUD.png)

## Scripting API
- **loadTooledRopes**("path/to/file.blr");
	- Clears all ropes and loads from a file. Returns error string.
- **saveTooledRopes**("path/to/file.blr");
	- Saves all ropes to a file. Returns error string.
- **clearRopes**("bl_id or empty string");
	- Clears ALL ropes, or just the provided BL_ID's ropes.
- **getRopeCount**("bl_id or empty string");
	- Returns the number of ALL ropes, or just the provided BL_ID's ropes.

## Credits
- [Zapk](https://github.com/zapk) - Server scripts
- [Zeblote](https://github.com/zeblote) - Bottom-print design
- [Wrapperup](https://github.com/wrapperup) - Cylinder DTS, \_aimRope function
- [Port](https://github.com/qoh) - Something probably
- **Trader:** Axis rotation functions
