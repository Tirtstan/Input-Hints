using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace InputHints
{
    /// <summary>
    /// Utilities for working with Input System layout and control paths.
    /// </summary>
    public static class InputLayoutPathUtility
    {
        private static readonly List<int> bindingIndexBuffer = new();

        /// <summary>
        /// Remove the root (device name) from a control path.
        /// Example: &lt;Gamepad&gt;/dpad/left → dpad/left
        /// </summary>
        public static string RemoveRoot(string inputControlPath)
        {
            if (string.IsNullOrEmpty(inputControlPath))
                return string.Empty;

            int startIndex = inputControlPath[0] == InputControlPath.Separator ? 1 : 0;
            int separationIndex = inputControlPath.IndexOf(InputControlPath.Separator, startIndex);

            if (separationIndex == -1)
                return inputControlPath;

            if (separationIndex == inputControlPath.Length)
                return string.Empty;

            return inputControlPath.Substring(separationIndex + 1);
        }

        /// <summary>
        /// Get the parent path of an input layout path.
        /// Example: /leftStick/x → /leftStick
        /// </summary>
        public static string GetParent(string inputLayoutPath)
        {
            if (string.IsNullOrEmpty(inputLayoutPath))
                return string.Empty;

            int lastSeparatorIndex = inputLayoutPath.LastIndexOf(InputControlPath.Separator);
            if (lastSeparatorIndex == -1)
                return string.Empty;

            return inputLayoutPath.Substring(0, lastSeparatorIndex);
        }

        /// <summary>
        /// Searches for bindings within an action that match the control scheme
        /// and returns their effective paths.
        /// </summary>
        /// <param name="action">Target action.</param>
        /// <param name="controlScheme">Control scheme for mask filtering.</param>
        /// <param name="results">Effective paths of detected bindings.</param>
        /// <returns>True if any binding paths were found.</returns>
        public static bool TryGetActionBindingPaths(
            InputAction action,
            string controlScheme,
            List<string> results
        )
        {
            results.Clear();

            if (action == null)
                return false;

            bindingIndexBuffer.Clear();
            action.GetBindingIndexes(InputBinding.MaskByGroup(controlScheme), bindingIndexBuffer);

            for (int i = 0; i < bindingIndexBuffer.Count; i++)
            {
                int bindingIndex = bindingIndexBuffer[i];
                if (bindingIndex < 0)
                    continue;

                results.Add(action.bindings[bindingIndex].effectivePath);
            }

            return results.Count > 0;
        }

        /// <summary>
        /// Returns true if the path contains special path components (&lt; { or ().
        /// </summary>
        public static bool HasPathComponent(string path) =>
            path.IndexOf('<') >= 0 || path.IndexOf('{') >= 0 || path.IndexOf('(') >= 0;
    }
}
