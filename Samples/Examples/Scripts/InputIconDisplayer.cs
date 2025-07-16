using InputHints;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
[AddComponentMenu("Input Hints/Input Icon Displayer")]
public class InputIconDisplayer : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("Components")]
    [SerializeField]
    private InputActionReference inputActionReference;

    [SerializeField]
    private Image iconImage;

    [Header("Configs")]
    [SerializeField]
    [Tooltip(
        "If true, the icon will be shown when the UI element is selected, and hidden when deselected. If false, it will be always be shown."
    )]
    private bool toggleOnSelection;
    private InputIcon inputIcon;

    private void OnEnable()
    {
        InputManager.OnControlsChanged += OnControlsChanged;
        UpdateInputIcon();
    }

    private void OnControlsChanged(PlayerInput _) => UpdateInputIcon();

    private void UpdateInputIcon()
    {
        inputIcon = InputManager.Instance.GetInputIconForAction(inputActionReference);
        SetInputIcon(inputIcon);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (toggleOnSelection)
            ShowIcon();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (toggleOnSelection)
            HideIcon();
    }

    private void SetInputIcon(InputIcon inputIcon)
    {
        iconImage.sprite = inputIcon.Sprite;
        iconImage.preserveAspect = true;

        if (!toggleOnSelection || IsSelected())
        {
            ShowIcon();
        }
        else
        {
            HideIcon();
        }
    }

    private void ShowIcon()
    {
        if (inputIcon != null)
            iconImage.color = inputIcon.Tint;
    }

    private void HideIcon()
    {
        iconImage.color = Color.clear;
    }

    private bool IsSelected() => EventSystem.current.currentSelectedGameObject == gameObject;

    private void OnDisable()
    {
        InputManager.OnControlsChanged -= OnControlsChanged;
    }
}
