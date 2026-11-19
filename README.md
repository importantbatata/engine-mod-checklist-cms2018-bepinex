# Engine Build Checklist

### Know what’s installed. See what’s missing. Keep your engine build moving.

Engine Build Checklist is a BepInEx mod for **Car Mechanic Simulator 2018** that turns the engine stand into a practical build reference. Open a live parts overview, see installed and missing quantities, and add parts to the game’s shopping list without leaving your build workflow.

The supplied source declares version **1.0.11** and plugin ID `local.cms18.enginebuildchecklist`.

## Latest patch: 1.0.11

Addresses [issue #1](https://github.com/importantbatata/engine-mod-checklist-cms2018-bepinex/issues/1), where the checklist option appeared on unrelated interactable items while an engine was on the stand. The radial patch now uses an explicit engine-stand menu allowlist and verifies the original option list before appending the checklist. F8 remains available in supported garage modes. See [CHANGELOG.md](CHANGELOG.md) for the patch details.

## Features

- **Live engine overview:** scans parts on the engine stand, including hidden unmounted parts, and refreshes every 0.25 seconds while open.
- **Clear priorities:** groups parts by item ID, puts rows with missing parts first, and shows installed and missing totals.
- **Shopping-list access:** click a row to invoke the game’s shopping-list action once for that part name. Repeated clicks invoke it again; this does not automatically add the entire missing quantity or purchase anything.
- **Two ways to open:** use the configurable F8 shortcut or the Engine Build Checklist radial-menu option when available.
- **Focused interaction:** supported gameplay buttons and camera axes are blocked while the panel is open, and the cursor remains visible and unlocked.
- **Readable panel:** a scrollable, screen-scaled interface uses the game’s shopping-list font where available, with green installed counts and amber missing counts.

## Requirements

- A Windows installation of Car Mechanic Simulator 2018.
- A working BepInEx installation providing `BepInEx.dll` and `0Harmony.dll` in `BepInEx/core`.
- An engine or engine block on the garage engine stand to populate the checklist.

The supplied files do not establish a tested game-build or BepInEx-version matrix. The mod patches game methods directly, so compatibility depends on those methods and assemblies matching the expected API.

## Installation

1. Close the game and make sure BepInEx is installed and loads correctly.
2. Download [EngineBuildChecklist.dll](EngineBuildChecklist.dll) using GitHub’s **Download raw file** button, or download and extract the repository ZIP.
3. Create `BepInEx/plugins/EngineBuildChecklist/` inside your game directory and copy the DLL into it. Replace an older copy and avoid keeping duplicate copies elsewhere in `plugins`.
4. Launch the game, enter the garage, and place an engine on the stand.
5. Press **F8** to open the checklist.

Expected layout:

```text
Car Mechanic Simulator 2018/
└── BepInEx/
    └── plugins/
        └── EngineBuildChecklist/
            └── EngineBuildChecklist.dll
```

Only the mod DLL is needed in the plugin folder. The C# files and build script are for development.

## Controls and configuration

| Action | Control |
| --- | --- |
| Open or close the panel in supported garage modes | F8 by default |
| Close the panel | Escape or the Close button |
| Add a part to the shopping list | Click its row |
| Open from the radial menu | Select Engine Build Checklist when available |

After the plugin first loads, BepInEx generates `BepInEx/config/local.cms18.enginebuildchecklist.cfg`. Close the game before changing the shortcut:

```ini
[Controls]
ToggleChecklist = F8
```

The radial option is enabled when an engine is present on the stand. Version 1.0.11 restricts the option to the five supported engine-stand menu keys and requires the displayed options to match that menu’s configured top-level options. Unrelated garage wheels and submenus are left unchanged. The panel opens after the radial menu finishes closing. F8 can open an empty panel with a prompt to place an engine on the stand.

The panel closes when you leave a supported garage/build mode. It restores the saved cursor state when closed.

## Included source and patches

All five C# files form one partial plugin class and must be compiled together. These are Harmony runtime patches, rather than standalone `.patch` files.

| File | Purpose |
| --- | --- |
| [EngineBuildChecklist.cs](EngineBuildChecklist.cs) | Plugin lifecycle, shortcut configuration, garage-mode checks, part scanning, sorting, totals, and shopping-list actions. |
| [EngineBuildChecklist.RadialMenuPatches.cs](EngineBuildChecklist.RadialMenuPatches.cs) | Adds the radial option, availability checks, icon and description, and opens the panel after the wheel closes. |
| [EngineBuildChecklist.GameplayInputPatches.cs](EngineBuildChecklist.GameplayInputPatches.cs) | Suppresses supported public, parameterless gameplay and camera input methods returning `bool` or `float` while open. |
| [EngineBuildChecklist.CursorPatches.cs](EngineBuildChecklist.CursorPatches.cs) | Prevents the normal cursor-lock method from running while open and maintains an unlocked cursor. |
| [EngineBuildChecklist.Panel.cs](EngineBuildChecklist.Panel.cs) | Draws the checklist panel, count columns, shopping feedback, scroll view, styling, and row dividers. |
| [build.ps1](build.ps1) | Compiles all source files against locally installed game, Unity, BepInEx, and Harmony assemblies. |
| [EngineBuildChecklist.dll](EngineBuildChecklist.dll) | Supplied prebuilt plugin binary. |

Installed tuned parts use their tuned item IDs; missing parts use their base IDs. Parts marked `SpecialGroup == 1` are excluded to match the stand loader’s exclusion. Missing counts describe mount state, not stock in your inventory, part condition, or a purchasing recommendation.

## Build from source

Keep the source files and `build.ps1` together. On Windows, open PowerShell in this directory and run:

```powershell
.\build.ps1 -GameDirectory 'D:\SteamLibrary\steamapps\common\Car Mechanic Simulator 2018'
```

Omit `-GameDirectory` if the game is installed at the script’s default location:

```text
C:\Program Files (x86)\Steam\steamapps\common\Car Mechanic Simulator 2018
```

The script uses the Windows .NET Framework 64-bit C# compiler at `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe`. It checks for the compiler, every source file, and all referenced assemblies before compiling. Compiler warnings are treated as errors. A successful build writes `EngineBuildChecklist.dll` beside the script, replacing the existing local binary.

References come from `cms2018_Data/Managed` and `BepInEx/core` in your own game installation; game and framework assemblies are not included here. Copy the rebuilt DLL into the plugin folder with the game closed.

## Troubleshooting

| Symptom | What to check |
| --- | --- |
| F8 does nothing | Confirm BepInEx loads, check the configured shortcut, and enter a supported garage/build mode. Look for the plugin’s “Ready” entry in `BepInEx/LogOutput.log`. |
| The checklist is empty | Place an engine or engine block on the stand. The checklist reads the stand’s parts rather than a car’s installed engine. |
| The radial option is absent or disabled | Confirm an engine is on the stand and try the engine-stand wheel. Use F8 as an alternate entry point and inspect the log for patch errors. |
| A shopping-list entry fails | Check `BepInEx/LogOutput.log` for the exception and confirm the game UI is ready. |
| Building reports a required file missing | Check `-GameDirectory`, BepInEx/core, the game’s Managed folder, and the .NET Framework compiler path. |
| Startup fails after a game or loader change | Inspect the log for changed methods or missing assemblies. Remove the plugin DLL to disable the mod. |

## Validation and contributions

This import preserves all seven supplied files unchanged. The README describes the supplied source. The included DLL has not been rebuilt or verified against that source, and no in-game test was performed as part of this import.

When testing changes, check F8 and radial-menu opening, empty-stand behavior, installed/missing counts, shopping-list clicks, input blocking, and cursor restoration after closing. Include your game version, BepInEx version, reproduction steps, and relevant log entries when reporting an issue.

This is an unofficial mod for Car Mechanic Simulator 2018 and is not affiliated with or endorsed by the game’s developers or publishers.
