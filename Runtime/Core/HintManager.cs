using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputHints
{
    /// <summary>
    /// Central registry for hint providers. Queries registered providers in order
    /// and returns the first successful result. Falls back to parent control paths
    /// when no direct match is found.
    /// </summary>
    public static class HintManager
    {
        private static readonly List<IHintProvider> providers = new();

        /// <summary>
        /// Register a hint provider. Providers are queried in registration order.
        /// </summary>
        public static void RegisterProvider(IHintProvider provider)
        {
            if (provider != null && !providers.Contains(provider))
                providers.Add(provider);
        }

        /// <summary>
        /// Unregister a previously registered hint provider.
        /// </summary>
        public static void UnregisterProvider(IHintProvider provider)
        {
            if (provider != null)
                providers.Remove(provider);
        }

        /// <summary>
        /// Tries to get a hint sprite for the given control path, falling back to
        /// parent paths (e.g. "dpad/left" → "dpad") if no direct match is found.
        /// </summary>
        public static bool TryGetHint(IReadOnlyList<InputDevice> activeDevices, string controlPath, out Sprite sprite)
        {
            for (int i = 0; i < providers.Count; i++)
            {
                if (providers[i].TryGetHint(activeDevices, controlPath, out sprite))
                    return true;
            }

            // Fall back to parent path
            string parentPath = InputLayoutPathUtility.GetParent(controlPath);
            if (!string.IsNullOrEmpty(parentPath))
                return TryGetHint(activeDevices, parentPath, out sprite);

            sprite = null;
            return false;
        }

        /// <summary>
        /// Tries to get the TMP Sprite Asset for the current active devices.
        /// Returns the asset from the first provider that can handle the devices.
        /// </summary>
        public static bool TryGetTMPSpriteAsset(
            IReadOnlyList<InputDevice> activeDevices,
            out TMP_SpriteAsset spriteAsset
        )
        {
            for (int i = 0; i < providers.Count; i++)
            {
                if (
                    providers[i].CanProvide(activeDevices)
                    && providers[i].TryGetTMPSpriteAsset(activeDevices, out spriteAsset)
                )
                    return true;
            }

            spriteAsset = null;
            return false;
        }

        /// <summary>
        /// Tries to get a TMP sprite name for the given control path, with parent fallback.
        /// </summary>
        public static bool TryGetTMPName(
            IReadOnlyList<InputDevice> activeDevices,
            string controlPath,
            out string tmpName
        )
        {
            for (int i = 0; i < providers.Count; i++)
            {
                if (providers[i].TryGetTMPName(activeDevices, controlPath, out tmpName))
                    return true;
            }

            string parentPath = InputLayoutPathUtility.GetParent(controlPath);
            if (!string.IsNullOrEmpty(parentPath))
                return TryGetTMPName(activeDevices, parentPath, out tmpName);

            tmpName = null;
            return false;
        }

        /// <summary>
        /// Clears all registered providers. Useful for cleanup or domain reloads.
        /// </summary>
        public static void ClearProviders()
        {
            providers.Clear();
        }
    }
}
