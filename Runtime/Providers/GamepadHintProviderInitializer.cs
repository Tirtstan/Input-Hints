using UnityEngine;

namespace InputHints.Providers
{
    /// <summary>
    /// Initializer for gamepads that maps device subtypes (Xbox, PlayStation, Switch)
    /// to different <see cref="HintMapSO"/> assets.
    /// </summary>
    [AddComponentMenu("Input Hints/Providers/Gamepad Hint Provider")]
    public class GamepadHintProviderInitializer : MonoBehaviour
    {
        [Header("Fallback")]
        [Tooltip("Used when no subtype-specific map matches the connected gamepad.")]
        [SerializeField]
        private HintMapSO fallbackMap;

        [Header("Subtype Maps")]
        [Tooltip("Map used for Xbox-style gamepads.")]
        [SerializeField]
        private HintMapSO xboxMap;

        [Tooltip("Map used for PlayStation-style gamepads.")]
        [SerializeField]
        private HintMapSO playstationMap;

        [Tooltip("Map used for Nintendo Switch Pro-style gamepads.")]
        [SerializeField]
        private HintMapSO switchProMap;

        [Tooltip("Map used for Steam Deck.")]
        [SerializeField]
        private HintMapSO steamDeckMap;

        [Tooltip("Map used for Steam Controller.")]
        [SerializeField]
        private HintMapSO steamControllerMap;

        private GamepadHintProvider provider;

        private void Awake()
        {
            provider = new GamepadHintProvider(
                fallbackMap,
                xboxMap,
                playstationMap,
                switchProMap,
                steamDeckMap,
                steamControllerMap
            );
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
