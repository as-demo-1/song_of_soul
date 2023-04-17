/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.Inspectors.Game
{
    using UnityEngine;
    using UnityEditor;
    using Opsive.Shared.Game;

    /// <summary>
    /// Shows a custom inspector for the Scheduler.
    /// </summary>
    [CustomEditor(typeof(SchedulerBase), true)]
    public class SchedulerBaseInspector : Editor
    {
        /// <summary>
        /// Draws the scheduled events list.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUI.enabled = !Application.isPlaying;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxEventCount"));
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
            GUI.enabled = true;

            var scheduler = target as SchedulerBase;
            EditorGUILayout.LabelField("Update Events Scheduled: " + (scheduler.ActiveUpdateEventCount + scheduler.ActiveFixedUpdateEventCount));
            if (scheduler.ActiveUpdateEventCount > 0) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Time", GUILayout.Width(50));
                EditorGUILayout.LabelField("Target", GUILayout.Width(300));
                EditorGUILayout.LabelField("Method");
                EditorGUILayout.EndHorizontal();
                var updateIndex = 0;
                var fixedUpdateIndex = 0;
                for (int i = 0; i < (scheduler.ActiveUpdateEventCount + scheduler.ActiveFixedUpdateEventCount); ++i) {
                    // The array will loop between both the update and fixed update events. Show both events in one list in chronological order.
                    ScheduledEventBase scheduledEvent;
                    if (updateIndex >= scheduler.ActiveUpdateEventCount) {
                        // There are no more update events.
                        scheduledEvent = scheduler.ActiveFixedUpdateEvents[fixedUpdateIndex];
                        fixedUpdateIndex++;
                    } else if (fixedUpdateIndex >= scheduler.ActiveFixedUpdateEventCount) {
                        // There are no more fixed update events.
                        scheduledEvent = scheduler.ActiveUpdateEvents[updateIndex];
                        updateIndex++;
                    } else if (scheduler.ActiveUpdateEvents[updateIndex].EndTime <= scheduler.ActiveFixedUpdateEvents[fixedUpdateIndex].EndTime) {
                        // The update event is sooner.
                        scheduledEvent = scheduler.ActiveUpdateEvents[updateIndex];
                        updateIndex++;
                    } else {
                        // The fixed update event is sooner.
                        scheduledEvent = scheduler.ActiveFixedUpdateEvents[fixedUpdateIndex];
                        fixedUpdateIndex++;
                    }

                    // A value of -1 has no end time.
                    EditorGUILayout.BeginHorizontal();
                    if (scheduledEvent.EndTime == -1) {
                        EditorGUILayout.LabelField("-", GUILayout.Width(50));
                    } else {
                        EditorGUILayout.LabelField((scheduledEvent.EndTime - Time.time).ToString("0.##"), GUILayout.Width(50));
                    }
                    var targetName = "";
                    var methodName = "";
                    if (scheduledEvent.Target != null) {
                        if (scheduledEvent.Target is Object) {
                            targetName = (scheduledEvent.Target as Object).name;
                        } else {
                            targetName = scheduledEvent.Target.GetType().Name;
                        }
                    }
                    if (scheduledEvent.Method != null) {
                        methodName = scheduledEvent.Method.Name;
                    }
                    EditorGUILayout.LabelField(targetName, GUILayout.Width(300));
                    EditorGUILayout.LabelField(methodName);
                    EditorGUILayout.EndHorizontal();
                }
            }

            // Keep repainting the inspector so the events/duration refreshes.
            Repaint();
        }
    }
}