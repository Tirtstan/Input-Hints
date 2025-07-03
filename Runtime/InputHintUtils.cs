using UnityEngine.InputSystem;

namespace InputHints
{
    public static class InputHintUtils
    {
        /// <summary>
        /// Extracts the control name from the binding path.
        /// </summary>
        /// <param name="bindingPath">The full binding path from the input system.</param>
        /// <returns>The control name, which is the last segment of the binding path after the last '/'.</returns>
        /// <remarks>
        /// <code>
        /// string result1 = ExtractControlName("&lt;Gamepad&gt;/buttonSouth"); // Returns "buttonSouth"
        /// string result2 = ExtractControlName("&lt;Keyboard&gt;/e"); // Returns "e"
        /// </code>
        /// </remarks>
        public static string ExtractControlName(string bindingPath)
        {
            int slashIndex = bindingPath.LastIndexOf('/');
            return slashIndex >= 0 ? bindingPath.Substring(slashIndex + 1) : bindingPath;
        }

        /// <summary>
        /// Gets the effective binding path for the current control scheme.
        /// </summary>
        /// <param name="currentScheme">The desired control scheme, eg., "Gamepad".</param>
        /// <param name="action">The InputAction to get the binding for.</param>
        /// <returns>The effective binding path for the current control scheme, or an empty string if no binding is found.</returns>
        public static string GetBindingForControlScheme(string currentScheme, InputAction action)
        {
            foreach (InputBinding binding in action.bindings)
            {
                if (string.IsNullOrEmpty(binding.groups) || binding.groups.Contains(currentScheme))
                    return binding.effectivePath;
            }

            return string.Empty;
        }
    }
}
