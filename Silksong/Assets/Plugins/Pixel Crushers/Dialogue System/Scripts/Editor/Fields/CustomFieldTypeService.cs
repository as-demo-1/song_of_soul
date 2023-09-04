using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This class manages custom field types. It identifies custom field types in the 
    /// DialogueSystemEditors and Assembly-CSharp-Editor assemblies and makes them available
    /// to the Dialogue Editor.
    /// </summary>
    public static class CustomFieldTypeService
    {

        /// <summary>
        /// Optional attribute. Use it if you want to change showing field type name (default is class name) for example CustomField_Text - > Text
        /// </summary>
        /// <param name="_showName">Name will be shown in Dialogue Editor Window </param>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        public sealed class Name : System.Attribute
        {
            public string showingName;
            public Name(string _showName)
            {
                showingName = _showName;
            }
        }

        /// <summary>
        /// Array of names of all classes which are defived from CustomFieldType class
        /// </summary>
        public static string[] typeNames;

        /// <summary>
        /// Array of Name attributes of derived classes
        /// </summary>
        public static string[] typesPublicNames;

        /// <summary>
        /// Dictionary of type classes instances. key is a name of type (type.ToString())
        /// </summary>
        public static Dictionary<string, CustomFieldType> typesMapping;

        public static string DrawField(Field field, DialogueDatabase dataBase)
        {
            TryConvertType(field);
            var fieldCustomType = GetFieldCustomType(field.typeString);
            return (fieldCustomType != null) ? fieldCustomType.Draw(field.value, dataBase) : field.value;
        }

        public static string DrawField(GUIContent label, Field field, DialogueDatabase dataBase)
        {
            TryConvertType(field);
            var fieldCustomType = GetFieldCustomType(field.typeString);
            return (fieldCustomType != null) ? fieldCustomType.Draw(label, field.value, dataBase) : field.value;
        }

        public static string DrawField(Rect rect, Field field, DialogueDatabase dataBase)
        {
            TryConvertType(field);
            var fieldCustomType = GetFieldCustomType(field.typeString);
            return (fieldCustomType != null) ? fieldCustomType.Draw(rect, field.value, dataBase) : field.value;
        }

        public static void DrawFieldType(Field field)
        {
            TryConvertType(field);

            string[] fieldTypes = GetDialogueSystemTypes();
            string[] fieldPublicNames = GetDialogueSystemPublicNames();

            // Determine current field.typeString index in fieldTypes array
            int currentIndex = 0;
            for (int i = 0; i < fieldTypes.Length; i++)
            {
                var item = fieldTypes[i];
                if (item == field.typeString)
                {
                    currentIndex = i;
                    break;
                }
            }

            int newFieldIndex = EditorGUILayout.Popup(currentIndex, fieldPublicNames);

            if (newFieldIndex != currentIndex)
            {
                field.typeString = fieldTypes[newFieldIndex];
                Dictionary<string, CustomFieldType> types = GetTypesDictionary();
                field.type = types[fieldTypes[newFieldIndex]].storeFieldAsType;
                field.value = string.Empty;
            }
        }

        public static void DrawFieldType(Rect rect, Field field)
        {
            TryConvertType(field);

            string[] fieldTypes = GetDialogueSystemTypes();
            string[] fieldPublicNames = GetDialogueSystemPublicNames();

            // Determine current field.typeString index in fieldTypes array
            int currentIndex = 0;
            for (int i = 0; i < fieldTypes.Length; i++)
            {
                var item = fieldTypes[i];
                if (item == field.typeString)
                {
                    currentIndex = i;
                    break;
                }
            }

            int newFieldIndex = EditorGUI.Popup(rect, currentIndex, fieldPublicNames);

            if (newFieldIndex != currentIndex)
            {
                field.typeString = fieldTypes[newFieldIndex];
                Dictionary<string, CustomFieldType> types = GetTypesDictionary();
                field.type = types[fieldTypes[newFieldIndex]].storeFieldAsType;
                field.value = string.Empty;
            }
        }

        /// <summary>
        /// When user opens database first time, field's typeString is null. So this function fixes it and set field.typeString to the most relevant type
        /// </summary>
        private static void TryConvertType(Field field)
        {
            if (field.typeString == string.Empty || field.typeString == null)
            {
                if (IsQuestStateField(field))
                    field.typeString = "CustomFieldType_QuestState";
                else
                    // converts (for example) FieldType.Text to class name CustomFieldType_Text which has its own drawer
                    field.typeString = "CustomFieldType_" + Enum.GetName(typeof(FieldType), (int)field.type);
            }
        }

        public static string[] GetDialogueSystemTypes()
        {
            if (typeNames == null)
                Init();
            return typeNames;
        }

        public static string[] GetDialogueSystemPublicNames()
        {
            if (typesPublicNames == null)
                Init();
            return typesPublicNames;
        }

        public static Dictionary<string, CustomFieldType> GetTypesDictionary()
        {
            if (typesMapping == null)
                Init();
            return typesMapping;
        }

        private static void Init()
        {
            List<Type> derivedTypes = FindAllDerivedTypes<CustomFieldType>();
            typeNames = new string[derivedTypes.Count];
            typesPublicNames = new string[derivedTypes.Count];

            // init array of classes names 
            for (int i = 0; i < derivedTypes.Count; i++)
            {
                //typeNames[i] = derivedTypes[i].ToString();
                typeNames[i] = derivedTypes[i].ToString().Replace("PixelCrushers.DialogueSystem.", string.Empty);
            }

            // init array of public names of classes
            for (int i = 0; i < derivedTypes.Count; i++)
            {
                string classPublicName = string.Empty;
                CustomFieldTypeService.Name MyAttribute =
                    (CustomFieldTypeService.Name)Attribute.GetCustomAttribute(derivedTypes[i], typeof(CustomFieldTypeService.Name));
                if (MyAttribute != null)
                    classPublicName = MyAttribute.showingName;
                else
                    classPublicName = derivedTypes[i].ToString().Replace("PixelCrushers.DialogueSystem.", string.Empty);

                typesPublicNames[i] = classPublicName;
            }

            typesMapping = new Dictionary<string, CustomFieldType>();
            for (int i = 0; i < derivedTypes.Count; i++)
            {
                CustomFieldType newDrawer = (CustomFieldType)CreateClassFromString(typeNames[i]);
                typesMapping.Add(typeNames[i], newDrawer);
            }
        }

        public static object CreateClassFromString(string classTypeName)
        {
            classTypeName = "PixelCrushers.DialogueSystem." + classTypeName;
            Type t = Type.GetType(classTypeName) ??
                Type.GetType(classTypeName + ",DialogueSystemEditor", false, false) ??
                Type.GetType(classTypeName + ",DialogueSystemEditors", false, false) ??
                Type.GetType(classTypeName + ",Assembly-CSharp-Editor", false, false);
            if (t == null)
            {
                t = typeof(CustomFieldType_Text);
                //---Was: throw new Exception("Type " + classTypeName + " not found.");
            }
            return Activator.CreateInstance(t);
        }

        private static List<Type> FindAllDerivedTypes<T>()
        {
            var list = new List<Type>();
            try
            {
                var derivedType = typeof(T);
                var assemblies = RuntimeTypeUtility.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var wrapperList = (from assemblyType in assembly.GetExportedTypes()
                                           where derivedType.IsAssignableFrom(assemblyType) && assemblyType != derivedType
                                           select assemblyType).ToArray();
                        list.AddRange(wrapperList);
                    }
                    catch (System.Exception)
                    {
                        // If error, ignore assembly and move on.
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Dialogue Editor: FindAllDerivedTypes error: " + e.Message + ". Please contact support@pixelcrushers.com.");
            }
            return list;
        }

        private static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            Type derivedType = typeof(T);
            return assembly.GetTypes().Where(t => t != derivedType && derivedType.IsAssignableFrom(t)).ToList();
        }

        public static CustomFieldType GetFieldCustomType(string typeString)
        {
            if (typesMapping == null)
                Init();
            if (typesMapping.ContainsKey(typeString))
            {
                return typesMapping[typeString];
            }
            else
            {
                if (string.IsNullOrEmpty(typeString)) typeString = "CustomFieldType_Text";
                else Debug.Log("Can't find type: " + typeString + ". Define a class with this type inside an Editor folder.");
                return typesMapping.ContainsKey("CustomFieldType_Text") ? typesMapping["CustomFieldType_Text"] : null;
            }
        }

        private static bool IsQuestStateField(Field field)
        {
            return (field != null) &&
                (string.Equals(field.title, "State") ||
                 (!string.IsNullOrEmpty(field.title) && field.title.EndsWith(" State")));
        }
    }

}
