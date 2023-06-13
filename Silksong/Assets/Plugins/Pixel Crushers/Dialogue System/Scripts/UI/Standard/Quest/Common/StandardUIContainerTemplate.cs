// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// UI holder for general UI content.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class StandardUIContainerTemplate : StandardUIContentTemplate
    {

        [NonSerialized]
        private List<StandardUIContentTemplate> m_instances = new List<StandardUIContentTemplate>();

        public List<StandardUIContentTemplate> instances { get { return m_instances; } }

        public void AddInstanceToContainer(StandardUIContentTemplate instance)
        {
            instance.gameObject.SetActive(true);
            instances.Add(instance);
            instance.transform.SetParent(this.transform, false);
        }

        public override void Despawn()
        {
            instances.ForEach(instance => instance.Despawn());
            instances.Clear();
            base.Despawn();
        }

    }
}
