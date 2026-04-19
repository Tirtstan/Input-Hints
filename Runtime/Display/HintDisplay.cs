using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputHints.Display
{
    /// <summary>
    /// Abstract base for all hint display components. Handles PlayerInput tracking,
    /// event subscription, and slow-tick polling for late-spawned PlayerInput instances.
    /// Subclasses only need to implement <see cref="ApplyHint"/> and <see cref="ClearHint"/>.
    /// </summary>
    public abstract class HintDisplay : MonoBehaviour
    {
        [Header("Input Source")]
        [Tooltip("PlayerInput to read bindings and active devices from.")]
        public PlayerInput PlayerInput;

        [Tooltip("Input Action name used to resolve the displayed hint.")]
        public string ActionName = string.Empty;

        [Tooltip(
            "Optional binding index to resolve when multiple bindings are present. Default is 0."
        )]
        [Min(0)]
        public int BindingIndex = 0;

        [Header("Detection")]
        [Tooltip("How often (seconds) to check for PlayerInput changes. 0 = every frame.")]
        [Min(0f)]
        [SerializeField]
        private float pollInterval = 1f;

        [Tooltip("Automatically find a PlayerInput if none is assigned.")]
        [SerializeField]
        private bool autoCollectPlayerInput = true;

        private PlayerInput lastPlayerInput;
        private float pollTimer;
        private readonly List<string> pathBuffer = new();
        private bool isSubscribed;

        protected virtual void OnEnable()
        {
            pollTimer = 0f;
            lastPlayerInput = null;
            CheckPlayerInput();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeFromEvents();
            lastPlayerInput = null;
        }

        protected virtual void Update()
        {
            pollTimer += Time.unscaledDeltaTime;

            if (pollTimer < pollInterval)
                return;

            pollTimer = 0f;
            CheckPlayerInput();
        }

        /// <summary>
        /// Called when a hint sprite is successfully resolved for a binding.
        /// </summary>
        /// <param name="sprite">The resolved hint sprite.</param>
        /// <param name="controlPath">The control path that was resolved.</param>
        protected abstract void ApplyHint(Sprite sprite, string controlPath);

        /// <summary>
        /// Called when no hint could be resolved (clear the display).
        /// </summary>
        protected abstract void ClearHint();

        /// <summary>
        /// Force a refresh of the displayed hints. Call externally when you know
        /// the PlayerInput or action has changed.
        /// </summary>
        public void RefreshHints()
        {
            if (PlayerInput == null || !PlayerInput.isActiveAndEnabled)
            {
                ClearHint();
                return;
            }

            var devices = PlayerInput.devices;
            if (devices.Count == 0)
                return;

            if (string.IsNullOrWhiteSpace(ActionName))
                return;

            InputAction action = PlayerInput.actions.FindAction(ActionName, throwIfNotFound: false);
            if (action == null)
                return;

            if (
                InputLayoutPathUtility.TryGetActionBindingPaths(
                    action,
                    PlayerInput.currentControlScheme,
                    pathBuffer
                )
            )
            {
                OnBindingsResolved(devices, pathBuffer);
            }
            else
            {
                ClearHint();
            }
        }

        /// <summary>
        /// Called after binding paths have been resolved. Override for custom handling
        /// (e.g. <see cref="HintComposite"/> spawns multiple children).
        /// The default implementation applies the binding based on BindingIndex.
        /// </summary>
        protected virtual void OnBindingsResolved(
            IReadOnlyList<InputDevice> devices,
            IReadOnlyList<string> bindingPaths
        )
        {
            if (bindingPaths.Count > 0)
            {
                int index = Mathf.Min(BindingIndex, bindingPaths.Count - 1);
                if (HintManager.TryGetHint(devices, bindingPaths[index], out Sprite sprite))
                {
                    ApplyHint(sprite, bindingPaths[index]);
                    return;
                }
            }

            ClearHint();
        }

        private void CheckPlayerInput()
        {
            if (PlayerInput == null && autoCollectPlayerInput)
                PlayerInput = PlayerInput.all.FirstOrDefault();

            if (PlayerInput != lastPlayerInput)
            {
                UnsubscribeFromEvents();

                if (PlayerInput != null)
                {
                    SubscribeToEvents(PlayerInput);
                    RefreshHints();
                }
                else
                {
                    ClearHint();
                }

                lastPlayerInput = PlayerInput;
            }
        }

        private void SubscribeToEvents(PlayerInput playerInput)
        {
            if (isSubscribed)
                return;

            switch (playerInput.notificationBehavior)
            {
                case PlayerNotifications.InvokeUnityEvents:
                    playerInput.controlsChangedEvent.AddListener(OnControlsChanged);
                    break;
                case PlayerNotifications.InvokeCSharpEvents:
                    playerInput.onControlsChanged += OnControlsChanged;
                    break;
            }

            isSubscribed = true;
        }

        private void UnsubscribeFromEvents()
        {
            if (!isSubscribed || lastPlayerInput == null)
                return;

            switch (lastPlayerInput.notificationBehavior)
            {
                case PlayerNotifications.InvokeUnityEvents:
                    lastPlayerInput.controlsChangedEvent.RemoveListener(OnControlsChanged);
                    break;
                case PlayerNotifications.InvokeCSharpEvents:
                    lastPlayerInput.onControlsChanged -= OnControlsChanged;
                    break;
            }

            isSubscribed = false;
        }

        private void OnControlsChanged(PlayerInput playerInput)
        {
            if (playerInput == PlayerInput)
                RefreshHints();
        }
    }
}
