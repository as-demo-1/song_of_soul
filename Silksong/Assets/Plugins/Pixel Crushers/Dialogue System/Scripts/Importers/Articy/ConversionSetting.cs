#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

namespace PixelCrushers.DialogueSystem.Articy
{

    /// <summary>
    /// An articy Entity can be an NPC, Player, Item, or Quest. This enum is used to allow the user to
    /// specify the type of the entity so it can be imported properly as a dialogue database.
    /// </summary>
    public enum EntityCategory { NPC, Player, Item, Quest };

    /// <summary>
    /// The current conversion preference for an articy element.
    /// </summary>
    [System.Serializable]
    public class ConversionSetting
    {

        public string Id { get; set; }
        public bool Include { get; set; }
        public EntityCategory Category { get; set; }

        public ConversionSetting()
        {
            Assign(null);
        }

        public ConversionSetting(string id)
        {
            Assign(id);
        }

        private void Assign(string id)
        {
            Id = id;
            Include = !string.IsNullOrEmpty(id);
            Category = EntityCategory.NPC;
        }

    }

}
#endif
