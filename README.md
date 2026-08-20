# Simple Compass

A [Timberborn](https://timberborn.com/) mod that adds a floating HUD compass whose needle always points to true north on the map, updating live as you pan, zoom, and rotate the camera.

![Simple Compass screenshot](thumbnail.png)

## Features

- Needle always points to true north, regardless of camera pan/zoom/rotation.
- Left-click and drag anywhere on the dial to reposition it.
- Double-click to pin it in place — the ring turns solid gold while pinned, dim white while free to drag.
- Position and pinned state are remembered between sessions.
- Styled to match the game's own UI theme (colors and font sampled directly from the game's assets), rather than custom art.

## Installation

### Steam Workshop

*(Coming soon — link will go here once published.)*

### Manual install

1. Download or clone this repository.
2. Copy (or symlink) the folder into your Timberborn `Mods` directory, e.g.:
   - Windows: `Documents\Timberborn\Mods\SimpleCompass`
   - Linux (Steam Proton): `~/.steam/steam/steamapps/compatdata/1062090/pfx/drive_c/users/steamuser/Documents/Timberborn/Mods/SimpleCompass`
3. Launch the game and enable the mod from the in-game Mod Manager.

## Building from source

Requires the .NET SDK and a local Timberborn install.

```bash
dotnet build
```

The build compiles against Timberborn's own game DLLs, referenced from `$(TimberbornDir)/Timberborn_Data/Managed`. By default `TimberbornDir` assumes a Steam install at `~/.steam/debian-installation/steamapps/common/Timberborn`; override it if yours lives elsewhere:

```bash
dotnet build -p:TimberbornDir=/path/to/Timberborn
```

On success, `SimpleCompass.dll` is copied automatically to the repo root, which doubles as the mod folder — see [`SimpleCompass.csproj`](SimpleCompass.csproj) and [`Directory.Build.props`](Directory.Build.props) for why build output is kept outside the repo tree.

## License

[MIT](LICENSE)
