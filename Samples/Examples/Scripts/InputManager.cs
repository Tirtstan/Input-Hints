using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace InputHints.Samples
{
    [DefaultExecutionOrder(-10)]
    [RequireComponent(typeof(PlayerInput))]
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }
        public static event Action<PlayerInput> OnActionMapChanged;
        public static event Action<PlayerInput> OnControlsChanged;

        [Header("Database")]
        [SerializeField]
        private InputIconDatabaseSO iconDatabase;

        [Header("Configs")]
        [SerializeField]
        [Tooltip("Control schemes that enable the deselection on background click toggle if current (EventSystem).")]
        private string[] deselectableSchemes = { "Keyboard&Mouse" };
        private PlayerInput playerInput;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            playerInput = GetComponent<PlayerInput>();
            playerInput.onControlsChanged += ControlsChanged;
        }

        private void Start()
        {
            ControlsChanged(playerInput);
        }

        private void ControlsChanged(PlayerInput input)
        {
            InputSystemUIInputModule inputSystemUIInputModule =
                EventSystem.current.currentInputModule as InputSystemUIInputModule;

            bool isDeselectableScheme = Array.Exists(
                deselectableSchemes,
                scheme => scheme == input.currentControlScheme
            );

            OnControlsChanged?.Invoke(input);
        }

        public void SwitchActionMap(string actionMapName)
        {
            if (playerInput.currentActionMap.name != actionMapName)
            {
                playerInput.SwitchCurrentActionMap(actionMapName);
                OnActionMapChanged?.Invoke(playerInput);
            }
        }

        public InputIcon GetInputIconForAction(InputActionReference actionReference) =>
            iconDatabase.GetInputIconForAction(playerInput.currentControlScheme, actionReference.action);

        public PlayerInput GetPlayerInput() => playerInput;

        private void OnDestroy()
        {
            playerInput.onControlsChanged -= ControlsChanged;
        }
    }
}
