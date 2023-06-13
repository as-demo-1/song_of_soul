// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    [Serializable]
    public class DialogueEntrySceneEvent
    {
        public string guid = string.Empty;
        public GameObjectUnityEvent onExecute = new GameObjectUnityEvent();
    }

    /// <summary>
    /// Holds scene-specific UnityEvents referenced by a dialogue database's dialogue entries.
    /// </summary>
    [AddComponentMenu("")]
    public class DialogueSystemSceneEvents : MonoBehaviour
    {
        [HelpBox("Do not remove this GameObject. It contains UnityEvents referenced by a dialogue database. This GameObject should not be a child of the Dialogue Manager or marked as Don't Destroy On Load.", HelpBoxMessageType.Info)]
        public List<DialogueEntrySceneEvent> dialogueEntrySceneEvents = new List<DialogueEntrySceneEvent>();

        private static DialogueSystemSceneEvents m_sceneInstance = null;
        public static DialogueSystemSceneEvents sceneInstance
        {
            get
            {
                if (m_sceneInstance == null)
                {
                    m_sceneInstance = FindObjectOfType<DialogueSystemSceneEvents>();
                }
                return m_sceneInstance;
            }
            set
            {
                m_sceneInstance = value;
            }
        }

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            m_sceneInstance = null;
        }
#endif

        private void Awake()
        {
            m_sceneInstance = this;
        }

        public static int AddNewDialogueEntrySceneEvent(out string guid)
        {
            guid = string.Empty;
            if (sceneInstance == null) return -1;
            guid = Guid.NewGuid().ToString();
            var x = new DialogueEntrySceneEvent();
            x.guid = guid;
            sceneInstance.dialogueEntrySceneEvents.Add(x);
            return sceneInstance.dialogueEntrySceneEvents.Count - 1;
        }

        public static void RemoveDialogueEntrySceneEvent(string guid)
        {
            if (Application.isPlaying || sceneInstance == null) return;
            sceneInstance.dialogueEntrySceneEvents.RemoveAll(x => x.guid == guid);
        }

        public static DialogueEntrySceneEvent GetDialogueEntrySceneEvent(string guid)
        {
            if (sceneInstance == null) return null;
            return sceneInstance.dialogueEntrySceneEvents.Find(x => x.guid == guid);
        }

        public static int GetDialogueEntrySceneEventIndex(string guid)
        {
            if (sceneInstance == null) return -1;
            return sceneInstance.dialogueEntrySceneEvents.FindIndex(x => x.guid == guid);
        }

    }
}
