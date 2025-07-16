using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SchemeText : MonoBehaviour
{
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        InputManager.OnControlsChanged += UpdateSchemeText;
    }

    private void UpdateSchemeText(PlayerInput input)
    {
        string controlScheme = input.currentControlScheme ?? "No Control Scheme.";
        text.SetText($"Control Scheme: {controlScheme}");
    }

    private void OnDestroy()
    {
        InputManager.OnControlsChanged -= UpdateSchemeText;
    }
}
