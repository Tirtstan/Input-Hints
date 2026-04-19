using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputHints.TMP
{
    /// <summary>
    /// Swaps the <see cref="TMP_SpriteAsset"/> on a <see cref="TMP_Text"/> component
    /// based on the active input device. Can also dynamically replace <action=ActionName>
    /// tags securely embedded in the text with resolved sprite formatting at runtime.
    /// </summary>
    public class HintTMPText : MonoBehaviour
    {
        [Header("Components")]
        [Tooltip("TMP text component whose sprite asset and text will be managed.")]
        [SerializeField]
        private TMP_Text targetText;

        [Tooltip("PlayerInput used to detect the active device and resolve actions.")]
        public PlayerInput PlayerInput;

        [Header("Formatting")]
        [Tooltip(
            "Input Action names to look for and replace in the TMP Text box. Formatted dynamically as <action=ActionName>."
        )]
        public string[] InputActionNames;

        [Header("Detection")]
        [Tooltip("How often (seconds) to check for PlayerInput changes.")]
        [Min(0f)]
        [SerializeField]
        private float pollInterval = 1f;

        [Tooltip("Automatically find a PlayerInput if none is assigned.")]
        [SerializeField]
        private bool autoCollectPlayerInput = true;

        private PlayerInput lastPlayerInput;
        private float pollTimer;
        private bool isSubscribed;
        private TMP_SpriteAsset lastAppliedAsset;

        private string originalTextTemplate;
        private readonly List<string> pathBuffer = new();

        /// <summary>
        /// The TMP text component whose sprite asset is being managed.
        /// </summary>
        public TMP_Text TargetText => targetText;

#if UNITY_EDITOR
        private void Reset()
        {
            targetText = GetComponent<TMP_Text>();
        }
#endif

        private void Awake()
        {
            if (targetText == null)
                targetText = GetComponent<TMP_Text>();

            // Capture the starting text as the template to perform replacements on
            if (targetText != null)
                originalTextTemplate = targetText.text;
        }

        private void OnEnable()
        {
            pollTimer = 0f;
            lastPlayerInput = null;
            CheckPlayerInput();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            lastPlayerInput = null;
        }

        private void Update()
        {
            pollTimer += Time.unscaledDeltaTime;

            if (pollTimer < pollInterval)
                return;

            pollTimer = 0f;
            CheckPlayerInput();
        }

        /// <summary>
        /// Update the base string template from code (e.g. replacing the text completely)
        /// and apply current hints immediately.
        /// </summary>
        public void SetText(string newTextTemplate)
        {
            originalTextTemplate = newTextTemplate;
            UpdateHints();
        }

        /// <summary>
        /// Force a refresh of the TMP sprite asset and text based on the current device and bindings.
        /// </summary>
        public void UpdateHints()
        {
            if (targetText == null || PlayerInput == null || !PlayerInput.isActiveAndEnabled)
                return;

            var devices = PlayerInput.devices;
            if (devices.Count == 0)
                return;

            // Swap the overall TMP Sprite Asset if changed
            if (HintManager.TryGetTMPSpriteAsset(devices, out TMP_SpriteAsset spriteAsset))
            {
                if (spriteAsset != lastAppliedAsset)
                {
                    targetText.spriteAsset = spriteAsset;
                    lastAppliedAsset = spriteAsset;
                }
            }

            // Perform string replacement formatting
            if (!string.IsNullOrEmpty(originalTextTemplate))
            {
                string formattedText = originalTextTemplate;

                if (InputActionNames != null && InputActionNames.Length > 0)
                {
                    for (int i = 0; i < InputActionNames.Length; i++)
                    {
                        string actionName = InputActionNames[i];
                        if (string.IsNullOrEmpty(actionName))
                            continue;

                        // Match against editor-copied tag format
                        string placeholder = $"<action={actionName}>";
                        if (!formattedText.Contains(placeholder))
                            continue;

                        string resolvedTag = placeholder; // fallback

                        InputAction action = PlayerInput.actions.FindAction(actionName, throwIfNotFound: false);
                        if (action != null)
                        {
                            string scheme = Application.isPlaying ? PlayerInput.currentControlScheme : null;

                            lock (pathBuffer)
                            {
                                if (!InputLayoutPathUtility.TryGetActionBindingPaths(action, scheme, pathBuffer))
                                    InputLayoutPathUtility.TryGetActionBindingPaths(action, null, pathBuffer);

                                if (
                                    pathBuffer.Count > 0
                                    && HintManager.TryGetTMPName(devices, pathBuffer[0], out string tmpName)
                                )
                                {
                                    resolvedTag = $"<sprite name={tmpName}>";
                                }
                            }
                        }

                        formattedText = formattedText.Replace(placeholder, resolvedTag);
                    }
                }

                targetText.text = formattedText;
            }
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
                    UpdateHints();
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
                UpdateHints();
        }
    }
}
