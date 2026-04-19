using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputHints.Providers
{
    /// <summary>
    /// Generic hint provider for a specific <see cref="InputDevice"/> type.
    /// Looks up control paths against one or more <see cref="HintMapSO"/> assets.
    /// </summary>
    public class DeviceHintProvider<T> : IHintProvider
        where T : InputDevice
    {
        public readonly List<HintMapSO> HintMaps = new();

        public bool CanProvide(IReadOnlyList<InputDevice> activeDevices)
        {
            for (int i = 0; i < activeDevices.Count; i++)
            {
                if (activeDevices[i] is T)
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

            T supportedDevice = FindDevice(devices);
            if (supportedDevice == null)
                return false;

            string localPath = ResolveLocalPath(supportedDevice, controlPath);

            for (int i = 0; i < HintMaps.Count; i++)
            {
                if (
                    HintMaps[i] != null
                    && HintMaps[i].TryGetEntry(localPath, out HintMapSO.HintEntry entry)
                )
                {
                    sprite = entry.Glyph;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetTMPSpriteAsset(
            IReadOnlyList<InputDevice> devices,
            out TMP_SpriteAsset spriteAsset
        )
        {
            spriteAsset = null;

            if (!CanProvide(devices))
                return false;

            for (int i = 0; i < HintMaps.Count; i++)
            {
                if (HintMaps[i] != null && HintMaps[i].TMPSpriteAsset != null)
                {
                    spriteAsset = HintMaps[i].TMPSpriteAsset;
                    return true;
                }
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

            T supportedDevice = FindDevice(devices);
            if (supportedDevice == null)
                return false;

            string localPath = ResolveLocalPath(supportedDevice, controlPath);

            for (int i = 0; i < HintMaps.Count; i++)
            {
                if (
                    HintMaps[i] != null
                    && HintMaps[i].TryGetEntry(localPath, out HintMapSO.HintEntry entry)
                )
                {
                    tmpName = entry.ResolvedTMPName;
                    return true;
                }
            }

            return false;
        }

        private static T FindDevice(IReadOnlyList<InputDevice> devices)
        {
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i] is T typed)
                    return typed;
            }

            return null;
        }

        private static string ResolveLocalPath(T device, string controlPath)
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
