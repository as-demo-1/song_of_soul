// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Increments an element of the Lua Variable[] table when the GameObject is destroyed or 
    /// disabled, and then updates the quest tracker if it's attached to the Dialogue Manager
    /// object or its children. This script is useful for kill quests or gathering quests.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class IncrementOnDestroy : MonoBehaviour
    {

        public enum IncrementOn { Destroy, Disable, Manually }

        [Tooltip("Increment on Destroy or Disable.")]
        public IncrementOn incrementOn = IncrementOn.Destroy;

        /// <summary>
        /// The variable to increment.
        /// </summary>
        [Tooltip("Increment this Dialogue System variable.")]
        [VariablePopup(true)]
        public string variable = string.Empty;

        /// <summary>
        /// The increment amount. To decrement, use a negative number.
        /// </summary>
        [Tooltip("Increment the variable by this amount. Use a negative value to decrement.")]
        public int increment = 1;

        /// <summary>
        /// The minimum value.
        /// </summary>
        [Tooltip("After incrementing, ensure that the variable is at least this value.")]
        public int min = 0;

        /// <summary>
        /// The maximum value.
        /// </summary>
        [Tooltip("After incrementing, ensure that the variable is no more than this value.")]
        public int max = 100;

        [Tooltip("Optional alert message to show when incrementing.")]
        public string alertMessage = string.Empty;

        [Tooltip("Duration to show alert, or 0 to use default duration.")]
        public float alertDuration = 0;

        [Tooltip("If set, only increment if the conditions are true.")]
        public Condition condition = new Condition();

        public UnityEvent onIncrement = new UnityEvent();

        protected bool listenForOnDestroy = false;
        protected bool awakeMarkedForDestroy = false;

        protected virtual string actualVariableName
        {
            get { return string.IsNullOrEmpty(variable) ? DialogueActor.GetPersistentDataName(transform) : variable; }
        }
        protected string ActualVariableName { get { return actualVariableName; } } // Kept for 1.x compatibility.

        protected virtual void Awake()
        {
            // Check if a DestructibleSaver on the same GameObject will be destroying this
            // object when save data is applied. If so, ignore the OnDestroy:
            var destructibleSaver = GetComponent<DestructibleSaver>();
            if (destructibleSaver != null)
            {
                var saveSystem = GameObjectUtility.FindFirstObjectByType<SaveSystem>();
                if (saveSystem != null)
                {
                    if (SaveSystem.currentSavedGameData != null)
                    {
                        var destructibleSaverKey = destructibleSaver.key;
                        var s = SaveSystem.currentSavedGameData.GetData(destructibleSaverKey);
                        if (!string.IsNullOrEmpty(s))
                        {
                            var data = SaveSystem.Deserialize<DestructibleSaver.DestructibleData>(s);
                            if (data != null && data.destroyed)
                            {
                                listenForOnDestroy = false;
                                awakeMarkedForDestroy = true;
                            }
                        }
                    }
                }
            }

        }
        /// <summary>
        /// Only listen for OnDestroy if the script has been enabled.
        /// </summary>
        public virtual void OnEnable()
        {
            listenForOnDestroy = !awakeMarkedForDestroy;
            PersistentDataManager.RegisterPersistentData(gameObject);
        }

        /// <summary>
        /// If the level is being unloaded, this GameObject will be destroyed.
        /// We don't want to count this in the variable, so disable the script.
        /// </summary>
        public virtual void OnLevelWillBeUnloaded()
        {
            listenForOnDestroy = false;
        }

        /// <summary>
        /// If the application is ending, don't listen, as this can log errors
        /// in the console.
        /// </summary>
        public virtual void OnApplicationQuit()
        {
            listenForOnDestroy = false;
        }

        /// <summary>
        /// When this object is destroyed, increment the counter and update the quest tracker
        /// if incrementOn is set to Destroy.
        /// </summary>
        public virtual void OnDestroy()
        {
            if (incrementOn == IncrementOn.Destroy) TryIncrement();
        }

        /// <summary>
        /// When this object is disabled, increment the counter and update the quest tracker
        /// if incrementOn is set to Disable.
        /// </summary>
        public virtual void OnDisable()
        {
            PersistentDataManager.UnregisterPersistentData(gameObject);
            if (incrementOn == IncrementOn.Disable) TryIncrement();
        }

        /// <summary>
        /// Try to increment variable if conditions are met.
        /// </summary>
        public virtual void TryIncrement()
        {
            if (CanIncrement())
            {
                IncrementNow();
            }
        }

        /// <summary>
        /// Are the conditions correct to increment?
        /// </summary>
        protected virtual bool CanIncrement()
        {
            return Application.isPlaying &&
            listenForOnDestroy &&
            (DialogueManager.Instance != null && DialogueManager.DatabaseManager != null && DialogueManager.MasterDatabase != null) &&
            condition.IsTrue(null);
        }

        /// <summary>
        /// Increments variable. Assumes conditions to increment have already been checked and passed.
        /// </summary>
        protected virtual void IncrementNow()
        {
            int oldValue = DialogueLua.GetVariable(actualVariableName).asInt;
            int newValue = Mathf.Clamp(oldValue + increment, min, max);
            DialogueLua.SetVariable(actualVariableName, newValue);
            DialogueManager.SendUpdateTracker();
            if (!(string.IsNullOrEmpty(alertMessage) || DialogueManager.instance == null))
            {
                if (Mathf.Approximately(0, alertDuration))
                {
                    DialogueManager.ShowAlert(alertMessage);
                }
                else
                {
                    DialogueManager.ShowAlert(alertMessage, alertDuration);
                }
            }
            onIncrement.Invoke();
        }

    }

}