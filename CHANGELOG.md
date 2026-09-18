# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [[2.2.0]](https://github.com/Tirtstan/Input-Hints/releases/tag/v2.2.0) - 2026-09-18

### Added

- Added a touchscreen hint map to the Quick Start sample and wired its provider initializer.
- Added glyph mappings for the remaining supported keyboard and mouse controls, including navigation keys, keypad operators, meta keys, mouse side buttons, scrolling, and movement.

### Changed

- Updated the bundled Quick Start sprites to Kenney Input Prompts 1.5.
- Changed the sample hint maps to use solid glyph variants throughout.

### Fixed

- Rebuilt incomplete sprite-sheet slicing and corrected shifted or mismatched glyph assignments across the keyboard, mouse, gamepad, joystick, and touchscreen maps.
- Corrected stick-click glyphs, Xbox right-stick directions, PlayStation menu and touchpad controls, and Steam Controller stick and touchpad mappings.

## [[2.1.0]](https://github.com/Tirtstan/Input-Hints/releases/tag/v2.1.0) - 2026-09-13

### Added

- Added unbound lookup for full device paths. Menus can now show the correct glyphs without matching hardware connected.
- Added `HintRow` for displaying several caller-supplied control paths. It only replaces fallback text when every glyph resolves.

## [[2.0.2]](https://github.com/Tirtstan/Input-Hints/releases/tag/v2.0.2) - 2026-07-02

### Fixed

- **Hint provider registration timing** — `HintDisplay` retries hint resolution when additional providers register, and provider initializers register on `OnEnable` as well as `Awake` with `DefaultExecutionOrder(-1000)`.

## [[2.0.1]](https://github.com/Tirtstan/Input-Hints/releases/tag/v2.0.1) - 2026-06-16

### Fixed

- **Unity 6.5 Support** — `HintImageEditor` no longer uses deprecated `GetInstanceID` / `EntityId` APIs when deferring layout rebuilds; pending hints are tracked by direct reference instead.
- Updated README.md.

## [[2.0.0]](https://github.com/Tirtstan/Input-Hints/releases/tag/v2.0.0) - 2026-04-19

### Added

- **Hint maps** — `HintMapSO` assets map Input System control paths (e.g. `buttonSouth`, `space`) to sprites and optional TextMeshPro sprite assets.
- **Central resolution** — `HintManager` registers `IHintProvider` implementations in order and resolves sprites and TMP names with parent-path fallback via `InputLayoutPathUtility`.
- **Provider initializers**
    - `GamepadHintProviderInitializer` — subtype maps for Xbox, PlayStation, Switch Pro, Steam Deck, and Steam Controller, plus a fallback map.
    - `KeyboardHintProviderInitializer`, `MouseHintProviderInitializer`, `TouchscreenHintProviderInitializer`, `JoystickHintProviderInitializer` — generic `HintProviderInitializer<T>` wrappers for other device types.
- **Display components** — `HintImage` (UI), `HintSpriteRenderer` (world space), and `HintComposite` (multi-binding composites with pooled `HintImage` children). All extend `HintDisplay` (`PlayerInput`, action name, binding index, polling).
- **TextMeshPro** — `HintTMPText` swaps the active `TMP_SpriteAsset` for the current device and can replace `<action=ActionName>` placeholders in text with resolved sprite tags.
- **Editor** — inspectors for `HintImage` and `HintTMPText`, and a custom editor for `HintMapSO`.
