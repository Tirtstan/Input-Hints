using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputHints
{
    /// <summary>
    /// Interface for device-specific hint providers.
    /// Returns standard Sprite references mapping to current controls.
    /// </summary>
    public interface IHintProvider
    {
        /// <summary>
        /// Whether this provider can handle any of the given active devices.
        /// </summary>
        public bool CanProvide(IReadOnlyList<InputDevice> activeDevices);

        /// <summary>
        /// Tries to get a hint sprite for the given control path.
        /// </summary>
        /// <param name="devices">Currently active devices.</param>
        /// <param name="controlPath">Full input layout path, e.g. "&lt;Gamepad&gt;/buttonSouth".</param>
        /// <param name="sprite">The resolved hint sprite, if found.</param>
        public bool TryGetHint(IReadOnlyList<InputDevice> devices, string controlPath, out Sprite sprite);

        /// <summary>
        /// Tries to get the TMP Sprite Asset for the current device.
        /// Used to swap <see cref="TMP_Text.spriteAsset"/> when devices change.
        /// </summary>
        /// <param name="devices">Currently active devices.</param>
        /// <param name="spriteAsset">The resolved TMP Sprite Asset, if found.</param>
        public bool TryGetTMPSpriteAsset(IReadOnlyList<InputDevice> devices, out TMP_SpriteAsset spriteAsset);

        /// <summary>
        /// Tries to get a TMP sprite name for the given control path.
        /// </summary>
        /// <param name="devices">Currently active devices.</param>
        /// <param name="controlPath">Full input layout path.</param>
        /// <param name="tmpName">The resolved TMP sprite name, if found.</param>
        public bool TryGetTMPName(IReadOnlyList<InputDevice> devices, string controlPath, out string tmpName);
    }

    /// <summary>
    /// Optional capability for providers that can resolve hints from a full
    /// device-rooted control path without a connected device instance.
    /// Used by menus that display glyphs for schemes with no hardware attached.
    /// </summary>
    public interface IUnboundHintProvider
    {
        /// <summary>
        /// Tries to get a hint sprite for the given device-rooted control path.
        /// </summary>
        /// <param name="controlPath">Full input layout path, e.g. "&lt;Gamepad&gt;/buttonSouth".</param>
        /// <param name="sprite">The resolved hint sprite, if found.</param>
        public bool TryGetHintUnbound(string controlPath, out Sprite sprite);
    }
}
