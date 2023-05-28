// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This class manages a list of asset bundles. It's used by DialogueSystemController.
    /// It allows you to register asset bundles and then load assets from asset bundles
    /// or Resources without having to specify which asset bundle (or Resources) contains
    /// the asset.
    /// </summary>
    public class AssetBundleManager
    {

        private HashSet<AssetBundle> m_bundles = new HashSet<AssetBundle>();

        public void RegisterAssetBundle(AssetBundle bundle)
        {
            if (bundle == null) return;
            m_bundles.Add(bundle);
        }

        public void UnregisterAssetBundle(AssetBundle bundle)
        {
            if (bundle == null) return;
            m_bundles.Remove(bundle);
        }

        public UnityEngine.Object Load(string name)
        {
            foreach (var bundle in m_bundles)
            {
                if (bundle.Contains(name))
                {
                    return LoadFromBundle(bundle, name);
                }
            }
            return Resources.Load(name);
        }

        public UnityEngine.Object Load(string name, System.Type type)
        {
            foreach (var bundle in m_bundles)
            {
                if (bundle.Contains(name))
                {
                    return LoadFromBundle(bundle, name, type);
                }
            }
            return Resources.Load(name, type);
        }

        private UnityEngine.Object LoadFromBundle(AssetBundle bundle, string name)
        {
            return bundle.LoadAsset(name);
        }

        private UnityEngine.Object LoadFromBundle(AssetBundle bundle, string name, System.Type type)
        {
            return bundle.LoadAsset(name, type);
        }

    }

}
