// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// The base class of all assets such as actors, conversations, items, locations and 
    /// variables found in a DialogueDatabase.
    /// </summary>
    [System.Serializable]
    public class Asset
    {

        /// <summary>
        /// Every asset has an ID number. Internally, the Dialogue System works like Chat Mapper 
        /// and references assets by ID number.
        /// </summary>
        public int id = 0;

        /// <summary>
        /// The asset's fields. An Actor asset may have fields such as Age and IsPlayer, while a 
        /// DialogueEntry asset may have fields such as Menu Text, Dialogue Text, and Video File.
        /// </summary>
        public List<Field> fields = null;

        /// <summary>
        /// Gets or sets the Name field.
        /// </summary>
        /// <value>
        /// The value of the asset's Name field.
        /// </value>
        public string Name
        {
            get { return Field.LookupValue(fields, DialogueSystemFields.Name); }
            set { Field.SetValue(fields, DialogueSystemFields.Name, value); }
        }

        /// <summary>
        /// Gets the localized Name field.
        /// </summary>
        /// <value>The value of the localized Name field.</value>
        public string localizedName
        {
            get { return Field.LookupLocalizedValue(fields, DialogueSystemFields.Name); }
        }

        /// <summary>
        /// Gets or sets the Description field, which is optional and may not exist.
        /// </summary>
        public string Description
        {
            get { return Field.LookupValue(fields, DialogueSystemFields.Description); }
            set { Field.SetValue(fields, DialogueSystemFields.Description, value); }
        }

        /// @cond FOR_V1_COMPATIBILITY
        public string LocalizedName { get { return localizedName; } }
        /// @endcond

        /// <summary>
        /// Initializes a new DialogueAsset.
        /// </summary>
        public Asset() { }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="sourceAsset">Source asset.</param>
        public Asset(Asset sourceAsset)
        {
            this.id = sourceAsset.id;
            this.fields = Field.CopyFields(sourceAsset.fields);
        }

        /// <summary>
        /// Initializes a new DialogueAsset copied from a Chat Mapper asset.
        /// </summary>
        /// <param name='chatMapperID'>
        /// The Chat Mapper asset's ID.
        /// </param>
        /// <param name='chatMapperFields'>
        /// The Chat Mapper asset's fields.
        /// </param>
        public Asset(int chatMapperID, List<ChatMapper.Field> chatMapperFields)
        {
            Assign(chatMapperID, chatMapperFields);
        }

        /// <summary>
        /// Copies a Chat Mapper asset.
        /// </summary>
        /// <param name='chatMapperID'>
        /// Chat Mapper asset's ID.
        /// </param>
        /// <param name='chatMapperFields'>
        /// The Chat Mapper asset's fields.
        /// </param>
        public void Assign(int chatMapperID, List<ChatMapper.Field> chatMapperFields)
        {
            id = chatMapperID;
            fields = Field.CreateListFromChatMapperFields(chatMapperFields);
        }

        /// <summary>
        /// Checks whether a field exists.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the field exists; otherwise <c>false</c>.
        /// </returns>
        /// <param name='title'>
        /// Title of the field.
        /// </param>
        public bool FieldExists(string title)
        {
            return Field.FieldExists(fields, title);
        }

        /// <summary>
        /// Looks up the value of a field.
        /// </summary>
        /// <returns>
        /// The string value of the field with the specified title, or <c>null</c> if no field 
        /// matches.
        /// </returns>
        /// <param name='title'>
        /// The title of the field to look up.
        /// </param>
        public string LookupValue(string title)
        {
            return Field.LookupValue(fields, title);
        }


        /// <summary>
        /// Looks up the localized value of a field for the current language.
        /// </summary>
        /// <returns>The localized value.</returns>
        /// <param name="title">The title of the field to look up.</param>
        public string LookupLocalizedValue(string title)
        {
            return Field.LookupLocalizedValue(fields, title);
        }

        /// <summary>
        /// Looks up the value of a field.
        /// </summary>
        /// <returns>
        /// The int value of the field with the specified title, or <c>0</c> if no field matches.
        /// </returns>
        /// <param name='title'>
        /// The title of the field to look up.
        /// </param>
        public int LookupInt(string title)
        {
            return Field.LookupInt(fields, title);
        }

        /// <summary>
        /// Looks up the value of a field.
        /// </summary>
        /// <returns>
        /// The float value of the field with the specified title, or <c>0</c> if no field matches.
        /// </returns>
        /// <param name='title'>
        /// The title of the field to look up.
        /// </param>
        public float LookupFloat(string title)
        {
            return Field.LookupFloat(fields, title);
        }

        /// <summary>
        /// Looks up the value of a field.
        /// </summary>
        /// <returns>
        /// The bool value of the field with the specified title, or <c>false</c> if no field matches.
        /// </returns>
        /// <param name='title'>
        /// The title of the field to look up.
        /// </param>
        public bool LookupBool(string title)
        {
            return Field.LookupBool(fields, title);
        }

        /// <summary>
        /// Checks whether a field exists and has non-empty text.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the field is assigned; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='title'>
        /// Title of the field.
        /// </param>
        public bool IsFieldAssigned(string title)
        {
            return Field.IsFieldAssigned(fields, title);
        }

        /// <summary>
        /// Returns a field if it exists and has non-empty text.
        /// </summary>
        /// <returns>
        /// The field, or <c>null</c> if it doesn't exist or has empty text.
        /// </returns>
        /// <param name='title'>
        /// Title of the field.
        /// </param>
        public Field AssignedField(string title)
        {
            return Field.AssignedField(fields, title);
        }

    }

}
