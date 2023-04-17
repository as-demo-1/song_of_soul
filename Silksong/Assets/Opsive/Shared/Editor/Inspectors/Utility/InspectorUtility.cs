/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.Inspectors.Utility
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Small static functions used for interacting with the inspector.
    /// </summary>
    public static class InspectorUtility
    {
        private const int c_IndentWidth = 15;
        public static int IndentWidth { get { return c_IndentWidth; } }

        private const string c_EditorPrefsFoldoutKey = "Opsive.Shared.Editor.Foldout.";
        private static Dictionary<object, bool> s_FoldoutValueMap = new Dictionary<object, bool>();

        /// <summary>
        /// Local struct used as key for the s_ObjectTypes dictionary.
        /// </summary>
        private struct TypeWithCondition
        {
            public Type Type;
            public Func<Type, bool> Condition;
        }
        private static Dictionary<TypeWithCondition, List<Type>> s_ObjectTypes = new Dictionary<TypeWithCondition, List<Type>>();

        /// <summary>
        /// Draws a EditorGUI foldout, saving the foldout expand/collapse bool within a EditorPref.
        /// </summary>
        /// <param name="obj">The object that is being drawn beneath the foldout.</param>
        /// <param name="name">The name of the foldout.</param>
        /// <returns>True if the foldout is expanded.</returns>
        public static bool Foldout(object obj, string name)
        {
            return Foldout(obj, new GUIContent(name), true, string.Empty);
        }

        /// <summary>
        /// Draws a EditorGUI foldout, saving the foldout expand/collapse bool within a EditorPref.
        /// </summary>
        /// <param name="obj">The object that is being drawn beneath the foldout.</param>
        /// <param name="guiContent">The GUIContent of the foldout.</param>
        /// <returns>True if the foldout is expanded.</returns>
        public static bool Foldout(object obj, GUIContent guiContent)
        {
            return Foldout(obj, guiContent, true, string.Empty);
        }

        /// <summary>
        /// Draws a EditorGUI foldout, saving the foldout expand/collapse bool within a EditorPref.
        /// </summary>
        /// <param name="obj">The object that is being drawn beneath the foldout.</param>
        /// <param name="guiContent">The GUIContent of the foldout.</param>
        /// <param name="defaultExpanded">The default value if the foldout is expanded.</param>
        /// <returns>True if the foldout is expanded.</returns>
        public static bool Foldout(object obj, GUIContent guiContent, bool defaultExpanded)
        {
            return Foldout(obj, guiContent, defaultExpanded, string.Empty);
        }

        /// <summary>
        /// Draws a EditorGUI foldout, saving the foldout expand/collapse bool within a EditorPref.
        /// </summary>
        /// <param name="obj">The object that is being drawn beneath the foldout.</param>
        /// <param name="guiContent">The GUIContent of the foldout.</param>
        /// <param name="defaultExpanded">The default value if the foldout is expanded.</param>
        /// <param name="identifyingString">A string that can be used to help identify the foldout key.</param>
        /// <returns>True if the foldout is expanded.</returns>
        public static bool Foldout(object obj, GUIContent guiContent, bool defaultExpanded, string identifyingString)
        {
            if (obj == null) {
                return false;
            }
            var key = c_EditorPrefsFoldoutKey + "." + obj.GetType() + (obj is MonoBehaviour ? ("." + (obj as MonoBehaviour).name) : string.Empty) + "." + identifyingString + "." + guiContent.text;
            bool prevFoldout;
            if (!s_FoldoutValueMap.TryGetValue(key, out prevFoldout)) {
                prevFoldout = EditorPrefs.GetBool(key, defaultExpanded);
                s_FoldoutValueMap.Add(key, prevFoldout);
            }
            var foldout = EditorGUILayout.Foldout(prevFoldout, guiContent);
            if (foldout != prevFoldout) {
                EditorPrefs.SetBool(key, foldout);
                s_FoldoutValueMap[key] = foldout;
            }
            return foldout;
        }

        /// <summary>
        /// Adds the component of the specified type if it doesn't already exist.
        /// </summary>
        /// <typeparam name="T">The type of component to add.</typeparam>
        /// <param name="gameObject">The GameObject to add the component to.</param>
        /// <returns>The added component.</returns>
        public static T AddComponent<T>(GameObject gameObject) where T : Component
        {
            T component;
            if ((component = gameObject.GetComponent<T>()) == null) {
                return gameObject.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// Draws the UnityEvent property field with the correct indentation.
        /// </summary>
        /// <param name="property">The UnityEvent property.</param>
        public static void UnityEventPropertyField(SerializedProperty property)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * IndentWidth);
            EditorGUILayout.PropertyField(property);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Returns the active path that the save file window should start at.
        /// </summary>
        /// <returns>The name of the path to save the file in.</returns>
        public static string GetSaveFilePath()
        {
            var selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(selectedPath)) {
                selectedPath = "Assets";
            }

            return selectedPath;
        }


        /// <summary>
        /// Shows the add menu for the specified type
        /// </summary>
        /// <param name="type">The type that should be shown.</param>
        /// <param name="condition">Delegate which allows for custom conditions to be checked before the type is added.</param>
        /// <param name="addCallback">The callback when a type is selected from the menu.</param>
        public static void AddObjectType(Type type, Func<Type, bool> condition, GenericMenu.MenuFunction2 addCallback)
        {
            var typeWithCondition = new TypeWithCondition() { Type = type, Condition = condition };

            if (!s_ObjectTypes.TryGetValue(typeWithCondition, out var typeList)) {
                typeList = new List<Type>();

                // Search through all of the assemblies to find any types that derive from specified type.
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; ++i) {
                    var assemblyTypes = assemblies[i].GetTypes();
                    for (int j = 0; j < assemblyTypes.Length; ++j) {

                        var assemblyType = assemblyTypes[j];

                        // Must derive from specified type.
                        if (!type.IsAssignableFrom(assemblyType)) {
                            continue;
                        }

                        // Ignore abstract classes.
                        if (assemblyType.IsAbstract) {
                            continue;
                        }

                        //Check condition.
                        if (condition != null) {
                            if (condition.Invoke(assemblyType) == false) {
                                continue;
                            }
                        }

                        typeList.Add(assemblyType);
                    }
                }
                s_ObjectTypes.Add(typeWithCondition, typeList);
            }

            if (typeList == null || typeList.Count == 0) {
                return;
            }

            // All of the types have been retrieved. Show the menu.
            var addMenu = new GenericMenu();
            for (int i = 0; i < typeList.Count; ++i) {
                addMenu.AddItem(new GUIContent(Shared.Editor.Utility.EditorUtility.SplitCamelCase(typeList[i].Name)), false, addCallback, typeList[i]);
            }
            addMenu.ShowAsContext();
        }
    }
}