using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace InputHints
{
    /// <summary>
    /// Extension methods for <see cref="InputAction"/> binding index queries.
    /// </summary>
    public static class InputActionRebindingExtensions
    {
        /// <summary>
        /// Gets the indexes of all bindings in the action that match the specified binding mask.
        /// </summary>
        public static void GetBindingIndexes(this InputAction action, InputBinding bindingMask, List<int> results)
        {
            results.Clear();

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            ReadOnlyArray<InputBinding> bindings = action.bindings;
            for (int i = 0; i < bindings.Count; ++i)
            {
                if (bindingMask.Matches(bindings[i]))
                    results.Add(i);
            }
        }
    }
}
