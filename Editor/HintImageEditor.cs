using System.Collections.Generic;
using InputHints.Display;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace InputHints.Editor
{
    /// <summary>
    /// Custom editor for <see cref="HintImage"/> that validates PlayerInput
    /// notification behavior and organizes the inspector.
    /// </summary>
    [CustomEditor(typeof(HintImage)), CanEditMultipleObjects]
    public class HintImageEditor : UnityEditor.Editor
    {
        private static readonly List<int> PendingLayoutHintInstanceIds = new();
        private static readonly EditorApplication.CallbackFunction FlushPendingLayouts =
            FlushPendingHintLayouts;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Core fields
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Image"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PlayerInput"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ActionName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BindingIndex"));

            EditorGUILayout.Space();

            // Polling
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pollInterval"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoCollectPlayerInput"));

            EditorGUILayout.Space();

            // Layout element
            SerializedProperty enableLayout = serializedObject.FindProperty("EnableLayoutElement");
            EditorGUILayout.PropertyField(enableLayout);

            if (enableLayout.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("LayoutElementPriority")
                );
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LayoutElementSize"));
                EditorGUI.indentLevel--;
            }

            if (serializedObject.ApplyModifiedProperties() && !Application.isPlaying)
                QueueLayoutRebuildForTargets();

            // Validate PlayerInput notification behavior
            bool hasError = false;
            foreach (Object t in targets)
            {
                HintImage hintImage = (HintImage)t;
                if (
                    hintImage.PlayerInput != null
                    && !ValidateNotificationBehavior(hintImage.PlayerInput)
                )
                {
                    hasError = true;
                    break;
                }
            }

            if (hasError)
            {
                EditorGUILayout.HelpBox(
                    "PlayerInput's Notification Behavior must be set to "
                        + "'Invoke Unity Events' or 'Invoke C# Events' for automatic device change detection.",
                    MessageType.Error
                );
            }
        }

        private static bool ValidateNotificationBehavior(PlayerInput playerInput)
        {
            return playerInput.notificationBehavior == PlayerNotifications.InvokeUnityEvents
                || playerInput.notificationBehavior == PlayerNotifications.InvokeCSharpEvents;
        }

        private void QueueLayoutRebuildForTargets()
        {
            foreach (Object t in targets)
            {
                if (t is not HintImage hint || !hint)
                    continue;

                int id = hint.GetInstanceID();
                if (!PendingLayoutHintInstanceIds.Contains(id))
                    PendingLayoutHintInstanceIds.Add(id);
            }

            EditorApplication.delayCall -= FlushPendingLayouts;
            EditorApplication.delayCall += FlushPendingLayouts;
        }

        private static void FlushPendingHintLayouts()
        {
            EditorApplication.delayCall -= FlushPendingLayouts;

            for (int i = 0; i < PendingLayoutHintInstanceIds.Count; i++)
            {
                int id = PendingLayoutHintInstanceIds[i];
                if (EditorUtility.InstanceIDToObject(id) is not HintImage hint)
                    continue;

                if (!hint.isActiveAndEnabled)
                    continue;

                if (hint.transform is not RectTransform rect)
                    continue;

                LayoutRebuilder.MarkLayoutForRebuild(rect);
            }

            PendingLayoutHintInstanceIds.Clear();
        }
    }
}
