// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers
{

    /// <summary>
    /// Holds a list of references to SpawnedObject prefabs.
    /// </summary>
    public class SpawnedObjectList : ScriptableObject
    {

        [Tooltip("Save unique data on this spawned object's Saver components.")]
        [SerializeField]
        private List<SpawnedObject> m_spawnedObjectPrefabs;

        public List<SpawnedObject> spawnedObjectPrefabs
        {
            get { return m_spawnedObjectPrefabs; }
            set { m_spawnedObjectPrefabs = value; }
        }
    }
}
