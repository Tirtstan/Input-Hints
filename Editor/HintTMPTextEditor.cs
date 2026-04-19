using InputHints.TMP;
using UnityEditor;
using UnityEngine;

namespace InputHints.Editor
{
    /// <summary>
    /// Custom editor for <see cref="HintTMPText"/> that provides copyable action tags
    /// without needing complex runtime previewing.
    /// </summary>
    [CustomEditor(typeof(HintTMPText))]
    public class HintTMPTextEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw standard properties
            DrawDefaultInspector();

            HintTMPText hintTMP = (HintTMPText)target;

            if (hintTMP.InputActionNames == null || hintTMP.InputActionNames.Length == 0)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Action Tag Generator", EditorStyles.boldLabel);

            var tagStyle = new GUIStyle(EditorStyles.textField)
            {
                alignment = TextAnchor.MiddleLeft,
                font = EditorStyles.boldLabel.font
            };

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            foreach (string actionName in hintTMP.InputActionNames)
            {
                if (string.IsNullOrEmpty(actionName))
                    continue;

                EditorGUILayout.BeginHorizontal();

                // Fixed width for alignment
                EditorGUILayout.LabelField(
                    actionName,
                    GUILayout.Width(EditorGUIUtility.labelWidth)
                );

                string tagValue = $"<action={actionName}>";

                // Read-only text field for easy highlighting/viewing
                EditorGUILayout.TextField(tagValue, tagStyle);

                if (GUILayout.Button("Copy", GUILayout.Width(50)))
                {
                    GUIUtility.systemCopyBuffer = tagValue;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
    }
}
