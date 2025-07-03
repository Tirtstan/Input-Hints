using UnityEngine;

namespace InputHints
{
    [CreateAssetMenu(
        fileName = "Input Icon Scheme Database",
        menuName = "Input Hints/Input Icon Scheme Database",
        order = 1
    )]
    public class InputIconSchemeDatabaseSO : ScriptableObject
    {
        [Header("Control Scheme Properties")]
        [Tooltip(
            "Name of the control scheme, e.g., 'Keyboard&Mouse', 'Gamepad'. Find them in your Input Actions asset."
        )]
        public string SchemeName = "Keyboard&Mouse";
        public InputIcon[] Icons;
    }
}
