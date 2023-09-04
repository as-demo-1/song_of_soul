// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This persistent data component keeps track of when the GameObject
    /// has been destroyed or disabled. The next time the level or game is loaded, if
    /// the GameObject has previously been destroyed/disabled, this script will
    /// destroy/deactivate it again.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class PersistentDestructible : MonoBehaviour
    {

        public enum RecordOn { Destroy, Disable }

        [Tooltip("Record destroyed on Destroy or Disable.")]
        public RecordOn recordOn = RecordOn.Destroy;

        /// <summary>
        /// Assign a Lua variable name to keep track of whether the GameObject
        /// has been destroyed. If this is blank, it uses the name of the
        /// GameObject for the variable name. If the variable is <c>true</c>,
        /// the GameObject has been destroyed.
        /// </summary>
        [Tooltip("Unique Dialogue System variable (Boolean) to record whether the GameObject has been destroyed/disabled.")]
        public string variableName = string.Empty;

        [Tooltip("Spawn an instance of this when destroyed.")]
        public GameObject spawnWhenDestroyed;

        protected string ActualVariableName
        {
            get { return string.IsNullOrEmpty(variableName) ? DialogueActor.GetPersistentDataName(transform) : variableName; }
        }

        protected bool listenForOnDestroy = false;

        /// <summary>
        /// Only listen for OnDestroy if the script has been enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            PersistentDataManager.RegisterPersistentData(gameObject);
            listenForOnDestroy = true;
        }

        /// <summary>
        /// When applying persistent data, check the variable. If it's <c>true</c>,
        /// the GameObject has been destroyed previously, so destroy it now.
        /// </summary>
        public void OnApplyPersistentData()
        {
            if (DialogueLua.GetVariable(ActualVariableName).asBool)
            {
                // Before destroying the object, make it think that the level is
                // being unloaded. This will disable any persistent data scripts
                // that use OnDestroy, since it's not really being destroyed
                // during gameplay in this case.
                gameObject.BroadcastMessage("OnLevelWillBeUnloaded", SendMessageOptions.DontRequireReceiver);
                switch (recordOn)
                {
                    case RecordOn.Destroy:
                        Destroy(gameObject);
                        break;
                    case RecordOn.Disable:
                        gameObject.SetActive(false);
                        break;
                }
                SpawnCorpse();
            }
        }

        /// <summary>
        /// If the level is being unloaded, this GameObject will be destroyed.
        /// We don't want to count this in the variable, so disable the script.
        /// </summary>
        public void OnLevelWillBeUnloaded()
        {
            listenForOnDestroy = false;
        }

        /// <summary>
        /// If the application is ending, don't listen, as this can log errors
        /// in the console.
        /// </summary>
        public void OnApplicationQuit()
        {
            listenForOnDestroy = false;
        }

        /// <summary>
        /// If the GameObject is destroyed, set its Lua variable <c>true</c>.
        /// </summary>
        public void OnDestroy()
        {
            if (!(listenForOnDestroy && (recordOn == RecordOn.Destroy))) return;
            MarkDestroyed();
        }

        private void MarkDestroyed()
        {
            if (!Application.isPlaying) return;
            if (DialogueManager.instance == null || DialogueManager.databaseManager == null || DialogueManager.masterDatabase == null) return;
            DialogueLua.SetVariable(ActualVariableName, true);
            SpawnCorpse();
        }

        public void OnDisable()
        {
            if (!(listenForOnDestroy && (recordOn == RecordOn.Disable))) return;
            MarkDestroyed();
            PersistentDataManager.UnregisterPersistentData(gameObject);
        }

        private void SpawnCorpse()
        {
            if (spawnWhenDestroyed == null) return;
            Instantiate(spawnWhenDestroyed, transform.position, transform.rotation);
        }

    }

}