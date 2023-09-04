// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;
using System;

namespace PixelCrushers.DialogueSystem
{

    public delegate void UsableDelegate(Usable usable);

    /// <summary>
    /// This component indicates that the game object is usable. This component works in
    /// conjunction with the Selector component. If you leave overrideName blank but there
    /// is an OverrideActorName component on the same object, this component will use
    /// the name specified in OverrideActorName.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class Usable : MonoBehaviour
    {

        /// <summary>
        /// (Optional) Overrides the name shown by the Selector.
        /// </summary>
        public string overrideName;

        /// <summary>
        /// (Optional) Overrides the use message shown by the Selector.
        /// </summary>
        public string overrideUseMessage;

        /// <summary>
        /// The max distance at which the object can be used.
        /// </summary>
        public float maxUseDistance = 5f;

        [Serializable]
        public class UsableEvents
        {
            /// <summary>
            /// Invoked when a Selector or ProximitySelector selects this Usable.
            /// </summary>
            public UnityEvent onSelect = new UnityEvent();

            /// <summary>
            /// Invoked when a Selector or ProximitySelector deselects this Usable.
            /// </summary>
            public UnityEvent onDeselect = new UnityEvent();

            /// <summary>
            /// Invoked when a Selector or ProximitySelector uses this Usable.
            /// </summary>
            public UnityEvent onUse = new UnityEvent();
        }

        public UsableEvents events;

        public event UsableDelegate disabled = delegate { };

        protected virtual void OnDisable()
        {
            disabled(this);
        }

        public virtual void Start()
        {
        }

        /// <summary>
        /// Gets the name of the override, including parsing if it contains a [lua]
        /// or [var] tag.
        /// </summary>
        /// <returns>The override name.</returns>
        public virtual string GetName()
        {
            if (string.IsNullOrEmpty(overrideName))
            {
                return DialogueActor.GetActorName(transform);
            }
            else if (overrideName.Contains("[lua") || overrideName.Contains("[var"))
            {
                return DialogueManager.GetLocalizedText(FormattedText.Parse(overrideName, DialogueManager.masterDatabase.emphasisSettings).text);
            }
            else
            {
                return DialogueManager.GetLocalizedText(overrideName);
            }
        }

        public virtual void OnSelectUsable()
        {
            if (events != null && events.onSelect != null) events.onSelect.Invoke();
        }

        public virtual void OnDeselectUsable()
        {
            if (events != null && events.onDeselect != null) events.onDeselect.Invoke();
        }

        public virtual void OnUseUsable()
        {
            if (events != null && events.onUse != null) events.onUse.Invoke();
        }

    }

}
