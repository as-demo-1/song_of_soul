// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Assets are composed primarily of data elements called fields. This class represents a 
    /// field, which is a \<title, value, type\> tuple such as \<Name, Fred, Text\>. This class 
    /// also contains several static utility functions to work with fields.
    /// </summary>
    [System.Serializable]
    public class Field
    {

        /// <summary>
        /// The title of the field, such as Name or Age.
        /// </summary>
        public string title = null;

        /// <summary>
        /// The value of the field, such as Fred or 42.
        /// </summary>
        public string value = null;

        /// <summary>
        /// The data type of the field, such as Text or Number.
        /// </summary>
        public FieldType type = FieldType.Text;

        /// <summary>
        /// The name of a field drawer class.
        /// </summary>
        public string typeString = string.Empty;

        /// <summary>
        /// Initializes a new Field.
        /// </summary>
        public Field() { }

        /// <summary>
        /// Initializes a new Field copied from a Chat Mapper field.
        /// </summary>
        /// <param name='chatMapperField'>
        /// The Chat Mapper field to copy.
        /// </param>
        public Field(ChatMapper.Field chatMapperField)
        {
            Assign(chatMapperField);
        }

        /// <summary>
        /// The list of fields that use filenames. The values of these fields should replace 
        /// backslashes with forward slashes.
        /// </summary>
        private static readonly List<string> filenameFields = new List<string>
        { DialogueSystemFields.Pictures, DialogueSystemFields.TextureFiles, DialogueSystemFields.ModelFiles,
            DialogueSystemFields.AudioFiles, DialogueSystemFields.LipsyncFiles, DialogueSystemFields.AnimationFiles };

        /// <summary>
        /// Initializes a new Field.
        /// </summary>
        /// <param name='title'>
        /// Title of the field.
        /// </param>
        /// <param name='value'>
        /// Value of the field.
        /// </param>
        /// <param name='type'>
        /// Field type.
        /// </param>
        public Field(string title, string value, FieldType type)
        {
            this.title = title;
            this.value = (filenameFields.Contains(title) && (value != null)) ? value.Replace('\\', '/') : value;
            this.type = type;
            this.typeString = GetTypeString(type);
        }

        /// <summary>
        /// Initializes a new Field.
        /// </summary>
        /// <param name='title'>Title of the field.</param>
        /// <param name='value'>Value of the field.</param>
        /// <param name='type'>Field type.</param>
        /// <param name="typeString">Custom type string.</param>
        public Field(string title, string value, FieldType type, string typeString)
        {
            this.title = title;
            this.value = (filenameFields.Contains(title) && (value != null)) ? value.Replace('\\', '/') : value;
            this.type = type;
            this.typeString = typeString;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="sourceField">Source field.</param>
        public Field(Field sourceField)
        {
            this.title = sourceField.title;
            this.value = sourceField.value;
            this.type = sourceField.type;
            this.typeString = sourceField.typeString;
        }

        /// <summary>
        /// Copies the contents of a Chat Mapper field.
        /// </summary>
        /// <param name='chatMapperField'>
        /// The Chat Mapper field to copy.
        /// </param>
        public void Assign(ChatMapper.Field chatMapperField)
        {
            if (chatMapperField != null)
            {
                title = chatMapperField.Title;
                value = (filenameFields.Contains(title) && (chatMapperField.Value != null)) ? chatMapperField.Value.Replace('\\', '/') : chatMapperField.Value;
                type = StringToFieldType(chatMapperField.Type);
                typeString = GetTypeString(type);
            }
        }

        /// <summary>
        /// A static utility method that converts a Chat Mapper type string into a FieldType.
        /// </summary>
        /// <param name='chatMapperType'>
        /// The Chat Mapper type string.
        /// </param>
        /// <returns>
        /// The field type associated with the Chat Mapper type string.
        /// </returns>
        public static FieldType StringToFieldType(string chatMapperType)
        {
            if (string.Equals(chatMapperType, "Text")) return FieldType.Text;
            if (string.Equals(chatMapperType, "Number")) return FieldType.Number;
            if (string.Equals(chatMapperType, "Boolean")) return FieldType.Boolean;
            if (string.Equals(chatMapperType, "Files")) return FieldType.Files;
            if (string.Equals(chatMapperType, "Localization")) return FieldType.Localization;
            if (string.Equals(chatMapperType, "Actor")) return FieldType.Actor;
            if (string.Equals(chatMapperType, "Item")) return FieldType.Item;
            if (string.Equals(chatMapperType, "Location")) return FieldType.Location;
            if (string.Equals(chatMapperType, "Multiline")) return FieldType.Text;
            if (DialogueDebug.logWarnings) Debug.LogError(string.Format("{0}: Unrecognized Chat Mapper type: {1}", new System.Object[] { DialogueDebug.Prefix, chatMapperType }));
            return FieldType.Text;
        }

        /// <summary>
        /// A static utility method that creates a list of fields from a list of Chat Mapper fields.
        /// </summary>
        /// <param name='chatMapperFields'>
        /// The Chat Mapper fields.
        /// </param>
        /// <returns>
        /// A list of fields copied from the Chat Mapper fields.
        /// </returns>
        public static List<Field> CreateListFromChatMapperFields(List<ChatMapper.Field> chatMapperFields)
        {
            List<Field> list = new List<Field>();
            if (chatMapperFields != null)
            {
                foreach (var chatMapperField in chatMapperFields)
                {
                    list.Add(new Field(chatMapperField));
                }
            }
            return list;
        }

        /// <summary>
        /// Copies a field list.
        /// </summary>
        /// <returns>A new copy of the field list.</returns>
        /// <param name="sourceFields">Source fields.</param>
        public static List<Field> CopyFields(List<Field> sourceFields)
        {
            List<Field> fields = new List<Field>();
            foreach (var sourceField in sourceFields)
            {
                fields.Add(new Field(sourceField));
            }
            return fields;
        }

        /// <summary>
        /// A static utility method that checks whether a field exists in a list of fields.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the field exists; otherwise <c>false</c>.
        /// </returns>
        /// <param name='fields'>
        /// A list of fields.
        /// </param>
        /// <param name='title'>
        /// Title of the field.
        /// </param>
        public static bool FieldExists(List<Field> fields, string title)
        {
            return Lookup(fields, title) != null;
        }

        /// <summary>
        /// A static utility method that looks up a field by title in a list of fields.
        /// </summary>
        /// <param name='fields'>
        /// A list of fields.
        /// </param>
        /// <param name='title'>
        /// Title of the field.
        /// </param>
        /// <returns>
        /// The first field that matches the title, or <c>null</c> if no field matches.
        /// </returns>
        public static Field Lookup(List<Field> fields, string title)
        {
            return (fields == null) ? null : fields.Find(f => string.Equals(f.title, title));
        }

        /// <summary>
        /// A static utility method that looks up a field in a list and returns its string value.
        /// </summary>
        /// <param name='fields'>
        /// A list of fields.
        /// </param>
        /// <param name='title'>
        /// Title of the field.
        /// </param>
        /// <returns>
        /// The value of the field, or <c>null</c> if the field doesn't exist in the list.
        /// </returns>
        public static string LookupValue(List<Field> fields, string title)
        {
            Field field = Lookup(fields, title);
            return (field == null) ? null : field.value;
        }

        /// <summary>
        /// A static utility method that looks up a localized version of a field and
        /// returns its string value. Given a title, this method looks for a field appended
        /// with a blank space character or underscore and then the current language code.
        /// </summary>
        /// <returns>The localized value.</returns>
        /// <param name="fields">A list of fields.</param>
        /// <param name="title">The title of the field.</param>
        public static string LookupLocalizedValue(List<Field> fields, string title)
        {
            if (Localization.isDefaultLanguage)
            {
                return LookupValue(fields, title);
            }
            else
            {
                string value = LookupValue(fields, title + " " + Localization.language);
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
                else
                {
                    value = LookupValue(fields, title + "_" + Localization.language);
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                    else
                    {
                        return LookupValue(fields, title);
                    }
                }
            }
        }

        /// <summary>
        /// A static utility method that looks up a field in a list and returns its int value.
        /// </summary>
        /// <param name='fields'>
        /// A list of fields.
        /// </param>
        /// <param name='title'>
        /// Title of the field.
        /// </param>
        /// <returns>
        /// The value of the field, or <c>0</c> if the field doesn't exist or isn't an int.
        /// </returns>
        public static int LookupInt(List<Field> fields, string title)
        {
            return Tools.StringToInt(LookupValue(fields, title));
        }

        /// <summary>
        /// A static utility method that looks up a field in a list and returns its float value.
        /// </summary>
        /// <param name='fields'>
        /// A list of fields.
        /// </param>
        /// <param name='title'>
        /// Title of the field.
        /// </param>
        /// <returns>
        /// The value of the field, or <c>0</c> if the field doesn't exist or isn't a float.
        /// </returns>
        public static float LookupFloat(List<Field> fields, string title)
        {
            return Tools.StringToFloat(LookupValue(fields, title));
        }

        /// <summary>
        /// A static utility method that looks up a field in a list and returns its bool value.
        /// </summary>
        /// <param name='fields'>
        /// A list of fields.
        /// </param>
        /// <param name='title'>
        /// Title of the field.
        /// </param>
        /// <returns>
        /// The value of the field, or <c>false</c> if the field doesn't exist or isn't a bool.
        /// </returns>
        public static bool LookupBool(List<Field> fields, string title)
        {
            return Tools.StringToBool(LookupValue(fields, title));
        }

        /// <summary>
        /// A static utility method that sets the string value of a field.
        /// </summary>
        /// <param name='fields'>
        /// A list of fields.
        /// </param>
        /// <param name='title'>
        /// The title of the field to set.
        /// </param>
        /// <param name='value'>
        /// The new value of the field.
        /// </param>
        /// <param name='type'>
        /// The type of the field.
        /// </param>
        public static void SetValue(List<Field> fields, string title, string value, FieldType type)
        {
            Field field = Lookup(fields, title);
            if (field != null)
            {
                field.value = value;
                field.type = type;
            }
            else
            {
                fields.Add(new Field(title, value, type));
            }
        }

        /// <summary>
        /// A static utility method that sets the string value of a field.
        /// </summary>
        /// <param name='fields'>
        /// A list of fields.
        /// </param>
        /// <param name='title'>
        /// The title of the field to set.
        /// </param>
        /// <param name='value'>
        /// The new value of the field.
        /// </param>
        public static void SetValue(List<Field> fields, string title, string value)
        {
            SetValue(fields, title, value, FieldType.Text);
        }

        /// <summary>
        /// A static utility method that sets the float value of a field.
        /// </summary>
        /// <param name='fields'>
        /// A list of fields.
        /// </param>
        /// <param name='title'>
        /// The title of the field to set.
        /// </param>
        /// <param name='value'>
        /// The new value of the field.
        /// </param>
        public static void SetValue(List<Field> fields, string title, float value)
        {
            SetValue(fields, title, value.ToString(System.Globalization.CultureInfo.InvariantCulture), FieldType.Number);
        }

        /// <summary>
        /// A static utility method that sets the int value of a field.
        /// </summary>
        /// <param name='fields'>
        /// A list of fields.
        /// </param>
        /// <param name='title'>
        /// The title of the field to set.
        /// </param>
        /// <param name='value'>
        /// The new value of the field.
        /// </param>
        public static void SetValue(List<Field> fields, string title, int value)
        {
            SetValue(fields, title, value.ToString(), FieldType.Number);
        }

        /// <summary>
        /// A static utility method that sets the string bool of a field.
        /// </summary>
        /// <param name='fields'>
        /// A list of fields.
        /// </param>
        /// <param name='title'>
        /// The title of the field to set.
        /// </param>
        /// <param name='value'>
        /// The new value of the field.
        /// </param>
        public static void SetValue(List<Field> fields, string title, bool value)
        {
            SetValue(fields, title, value.ToString(), FieldType.Boolean);
        }

        /// <summary>
        /// A static utility method that checks whether a field exists and has non-empty text.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the field is assigned; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='fields'>
        /// A list of fields.
        /// </param>
        /// <param name='title'>
        /// Title of the field.
        /// </param>
        public static bool IsFieldAssigned(List<Field> fields, string title)
        {
            return (AssignedField(fields, title) != null);
        }

        /// <summary>
        /// A static utility method that returns a field if it exists and has non-empty text.
        /// </summary>
        /// <returns>
        /// The field, or <c>null</c> if it doesn't exist or has empty text.
        /// </returns>
        /// <param name='fields'>
        /// A list of fields.
        /// </param>
        /// <param name='title'>
        /// Title of the field.
        /// </param>
        public static Field AssignedField(List<Field> fields, string title)
        {
            Field field = Lookup(fields, title);
            return ((field != null) && !string.IsNullOrEmpty(field.value)) ? field : null;
        }

        /// <summary>
        /// Returns the value of a field.
        /// </summary>
        /// <returns>
        /// The value of the field, or <c>null</c> if field is <c>null</c>.
        /// </returns>
        /// <param name='field'>
        /// The field.
        /// </param>
        public static string FieldValue(Field field)
        {
            return (field != null) ? field.value : null;
        }

        /// <summary>
        /// Returns the localized field title.
        /// </summary>
        /// <returns>
        /// The localized title. If localization is currently using the default language, then the
        /// default title is returned.
        /// </returns>
        /// <param name='title'>
        /// The default title.
        /// </param>
        public static string LocalizedTitle(string title)
        {
            return Localization.isDefaultLanguage ? title : string.Format("{0} {1}", new System.Object[] { title, Localization.language });
        }

        public static string GetTypeString(FieldType type)
        {
            switch (type)
            {
                case FieldType.Actor:
                    return "CustomFieldType_Actor";
                case FieldType.Boolean:
                    return "CustomFieldType_Boolean";
                case FieldType.Files:
                    return "CustomFieldType_Files";
                case FieldType.Item:
                    return "CustomFieldType_Item";
                case FieldType.Localization:
                    return "CustomFieldType_Localization";
                case FieldType.Location:
                    return "CustomFieldType_Location";
                case FieldType.Number:
                    return "CustomFieldType_Number";
                case FieldType.Text:
                default:
                    return string.Empty;
            }

        }

    }

}
