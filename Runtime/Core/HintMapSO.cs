using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace InputHints
{
    /// <summary>
    /// Maps input control local paths to hint sprites.
    /// Optionally holds a <see cref="TMP_SpriteAsset"/> for inline TextMeshPro usage.
    /// </summary>
    [CreateAssetMenu(fileName = "HintMap", menuName = "Input Hints/Hint Map")]
    public class HintMapSO : ScriptableObject
    {
        [Serializable]
        public class HintEntry
        {
            [Tooltip("Input control local path, e.g. 'buttonSouth' or 'dpad/left'.")]
            public string ControlPath;

            [Tooltip("Sprite from a SpriteAtlas or sprite sheet.")]
            public Sprite Glyph;

            [Tooltip("Name for TMP <sprite name=X> tags. Falls back to ControlPath if empty.")]
            public string TMPSpriteName;

            /// <summary>
            /// Returns the resolved TMP sprite name, falling back to Glyph.name then ControlPath when empty.
            /// </summary>
            public string ResolvedTMPName =>
                string.IsNullOrEmpty(TMPSpriteName)
                    ? Glyph != null
                        ? Glyph.name
                        : ControlPath
                    : TMPSpriteName;
        }

        [SerializeField]
        private HintEntry[] entries = Array.Empty<HintEntry>();

        [Header("TMP")]
        [Tooltip(
            "TMP Sprite Asset for inline <sprite name=X> tags. Sprite names should match control paths."
        )]
        [SerializeField]
        private TMP_SpriteAsset tmpSpriteAsset;

        /// <summary>
        /// The TMP Sprite Asset associated with this hint map.
        /// Sprite names inside should match the control paths (e.g. buttonSouth, w, space).
        /// </summary>
        public TMP_SpriteAsset TMPSpriteAsset => tmpSpriteAsset;

        private Dictionary<string, HintEntry> lookup;
        private bool isDirty = true;

        /// <summary>
        /// Tries to find a hint entry for the given local control path (e.g. "buttonSouth").
        /// Uses a dictionary for O(1) lookups after first access.
        /// </summary>
        public bool TryGetEntry(string controlPath, out HintEntry entry)
        {
            RebuildLookupIfNeeded();

            if (lookup.TryGetValue(controlPath, out entry))
                return entry.Glyph != null;

            entry = null;
            return false;
        }

        /// <summary>
        /// Returns all entries. Useful for tooling and inspection.
        /// </summary>
        public IReadOnlyList<HintEntry> GetAllEntries() => entries;

        private void RebuildLookupIfNeeded()
        {
            if (!isDirty && lookup != null)
                return;

            lookup ??= new Dictionary<string, HintEntry>(entries.Length, StringComparer.Ordinal);
            lookup.Clear();

            for (int i = 0; i < entries.Length; i++)
            {
                HintEntry e = entries[i];
                if (string.IsNullOrEmpty(e.ControlPath))
                    continue;

                lookup.TryAdd(e.ControlPath, e);
            }

            isDirty = false;
        }

        private void OnValidate()
        {
            isDirty = true;
        }

        private void OnEnable()
        {
            isDirty = true;
        }
    }
}
