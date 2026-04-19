using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InputHints.Editor
{
    /// <summary>
    /// Custom editor for <see cref="HintMapSO"/> that shows a preview of sprites
    /// and highlights entries with missing data.
    /// </summary>
    [CustomEditor(typeof(HintMapSO))]
    public class HintMapSOEditor : UnityEditor.Editor
    {
        private SerializedProperty entriesProperty;
        private SerializedProperty tmpSpriteAssetProperty;

        private void OnEnable()
        {
            entriesProperty = serializedObject.FindProperty("entries");
            tmpSpriteAssetProperty = serializedObject.FindProperty("tmpSpriteAsset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            HintMapSO hintMap = (HintMapSO)target;

            EditorGUILayout.PropertyField(entriesProperty, true);
            EditorGUILayout.PropertyField(tmpSpriteAssetProperty);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            // Summary
            IReadOnlyList<HintMapSO.HintEntry> entries = hintMap.GetAllEntries();

            int total = entries.Count;
            int missingSprite = 0;
            int missingPath = 0;
            int hasTMPName = 0;

            for (int i = 0; i < entries.Count; i++)
            {
                if (string.IsNullOrEmpty(entries[i].ControlPath))
                    missingPath++;
                if (entries[i].Glyph == null)
                    missingSprite++;
                if (!string.IsNullOrEmpty(entries[i].TMPSpriteName))
                    hasTMPName++;
            }

            EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  Total entries: {total}");
            EditorGUILayout.LabelField($"  With TMP names: {hasTMPName}");
            EditorGUILayout.LabelField(
                $"  TMP Atlas assigned: {(hintMap.TMPSpriteAsset != null ? "Yes" : "No")}"
            );

            if (missingPath > 0)
                EditorGUILayout.HelpBox(
                    $"{missingPath} entries are missing a ControlPath.",
                    MessageType.Warning
                );

            if (missingSprite > 0)
                EditorGUILayout.HelpBox(
                    $"{missingSprite} entries are missing a Glyph sprite.",
                    MessageType.Warning
                );
        }
    }
}
