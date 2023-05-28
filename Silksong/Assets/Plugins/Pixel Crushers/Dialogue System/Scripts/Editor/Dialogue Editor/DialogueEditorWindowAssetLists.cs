// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles asset lists, which are lists of
    /// assets such as actors, items, and locations. Asset lists are used in popup menus
    /// in other parts of the class.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        private const string NoIDString = "-1";

        private class AssetList
        {
            public string[] IDs;
            public GUIContent[] names;

            public AssetList(string[] IDs, GUIContent[] names)
            {
                this.IDs = IDs;
                this.names = names;
            }

            public string GetID(int index)
            {
                return ((0 <= index) && (index < IDs.Length)) ? IDs[index] : NoIDString;
            }

            public int GetIndex(int id)
            {
                for (int index = 0; index < IDs.Length; index++)
                {
                    if (id == Tools.StringToInt(IDs[index])) return index;
                }
                return -1;
            }
        }

        private AssetList actorAssetList = null;
        private AssetList itemAssetList = null;
        private AssetList locationAssetList = null;
        private Dictionary<int, string> actorNamesByID = new Dictionary<int, string>();

        private void ResetAssetLists()
        {
            actorAssetList = null;
            itemAssetList = null;
            locationAssetList = null;
            actorNamesByID.Clear();
        }

        private AssetList GetAssetList<T>(List<T> assets) where T : Asset
        {
            if (typeof(T) == typeof(Actor))
            {
                if (actorAssetList == null) actorAssetList = MakeAssetList<T>(assets);
                return actorAssetList;
            }
            else if (typeof(T) == typeof(Item))
            {
                if (itemAssetList == null) itemAssetList = MakeAssetList<T>(assets);
                return itemAssetList;
            }
            else {
                if (typeof(T) != typeof(Location)) Debug.LogError(string.Format("{0}: Internal error: Unexpected type for asset list popup: {1}", DialogueDebug.Prefix, typeof(T).Name));
                if (locationAssetList == null) locationAssetList = MakeAssetList<T>(assets);
                return locationAssetList;
            }
        }

        private AssetList MakeAssetList<T>(List<T> assets) where T : Asset
        {
            List<string> IDs = new List<string>();
            List<GUIContent> names = new List<GUIContent>();
            IDs.Add(NoIDString);
            names.Add(new GUIContent("(None)", string.Empty));
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                IDs.Add(asset.id.ToString());
                names.Add(new GUIContent(string.Format("{0} [{1}]", asset.Name, asset.id), string.Empty));
            }
            return new AssetList(IDs.ToArray(), names.ToArray());
        }

        private string GetActorNameByID(int actorID)
        {
            if ((actorNamesByID.Count == 0) && (database != null))
            {
                database.actors.ForEach(actor => actorNamesByID[actor.id] = actor.Name);
            }
            return actorNamesByID.ContainsKey(actorID) ? actorNamesByID[actorID] : string.Empty;
        }

    }

}