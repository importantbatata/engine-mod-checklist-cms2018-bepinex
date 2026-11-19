# Changelog

## 1.0.11 — Engine-stand radial-menu fix

Addresses [issue #1](https://github.com/importantbatata/engine-mod-checklist-cms2018-bepinex/issues/1): the checklist option could appear on unrelated interactable items while an engine was on the stand.

- Restricts radial insertion to `!EngineStand`, `!PartSelectEngineStand`, `!PartSelectMountEngineStand`, `!PartUnMountEngineStand`, and `!ExamineConditionEngineStand`.
- Requires the current options to match the configured top-level menu options, avoiding submenus that retain an earlier menu key.
- Removes broad matching based on engine-prefixed options, hovered machine state, and the generic garage-wheel fallback.
- Keeps duplicate-option protection and requires a valid engine stand and menu configuration.
- Updates the declared plugin version from 1.0.10 to 1.0.11 and includes the supplied replacement DLL.

### Updating

Close the game and replace your installed `BepInEx/plugins/EngineBuildChecklist/EngineBuildChecklist.dll` with this version. Remove duplicate older copies. The plugin ID and configuration path are unchanged.

### Validation

Source changes were reviewed against the issue and repository version, and supplied files were checked by Git blob hashes. The DLL is supplied by the package author; it was not rebuilt or verified against the source here. No in-game regression test was performed.

In-game checks: with an engine on the stand, verify that unrelated interactables and submenus retain their normal menus; then verify checklist access from the supported engine-stand wheels and F8.
