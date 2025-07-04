using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace InputHints.Samples
{
    [AddComponentMenu("Input Hints/Input Action Button")]
    [RequireComponent(typeof(Button))]
    public class InputActionButton : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField]
        private InputActionReference inputActionReference;

        [Header("Configs")]
        [SerializeField]
        [Tooltip(
            "This enables button visual feedback and re-selection. If true, the button will send pointer events to the UI system. If false, it will only invoke the button's onClick event."
        )]
        private bool sendPointerEvents = true;

        [SerializeField]
        [Tooltip(
            "If true, the button will be triggered on action release. If false, it will be triggered on action press."
        )]
        private bool triggerOnRelease;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();

            inputActionReference.action.performed += OnActionPerformed;
            inputActionReference.action.canceled += OnActionCanceled;
        }

        private void OnActionPerformed(InputAction.CallbackContext context)
        {
            if (sendPointerEvents)
            {
                ExecuteEvents.Execute(
                    button.gameObject,
                    new PointerEventData(EventSystem.current),
                    ExecuteEvents.pointerDownHandler
                );
            }

            if (!triggerOnRelease)
                Click();
        }

        private void OnActionCanceled(InputAction.CallbackContext context)
        {
            if (sendPointerEvents)
            {
                ExecuteEvents.Execute(
                    button.gameObject,
                    new PointerEventData(EventSystem.current),
                    ExecuteEvents.pointerUpHandler
                );
            }

            if (triggerOnRelease)
                Click();
        }

        private void Click()
        {
            button.onClick?.Invoke();

            if (sendPointerEvents)
            {
                ExecuteEvents.Execute(
                    button.gameObject,
                    new PointerEventData(EventSystem.current),
                    ExecuteEvents.pointerClickHandler
                );
            }
        }

        private void OnDestroy()
        {
            inputActionReference.action.performed -= OnActionPerformed;
            inputActionReference.action.canceled -= OnActionCanceled;
        }
    }
}
