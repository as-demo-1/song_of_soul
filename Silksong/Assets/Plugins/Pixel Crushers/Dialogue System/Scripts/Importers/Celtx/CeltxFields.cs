#if USE_CELTX
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.Celtx
{
    public class CeltxFields
    {
        // Dialogue System Fields
        public const string TextureFiles = "Texture Files";
        public const string ModelFiles = "Model Files";
        public const string AudioFiles = "Audio Files";
        public const string LipsyncFiles = "Lipsync Files";
        public const string AnimationFiles = "Animation Files";

        public const string CurrentPortrait = "Current Portrait";

        public const string IsItem = "Is Item";
        public const string Group = "Group";

        public const string InitialValue = "Initial Value";
        
        public const string Title = "Title";
        
        public const string Actor = "Actor";
        public const string Conversant = "Conversant";
        public const string Priority = "Priority";
        public const string Sequence = "Sequence";
        public const string VoiceOverFile = "VoiceOverFile";

        public const string DialogueText = "Dialogue Text";
        public const string Parenthetical = "Parenthetical";

        // CeltxFields
        public const string CatalogId = "Catalog ID";
        public const string Name = "Name";
        public const string Description = "Description";
        public const string CeltxId = "Celtx ID";

        public const string GeneralNotes = "General Notes";
        public const string DevelopmentNotes = "Development Notes";
        public const string DesignNotes = "Design Notes";
        public const string StoryNotes = "Story Notes";
        public const string AnimationNotes = "Animation Notes";
        public const string DevelopmentStatus = "Development Status";
        
        public const string Pictures = "Pictures";

        public const string CeltxEntryType = "Celtx Entry Type";

        public const string TriggerDescription = "Trigger Description";

        // Character
        public const string IsPlayer = "IsPlayer";
        public const string BiographyAndLore = "Biography and Lore";

        // Location
        public const string Interior = "Interior";
        public const string Exterior = "Exterior";

        // Item
        public const string ItemType = "Item Type";
        public const string ItemProperties = "Item Properties";
        public const string ItemAvailability = "Item Availability";

        // Breakdown Type - Used for mechanic, environmental, event, sound, visual, haptic
        public const string CeltxBreakdownType = "Celtx Breakdown Type";

        // Mechanic
        public const string MechanicType = "Mechanic Type";
        public const string MechanicTrigger = "Mechanic Trigger";

        // Environmental
        public const string EnvironmentalType = "Environmental Type";

        // Event
        public const string EventTrigger = "Event Trigger";

        // Sequence Catalog Item
        public const string Objectives = "Objectives";

        // Variable
        public const string VariableType = "Variable Type";
    }
}
#endif