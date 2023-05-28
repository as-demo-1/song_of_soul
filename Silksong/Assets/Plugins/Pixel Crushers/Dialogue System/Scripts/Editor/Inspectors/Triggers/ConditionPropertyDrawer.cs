// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This custom drawer for Condition uses LuaConditionWizard.
    /// </summary>
    [CustomPropertyDrawer(typeof(Condition))]
    public class ConditionPropertyDrawer : PropertyDrawer
    {

        public static bool hideMainFoldout = false;

        private SerializedProperty luaConditionsProperty = null;
        private SerializedProperty questConditionsProperty = null;
        private SerializedProperty acceptedTagsProperty = null;
        private SerializedProperty acceptedGameObjectsProperty = null;

        private LuaConditionWizard luaConditionWizard = new LuaConditionWizard(EditorTools.selectedDatabase);
        private string currentLuaWizardContent = string.Empty;
        private float luaConditionWizardHeight = 0;
        private float luaFieldWidth = 0;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded) return height;
            FindProperties(property);
            luaConditionWizardHeight = luaConditionsProperty.isExpanded ? luaConditionWizard.GetHeight() : 0;
            height += luaConditionWizardHeight;
            height += GetTextAreaArrayHeight(luaConditionsProperty);
            height += GetQuestConditionsHeight(questConditionsProperty);
            height += GetArrayHeight(acceptedTagsProperty);
            height += GetArrayHeight(acceptedGameObjectsProperty);
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                EditorGUI.BeginProperty(position, label, property);

                var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

                if (hideMainFoldout)
                {
                    EditorGUI.LabelField(rect, "Required conditions:");
                }
                else
                {
                    property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, "Condition");
                }

                // Show last evaluation result:
                if (Application.isPlaying)
                {
                    var lastEvalProperty = property.FindPropertyRelative("lastEvaluationValue");
                    if (lastEvalProperty != null)
                    {
                        var originalColor = GUI.color;
                        GUIContent evalGuiContent = null;
                        switch ((Condition.LastEvaluationValue)lastEvalProperty.enumValueIndex)
                        {
                            case Condition.LastEvaluationValue.True:
                                GUI.color = Color.green;
                                evalGuiContent = new GUIContent("(Last Check: True)");
                                break;
                            case Condition.LastEvaluationValue.False:
                                GUI.color = Color.red;
                                evalGuiContent = new GUIContent("(Last Check: False)");
                                break;
                            default:
                                evalGuiContent = new GUIContent("(Last Check: None)");
                                break;
                        }
                        var evalSize = GUI.skin.label.CalcSize(evalGuiContent);
                        EditorGUI.LabelField(new Rect(position.x + position.width - evalSize.x, position.y, evalSize.x, EditorGUIUtility.singleLineHeight), evalGuiContent);
                        GUI.color = originalColor;
                    }
                }

                if (property.isExpanded)
                {
                    position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);

                    var oldIndentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;

                    FindProperties(property);

                    // Debugging (make sure using correct database):
                    //EditorGUI.ObjectField(new Rect(position.x + 100, position.y, position.width - 100, EditorGUIUtility.singleLineHeight), EditorTools.selectedDatabase, typeof(DialogueDatabase));

                    // Set up positioning variables:
                    var x = position.x;
                    var y = position.y + EditorGUIUtility.singleLineHeight;
                    var width = position.width;

                    EditorTools.SetInitialDatabaseIfNull();

                    rect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight + 2f);
                    y += rect.height;
                    luaConditionsProperty.isExpanded = EditorGUI.Foldout(rect, luaConditionsProperty.isExpanded, "Lua Conditions");

                    if (luaConditionsProperty.isExpanded)
                    {
                        rect = new Rect(x + 16f, y, 80f, EditorGUIUtility.singleLineHeight + 2f);
                        EditorGUI.LabelField(rect, "Size");
                        rect = new Rect(x + 96f, y, width - 96f, EditorGUIUtility.singleLineHeight + 2f);
                        y += rect.height;
                        var newArraySize = EditorGUI.IntField(rect, luaConditionsProperty.arraySize);
                        if (newArraySize != luaConditionsProperty.arraySize)
                        {
                            var oldSize = luaConditionsProperty.arraySize;
                            luaConditionsProperty.arraySize = newArraySize;
                            for (int i = oldSize; i < newArraySize; i++)
                            {
                                luaConditionsProperty.GetArrayElementAtIndex(i).stringValue = string.Empty;
                            }
                        }
                        for (int i = 0; i < luaConditionsProperty.arraySize; i++)
                        {
                            var labelGuiContent = new GUIContent("Element " + i);
                            EditorGUI.LabelField(new Rect(x + 16f, y, 80f, EditorGUIUtility.singleLineHeight), labelGuiContent);
                            luaFieldWidth = rect.width - 16f;
                            var element = luaConditionsProperty.GetArrayElementAtIndex(i);
                            var height = EditorTools.textAreaGuiStyle.CalcHeight(new GUIContent(element.stringValue), luaFieldWidth) + 2f;
                            rect = new Rect(x + 96f, y, width - 96f, height);
                            y += height;
                            element.stringValue = EditorGUI.TextArea(rect, element.stringValue, EditorTools.textAreaGuiStyle);
                        }
                    }

                    // Show Lua wizard if Lua Conditions is expanded:
                    if (luaConditionsProperty.isExpanded)
                    {
                        try
                        {
                            rect = new Rect(x, y, width, luaConditionWizardHeight);
                            luaConditionWizard.database = EditorTools.selectedDatabase;
                            if (luaConditionWizard.database != null)
                            {
                                if (!luaConditionWizard.IsOpen)
                                {
                                    luaConditionWizard.OpenWizard(string.Empty);
                                }
                                currentLuaWizardContent = luaConditionWizard.Draw(rect, new GUIContent("Lua Condition Wizard", "Use to add Lua conditions below"), currentLuaWizardContent);
                                if (!luaConditionWizard.IsOpen && !string.IsNullOrEmpty(currentLuaWizardContent))
                                {
                                    luaConditionsProperty.arraySize++;
                                    var luaElement = luaConditionsProperty.GetArrayElementAtIndex(luaConditionsProperty.arraySize - 1);
                                    luaElement.stringValue = currentLuaWizardContent;
                                    currentLuaWizardContent = string.Empty;
                                    luaConditionWizard.OpenWizard(string.Empty);
                                }
                            }
                            y += rect.height;
                        }
                        catch (System.Exception)
                        {
                            // Don't cause editor problems if Lua wizard has problems.
                        }
                    }

                    // Quest conditions:
                    rect = new Rect(x, y, width, GetQuestConditionsHeight(questConditionsProperty));
                    EditorGUI.PropertyField(rect, questConditionsProperty, true);
                    y += rect.height;

                    // Accepted tags:
                    rect = new Rect(x, y, width, GetArrayHeight(acceptedTagsProperty));
                    EditorGUI.PropertyField(rect, acceptedTagsProperty, true);
                    y += rect.height;

                    // Accepted GameObjects:
                    rect = new Rect(x, y, width, GetArrayHeight(acceptedGameObjectsProperty));
                    EditorGUI.PropertyField(rect, acceptedGameObjectsProperty, true);
                    y += rect.height;

                    EditorGUI.indentLevel = oldIndentLevel;
                }
            }
            catch (System.Exception)
            {
                // Don't throw error and cause problems with Unity editor.
            }
            finally
            {
                EditorGUI.EndProperty();
            }
        }

        private void FindProperties(SerializedProperty property)
        {
            luaConditionsProperty = property.FindPropertyRelative("luaConditions");
            questConditionsProperty = property.FindPropertyRelative("questConditions");
            acceptedTagsProperty = property.FindPropertyRelative("acceptedTags");
            acceptedGameObjectsProperty = property.FindPropertyRelative("acceptedGameObjects");
        }

        private float GetArrayHeight(SerializedProperty property)
        {
            return property.isExpanded
                ? ((2 + property.arraySize) * (EditorGUIUtility.singleLineHeight + 2f))
                    : EditorGUIUtility.singleLineHeight;
        }

        private float GetTextAreaArrayHeight(SerializedProperty property)
        {
            if (!property.isArray || !property.isExpanded) return EditorGUIUtility.singleLineHeight;
            float height = 2 * (EditorGUIUtility.singleLineHeight + 2f);
            for (int i = 0; i < property.arraySize; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                if (element == null) continue;
                if (luaFieldWidth == 0) luaFieldWidth = Screen.width - 34f;
                height += EditorTools.textAreaGuiStyle.CalcHeight(new GUIContent(element.stringValue), luaFieldWidth) + 2f;

            }
            return height;
        }

        private float GetQuestConditionsHeight(SerializedProperty questConditionsProperty)
        {
            var height = GetArrayHeight(questConditionsProperty);
            for (int i = 0; i < questConditionsProperty.arraySize; i++)
            {
                height += EditorGUIUtility.singleLineHeight;
                var questConditionProperty = questConditionsProperty.GetArrayElementAtIndex(i);
                if (questConditionProperty.FindPropertyRelative("checkQuestEntry").boolValue)
                {
                    height += EditorGUIUtility.singleLineHeight;
                }
            }
            return height;
        }

    }
}
