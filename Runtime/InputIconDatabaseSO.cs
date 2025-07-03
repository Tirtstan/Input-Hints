using UnityEngine;
using UnityEngine.InputSystem;

namespace InputHints
{
    [CreateAssetMenu(fileName = "Input Icon Database", menuName = "Input Hints/Input Icon Database", order = 0)]
    public class InputIconDatabaseSO : ScriptableObject
    {
        [Header("Scheme Databases")]
        [SerializeField]
        private InputIconSchemeDatabaseSO[] inputIconSchemeDatabases;

        /// <summary>
        /// Gets the input icon for a specific input action in the current control scheme.
        /// </summary>
        /// <param name="currentScheme">The current control scheme, e.g., "Gamepad".</param>
        /// <param name="inputAction">The InputAction to get the binding icon for.</param>
        /// <returns>The input icon for the action, or null if not found.</returns>
        public InputIcon GetInputIconForAction(string currentScheme, InputAction inputAction)
        {
            if (inputAction == null)
                return null;

            string binding = InputHintUtils.GetBindingForControlScheme(currentScheme, inputAction);
            if (string.IsNullOrEmpty(binding))
                return null;

            string controlName = InputHintUtils.ExtractControlName(binding);

            // find the matching database for the current scheme
            foreach (var database in inputIconSchemeDatabases)
            {
                if (database.SchemeName == currentScheme)
                {
                    // find the matching icon in this database
                    foreach (var icon in database.Icons)
                    {
                        if (icon.BindingPath == controlName)
                            return icon;
                    }

                    break;
                }
            }

            return null;
        }
    }
}
