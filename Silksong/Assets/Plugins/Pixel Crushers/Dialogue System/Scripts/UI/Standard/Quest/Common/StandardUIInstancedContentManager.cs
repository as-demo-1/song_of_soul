// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Manages UI content that has been instantiated from templates.
    /// </summary>
    public class StandardUIInstancedContentManager
    {

        // [TODO] Pool where possible.

        protected List<StandardUIContentTemplate> instances = new List<StandardUIContentTemplate>();

        public List<StandardUIContentTemplate> instancedContent { get { return instances; } }

        /// <summary>
        /// Clears this content manager's content.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < instances.Count; i++)
            {
                instances[i].Despawn();
            }
            instances.Clear();
        }

        /// <summary>
        /// Instantiates a copy of a template, pulling it from a pool of
        /// existing copies if possible.
        /// </summary>
        /// <typeparam name="T">Subclass of StandardUIContentTemplate.</typeparam>
        /// <param name="template">Template to instantiate copy of.</param>
        /// <returns>Instance of the template.</returns>
        public T Instantiate<T>(T template) where T : StandardUIContentTemplate
        {
            if (template == null) return null;
            return GameObject.Instantiate<T>(template);
        }

        /// <summary>
        /// Adds an instance to a container. Keeps a reference to the instance
        /// so it can be reclaimed to the pool when removed.
        /// </summary>
        /// <param name="instance">Instance to add.</param>
        /// <param name="container">Container to parent instance to.</param>
        public void Add(StandardUIContentTemplate instance, RectTransform container)
        {
            if (container == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Container isn't assigned to hold instance of UI template.", instance);
                return;
            }
            instance.gameObject.SetActive(true);
            instances.Add(instance);
            instance.transform.SetParent(container, false);
        }

        /// <summary>
        /// Removes an instance and returns it to the pool.
        /// </summary>
        /// <param name="instance"></param>
        public void Remove(StandardUIContentTemplate instance)
        {
            instances.Remove(instance);
            instance.Despawn();
        }

        /// <summary>
        /// Returns the last instance added with the Add() method.
        /// </summary>
        public StandardUIContentTemplate GetLastAdded()
        {
            return (instances.Count > 0) ? instances[instances.Count - 1] : null;
        }
    }
}