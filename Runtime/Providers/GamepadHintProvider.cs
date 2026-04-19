using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_WSA
using UnityEngine.InputSystem.Switch;
#endif

namespace InputHints.Providers
{
    /// <summary>
    /// Gamepad-specific hint provider that selects the correct <see cref="HintMapSO"/>
    /// based on the detected controller subtype (Xbox, PlayStation, Switch, Steam, or fallback).
    /// </summary>
    public class GamepadHintProvider : IHintProvider
    {
        private readonly HintMapSO fallbackMap;
        private readonly HintMapSO xboxMap;
        private readonly HintMapSO playstationMap;
        private readonly HintMapSO switchProMap;
        private readonly HintMapSO steamDeckMap;
        private readonly HintMapSO steamControllerMap;

        private const string STEAM_DECK_PRODUCT = "Steam Deck";
        private const string STEAM_CONTROLLER_PRODUCT = "Steam Controller";

        public GamepadHintProvider(
            HintMapSO fallbackMap,
            HintMapSO xboxMap,
            HintMapSO playstationMap,
            HintMapSO switchProMap,
            HintMapSO steamDeckMap = null,
            HintMapSO steamControllerMap = null
        )
        {
            this.fallbackMap = fallbackMap;
            this.xboxMap = xboxMap;
            this.playstationMap = playstationMap;
            this.switchProMap = switchProMap;
            this.steamDeckMap = steamDeckMap;
            this.steamControllerMap = steamControllerMap;
        }

        public bool CanProvide(IReadOnlyList<InputDevice> activeDevices)
        {
            for (int i = 0; i < activeDevices.Count; i++)
            {
                if (activeDevices[i] is Gamepad)
                    return true;
            }

            return false;
        }

        public bool TryGetHint(
            IReadOnlyList<InputDevice> devices,
            string controlPath,
            out Sprite sprite
        )
        {
            sprite = null;

            Gamepad gamepad = FindGamepad(devices);
            if (gamepad == null)
                return false;

            HintMapSO map = GetMapForDevice(gamepad);
            if (map == null)
                return false;

            string localPath = ResolveLocalPath(gamepad, controlPath);

            if (map.TryGetEntry(localPath, out HintMapSO.HintEntry entry))
            {
                sprite = entry.Glyph;
                return true;
            }

            return false;
        }

        public bool TryGetTMPSpriteAsset(
            IReadOnlyList<InputDevice> devices,
            out TMP_SpriteAsset spriteAsset
        )
        {
            spriteAsset = null;

            Gamepad gamepad = FindGamepad(devices);
            if (gamepad == null)
                return false;

            HintMapSO map = GetMapForDevice(gamepad);
            if (map != null && map.TMPSpriteAsset != null)
            {
                spriteAsset = map.TMPSpriteAsset;
                return true;
            }

            return false;
        }

        public bool TryGetTMPName(
            IReadOnlyList<InputDevice> devices,
            string controlPath,
            out string tmpName
        )
        {
            tmpName = null;

            Gamepad gamepad = FindGamepad(devices);
            if (gamepad == null)
                return false;

            HintMapSO map = GetMapForDevice(gamepad);
            if (map == null)
                return false;

            string localPath = ResolveLocalPath(gamepad, controlPath);

            if (map.TryGetEntry(localPath, out HintMapSO.HintEntry entry))
            {
                tmpName = entry.ResolvedTMPName;
                return true;
            }

            return false;
        }

        private static Gamepad FindGamepad(IReadOnlyList<InputDevice> devices)
        {
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i] is Gamepad gp)
                    return gp;
            }

            return null;
        }

        private HintMapSO GetMapForDevice(InputDevice device)
        {
            // check for Steam Deck/Controller specifically first as they might match other types (like XInput)
            string product = device.description.product ?? string.Empty;
            if (product.Contains(STEAM_DECK_PRODUCT))
                return steamDeckMap != null ? steamDeckMap : xboxMap;
            if (product.Contains(STEAM_CONTROLLER_PRODUCT))
                return steamControllerMap != null ? steamControllerMap : fallbackMap;

            HintMapSO map = device switch
            {
                XInputController => xboxMap,
                DualShockGamepad => playstationMap,
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_WSA
                SwitchProControllerHID => switchProMap,
#endif
                Gamepad => fallbackMap,
                _ => null,
            };

            return map != null ? map : fallbackMap;
        }

        private static string ResolveLocalPath(Gamepad device, string controlPath)
        {
            string localPath = InputLayoutPathUtility.RemoveRoot(controlPath);

            if (InputLayoutPathUtility.HasPathComponent(controlPath))
            {
                InputControl control = device.TryGetChildControl(controlPath);
                if (control != null)
                    localPath = InputLayoutPathUtility.RemoveRoot(control.path);
            }

            return localPath;
        }
    }
}
