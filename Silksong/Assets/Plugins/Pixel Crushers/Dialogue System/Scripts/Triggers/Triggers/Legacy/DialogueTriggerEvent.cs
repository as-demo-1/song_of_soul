// Copyright (c) Pixel Crushers. All rights reserved.

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This enum is used for the deprecated trigger components. It has been replaced 
    /// by DialogueSystemTriggerEvent, which is used by DialogueSystemTrigger.
    /// 
    /// A bit mask enum that defines the events that can trigger barks, conversations, and 
    /// sequences. As the Dialogue System has grown, trigger events were added to the end 
    /// rather than reordering the enum (which would break serialization in existing projects).
    /// If you modify the list, you must also update the property drawer DialogueTriggerEventDrawer.
    /// </summary>
    [System.Flags]
    public enum DialogueTriggerEvent
    {

        /// <summary>
        /// Trigger when the GameObject receives an OnBarkEnd message
        /// </summary>
        OnBarkEnd = 0x1,

        /// <summary>
        /// Trigger when the GameObject receives an OnConversationEnd message
        /// </summary>
        OnConversationEnd = 0x2,

        /// <summary>
        /// Trigger when the GameObject receives an OnSequenceEnd message
        /// </summary>
        OnSequenceEnd = 0x4,

        /// <summary>
        /// Trigger when another collider enters this GameObject's trigger collider
        /// </summary>
        OnTriggerEnter = 0x8,

        /// <summary>
        /// Trigger when the GameObject starts (e.g., at the start of the level)
        /// </summary>
        OnStart = 0x10,

        /// <summary>
        /// Trigger when the GameObject receives an OnUse message (e.g., from the Selector component)
        /// </summary>
        OnUse = 0x20,

        /// <summary>
        /// Trigger when the trigger script is enabled (allows retriggering if you disable and 
        /// re-enable the script or deactivate and re-activate its GameObject.
        /// </summary>
        OnEnable = 0x40,

        /// <summary>
        /// Trigger when another collider exits this GameObject's trigger collider
        /// </summary>
        OnTriggerExit = 0x80,

        /// <summary>
        /// Trigger when the GameObject is disabled
        /// </summary>
        OnDisable = 0x100,

        /// <summary>
        /// Trigger when the GameObject is destroyed
        /// </summary>
        OnDestroy = 0x200,

        /// <summary>
        /// Don't automatically trigger
        /// </summary>
        None = 0x400,

        /// <summary>
        /// Trigger when another collider touches this collider.
        /// </summary>
        OnCollisionEnter = 0x800,

        /// <summary>
        /// Trigger when another collider stops touching this collider
        /// </summary>
        OnCollisionExit = 0x1000
    }

}
