# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2026-04-19

### Added

- **Hint maps** — `HintMapSO` assets map Input System control paths (e.g. `buttonSouth`, `space`) to sprites and optional TextMeshPro sprite assets.
- **Central resolution** — `HintManager` registers `IHintProvider` implementations in order and resolves sprites and TMP names with parent-path fallback via `InputLayoutPathUtility`.
- **Provider initializers**
    - `GamepadHintProviderInitializer` — subtype maps for Xbox, PlayStation, Switch Pro, Steam Deck, and Steam Controller, plus a fallback map.
    - `KeyboardHintProviderInitializer`, `MouseHintProviderInitializer`, `TouchscreenHintProviderInitializer`, `JoystickHintProviderInitializer` — generic `HintProviderInitializer<T>` wrappers for other device types.
- **Display components** — `HintImage` (UI), `HintSpriteRenderer` (world space), and `HintComposite` (multi-binding composites with pooled `HintImage` children). All extend `HintDisplay` (`PlayerInput`, action name, binding index, polling).
- **TextMeshPro** — `HintTMPText` swaps the active `TMP_SpriteAsset` for the current device and can replace `<action=ActionName>` placeholders in text with resolved sprite tags.
- **Editor** — inspectors for `HintImage` and `HintTMPText`, and a custom editor for `HintMapSO`.

[2.0.0]: https://github.com/Tirtstan/Input-Hints/releases/tag/v2.0.0
