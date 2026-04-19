using UnityEngine;
using UnityEngine.InputSystem;

namespace InputHints.Providers
{
    /// <summary>
    /// MonoBehaviour initializer that creates and registers a <see cref="DeviceHintProvider{T}"/>
    /// for a specific device type. Drag <see cref="HintMapSO"/> assets into the inspector.
    /// </summary>
    public class HintProviderInitializer<T> : MonoBehaviour
        where T : InputDevice
    {
        [Header("Hint Maps")]
        [Tooltip("Hint maps searched in order when resolving glyphs for this device type.")]
        [SerializeField]
        private HintMapSO[] hintMaps;

        private DeviceHintProvider<T> provider;

        private void Awake()
        {
            if (hintMaps == null || hintMaps.Length == 0)
                return;

            provider = new DeviceHintProvider<T>();
            provider.HintMaps.AddRange(hintMaps);
            HintManager.RegisterProvider(provider);
        }

        private void OnDestroy()
        {
            if (provider != null)
            {
                HintManager.UnregisterProvider(provider);
                provider = null;
            }
        }
    }
}
