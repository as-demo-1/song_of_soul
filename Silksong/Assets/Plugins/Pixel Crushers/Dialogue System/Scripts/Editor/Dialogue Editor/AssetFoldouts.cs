// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    [Serializable]
    public class AssetFoldouts : ISerializationCallbackReceiver
    {

        public Dictionary<int, bool> properties = new Dictionary<int, bool>();
        public Dictionary<int, bool> fields = new Dictionary<int, bool>();

        public List<int> propertiesKeys = new List<int>();
        public List<bool> propertiesValues = new List<bool>();
        public List<int> fieldsKeys = new List<int>();
        public List<bool> fieldsValues = new List<bool>();

        public void OnBeforeSerialize()
        {
            SerializeDictionary(properties, propertiesKeys, propertiesValues);
            SerializeDictionary(fields, fieldsKeys, fieldsValues);
        }

        public void OnAfterDeserialize()
        {
            DeserializeDictionary(properties, propertiesKeys, propertiesValues);
            DeserializeDictionary(fields, fieldsKeys, fieldsValues);
        }

        private void SerializeDictionary(Dictionary<int, bool> dict, List<int> keys, List<bool> values)
        {
            keys.Clear();
            values.Clear();
            foreach (var kvp in dict)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        private void DeserializeDictionary(Dictionary<int, bool> dict, List<int> keys, List<bool> values)
        {
            dict.Clear();
            for (int i = 0; i < Math.Min(keys.Count, values.Count); i++)
                dict.Add(keys[i], values[i]);
        }

    }

}