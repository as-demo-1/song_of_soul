// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Preloads actor portraits to prevent hiccups when conversations start.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class PreloadActorPortraits : MonoBehaviour
    {

        [Tooltip("Preload for Unity UI.")]
        public bool supportUnityUI;

        [Tooltip("If preloading for Unity UI, collapse legacy textures to save memory. Dialogue Manager's Instantiate Database must be ticked.")]
        public bool collapseLegacyTextures;

        private List<Texture2D> legacyPortraits = new List<Texture2D>();

        private void Start()
        {
            if (DialogueManager.instance == null || DialogueManager.databaseManager == null || DialogueManager.masterDatabase == null) return;
            if (collapseLegacyTextures && !DialogueManager.instance.instantiateDatabase)
            {
                Debug.LogWarning(DialogueDebug.Prefix + ": Dialogue Manager's Instantiate Database checkbox isn't ticked. Can't collapse legacy textures.", DialogueManager.instance);
                collapseLegacyTextures = false;
            }
            var actors = DialogueManager.masterDatabase.actors;
            if (actors == null) return;
            for (int i = 0; i < actors.Count; i++)
            {
                PreloadActor(actors[i]);
            }
        }

        public void PreloadActor(Actor actor)
        {
            if (actor == null) return;
            actor.portrait = PreloadTexture(actor.portrait);
            if (actor.alternatePortraits == null) return;
            for (int i = 0; i < actor.alternatePortraits.Count; i++)
            {
                actor.alternatePortraits[i] = PreloadTexture(actor.alternatePortraits[i]);
            }
        }

        public Texture2D PreloadTexture(Texture2D texture)
        {
            if (texture == null) return null;
            if (supportUnityUI)
            {
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                if (collapseLegacyTextures)
                {
                    texture = new Texture2D(2, 2);
                }
                UITools.spriteCache.Add(texture, sprite);
            }
            legacyPortraits.Add(texture);
            return texture;
        }
    }
}