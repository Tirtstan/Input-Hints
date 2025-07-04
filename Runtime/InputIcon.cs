using System;
using UnityEngine;
using UnityEngine.Search;

namespace InputHints
{
    [Serializable]
    public class InputIcon
    {
        [Tooltip(
            "e.g., 'e', 'buttonSouth', 'leftButton'. Case sensitive (camelCase). Click the 'T' button in the Input Actions window for its binding path."
        )]
        public string BindingPath;

        [SearchContext(@"dir:""Input Icons""", SearchViewFlags.GridView | SearchViewFlags.Centered)]
        public Sprite Sprite;
        public Color Tint = Color.white;
    }
}
