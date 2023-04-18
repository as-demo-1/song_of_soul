/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.Inspectors.Utility
{
    using Opsive.Shared.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws individual object values.
    /// </summary>
    public static class ObjectInspector
    {
        private static int s_KeybordControl;
        private static int s_ArraySize;
        private static bool s_EditingArray;
        private static int s_FocusHash;
        private static HashSet<int> s_DrawnObjects;
        private static string[] s_LayerNames;
        private static int[] s_MaskValues;

        /// <summary>
        /// Draws all of the serialized fields.
        /// </summary>
        /// <param name="obj">The object to draw all of the fields of.</param>
        /// <param name="drawNoFieldsNotice">Should a notice be drawn if no fields can be drawn?</param>
        /// <returns>The updated object value based on the drawn fields.</returns>
        public static object DrawFields(object obj, bool drawNoFieldsNotice)
        {
            return DrawFields(obj, drawNoFieldsNotice, 0);
        }

        /// <summary>
        /// Draws all of the serialized fields.
        /// </summary>
        /// <param name="obj">The object to draw all of the fields of.</param>
        /// <param name="drawNoFieldsNotice">Should a notice be drawn if no fields can be drawn?</param>
        /// <param name="hashPrefix">The prefix of the hash from the parent class. This value will prevent collisions with similarly named objects.</param>
        /// <returns>The updated object value based on the drawn fields.</returns>
        private static object DrawFields(object obj, bool drawNoFieldsNotice, int hashPrefix)
        {
            if (obj == null) {
                return null;
            }

            // Only the serialized objects need to be drawn.
            var objType = obj.GetType();
            var fieldsDrawn = false;
            var hash = hashPrefix + Serialization.StringHash(obj.GetType().FullName);
            var fields = Serialization.GetSerializedFields(objType, MemberVisibility.Public);
            for (int i = 0; i < fields.Length; ++i) {
                // Do not draw HideInInspector fields.
                if (TypeUtility.GetAttribute(fields[i], typeof(HideInInspector)) != null) {
                    continue;
                }
                EditorGUI.BeginChangeCheck();
                // Fields can have tooltips assocaited with them. Tooltips are visible when hovering over the field label within the inspector.
                GUIContent guiContent;
                var tooltip = GetTooltipAttribute(fields[i]);
                if (tooltip != null) {
                    guiContent = new GUIContent(Shared.Editor.Utility.EditorUtility.SplitCamelCase(fields[i].Name), tooltip.tooltip);
                } else {
                    guiContent = new GUIContent(Shared.Editor.Utility.EditorUtility.SplitCamelCase(fields[i].Name));
                }
                var value = fields[i].GetValue(obj);
                value = DrawObject(guiContent, fields[i].FieldType, obj, value, fields[i].Name, hash, null, null, fields[i], true);
                // Update the object if the value was changed.
                if (EditorGUI.EndChangeCheck()) {
                    fields[i].SetValue(obj, value);
                }
                fieldsDrawn = true;
            }
            // Draw a notice if no fields were drawn.
            if (drawNoFieldsNotice && !fieldsDrawn) {
                EditorGUILayout.LabelField("(No Visible Fields)");
            }

            return obj;
        }

        /// <summary>
        /// Draws all of the serialized properties.
        /// </summary>
        /// <param name="type">The type of object to draw.</param>
        /// <param name="obj">The drawn object. This value can be null (such as if it hasn't been serialized).</param>
        /// <param name="hashPrefix">The prefix of the hash from the parent class. This value will prevent collisions with similarly named objects.</param>
        /// <param name="valuePositionMap">A map between the value hash and the position within the positions array.</param>
        /// <param name="serialization">The serialized data.</param>
        /// <param name="visibility">Specifies the visibility of the properties that should be drawn.</param>
        /// <param name="startDrawElementCallback">Callback issued before the elements are drawn.</param>
        /// <param name="endDrawElementCallback">Callback issued after the elemts are drawn.</param>
        /// <returns>The updated object value based on the drawn properties.</returns>
        public static object DrawProperties(Type type, object obj, int hashPrefix, Dictionary<int, int> valuePositionMap, Serialization serialization, MemberVisibility visibility, Action startDrawElementCallback, Func<int, List<int>, bool> endDrawElementCallback)
        {
            // Only the serialized properties need to be drawn.
            var properties = Serialization.GetSerializedProperties(type, visibility);
            var bitwiseHash = new Version(serialization.Version).CompareTo(new Version("3.1")) >= 0;
            for (int i = 0; i < properties.Length; ++i) {
                int hash;
                if (bitwiseHash) {
                    hash = (hashPrefix * Serialization.HashMultiplier) ^ (Serialization.StringHash(properties[i].PropertyType.FullName) + Serialization.StringHash(properties[i].Name));
                } else {
                    hash = hashPrefix + Serialization.StringHash(properties[i].PropertyType.FullName) + Serialization.StringHash(properties[i].Name);
                }
                // The value may not be serialized.
                if (!valuePositionMap.ContainsKey(hash)) {
                    continue;
                }

                // Issue the start callback before the value is drawn.
                if (startDrawElementCallback != null) {
                    startDrawElementCallback();
                }
                var value = Serializer.BytesToValue(properties[i].PropertyType, properties[i].Name, valuePositionMap, hashPrefix, serialization.Values,
                                                    serialization.ValuePositions, serialization.UnityObjects, false, visibility, bitwiseHash);

                EditorGUI.BeginChangeCheck();

                // Get a list of Unity Objects before the property is drawn. This will be used if the property is deleted and the Unity Object array needs to be cleaned up.
                var unityObjectIndexes = new List<int>();
                Serialization.GetUnityObjectIndexes(ref unityObjectIndexes, properties[i].PropertyType, properties[i].Name, hashPrefix, valuePositionMap, serialization.ValueHashes, serialization.ValuePositions,
                                                    serialization.Values, false, visibility, bitwiseHash);

                // Draw the property.
                value = DrawObject(new GUIContent(Shared.Editor.Utility.EditorUtility.SplitCamelCase(properties[i].Name)), properties[i].PropertyType, obj, value, properties[i].Name, hashPrefix, valuePositionMap, serialization, properties[i], false);

                // Issue the end callback after the value is drawn.
                if (endDrawElementCallback != null) {
                    var elementRemoved = endDrawElementCallback(i, unityObjectIndexes);
                    if (elementRemoved) {
                        return obj;
                    }
                }

                if (EditorGUI.EndChangeCheck()) {
                    // If the hash prefix isn't zero then the object shouldn't be serialized immediately because it isn't a base property. Set the value of the parent object and mark the GUI as changed
                    // so it'll be serialized by the base property. For example, if ClassA exists and has properties One and Two. If the One value is changed the serializer should serialize the ClassA object
                    // rather then the One object.
                    if (hashPrefix != 0) {
                        GUI.changed = true;
                        var setMethod = properties[i].GetSetMethod();
                        if (setMethod != null) {
                            setMethod.Invoke(obj, new object[] { value });
                        }
                        return obj;
                    }

                    // Remove the current element and then add it back at the end. The order of the values doesn't matter and this prevents each subsequent element from needing to be modified because the current
                    // value could have changed sizes.
                    Serialization.RemoveProperty(i, unityObjectIndexes, serialization, visibility, bitwiseHash);

                    // Add the property to the Serialization data.
                    Serialization.AddProperty(properties[i], value, unityObjectIndexes, serialization, visibility);

                    break;
                }
            }
            return obj;
        }

        /// <summary>
        /// Draws the specified object.
        /// </summary>
        /// <param name="guiContent">The GUIContent to draw with the associated object.</param>
        /// <param name="type">The type of object to draw.</param>
        /// <param name="parent">The parent of the value.</param>
        /// <param name="value">The value of the object.</param>
        /// <param name="name">The name of the object being drawn.</param>
        /// <param name="hashPrefix">The prefix of the hash from the parent class. This value will prevent collisions with similarly named objects.</param>
        /// <param name="valuePositionMap">A map between the value hash and the position within the positions array.</param>
        /// <param name="serialization">The serialized data.</param>
        /// <param name="fieldProperty">A reference to the field or property that is being drawn.</param>
        /// <param name="drawFields">Should the fields be drawn? If false the properties will be drawn.</param>
        /// <returns>The drawn object.</returns>
        public static object DrawObject(GUIContent guiContent, Type type, object parent, object value, string name, int hashPrefix, Dictionary<int, int> valuePositionMap, Serialization serialization, object fieldProperty, bool drawFields)
        {
            if (typeof(IList).IsAssignableFrom(type)) {
                return DrawArrayObject(guiContent, type, parent, value, name, hashPrefix, valuePositionMap, serialization, fieldProperty, drawFields);
            } else {
                return DrawSingleObject(guiContent, type, value, name, hashPrefix, valuePositionMap, serialization, fieldProperty, drawFields);
            }
        }

        /// <summary>
        /// Draws the specified array object.
        /// </summary>
        /// <param name="guiContent">The GUIContent to draw with the associated object.</param>
        /// <param name="type">The type of object to draw.</param>
        /// <param name="parent">The parent of the value.</param>
        /// <param name="value">The value of the object.</param>
        /// <param name="name">The name of the object being drawn.</param>
        /// <param name="hashPrefix">The prefix of the hash from the parent class. This value will prevent collisions with similarly named objects.</param>
        /// <param name="valuePositionMap">A map between the value hash and the position within the positions array.</param>
        /// <param name="serialization">The serialized data.</param>
        /// <param name="fieldProperty">A reference to the field or property that is being drawn.</param>
        /// <param name="drawFields">Should the fields be drawn? If false the properties will be drawn.</param>
        /// <returns>The drawn object.</returns>
        private static object DrawArrayObject(GUIContent guiContent, Type type, object parent, object value, string name, int hashPrefix, Dictionary<int, int> valuePositionMap, Serialization serialization, object fieldProperty, bool drawFields)
        {
            // Arrays and lists operate differently when retrieving the element type.
            Type elementType;
            if (type.IsArray) {
                elementType = type.GetElementType();
            } else {
                var baseFieldType = type;
                while (!baseFieldType.IsGenericType) {
                    baseFieldType = baseFieldType.BaseType;
                }
                elementType = baseFieldType.GetGenericArguments()[0];
            }

            // Create the list value if it is null. The list cannot be null.
            IList list;
            if (value == null) {
                if (type.IsGenericType || type.IsArray) {
                    list = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType), true) as IList;
                } else {
                    list = Activator.CreateInstance(type, true) as IList;
                }
                if (type.IsArray) {
                    // Copy to an array so SetValue will accept the new value.
                    var array = Array.CreateInstance(elementType, list.Count);
                    list.CopyTo(array, 0);
                    list = array;
                }
            } else {
                list = (IList)value;
            }

            EditorGUILayout.BeginVertical();
            if (InspectorUtility.Foldout(list, guiContent)) {
                EditorGUI.indentLevel++;
                var hash = hashPrefix + Serialization.StringHash(type.FullName) + Serialization.StringHash(name);
                // s_EditingArray is static so s_FocusHash is necessary so the array that is being modified can be identified.
                if (s_EditingArray && s_FocusHash == hash && (s_KeybordControl != GUIUtility.keyboardControl || Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)) {
                    s_EditingArray = false;
                    var newArray = Array.CreateInstance(elementType, s_ArraySize);
                    var origElement = -1;
                    // Copy all of the old values to the new array.
                    for (int i = 0; i < s_ArraySize; ++i) {
                        if (i < list.Count) {
                            origElement = i;
                        }
                        if (origElement == -1) {
                            break;
                        }

                        var listValue = list[origElement];
                        if (i >= list.Count && !typeof(UnityEngine.Object).IsAssignableFrom(elementType) && !typeof(string).IsAssignableFrom(elementType)) {
                            // Do not copy by reference.
                            listValue = Activator.CreateInstance(list[origElement].GetType(), true);
                        }

                        newArray.SetValue(listValue, i);
                    }
                    if (type.IsArray) {
                        list = newArray;
                    } else {
                        if (type.IsGenericType) {
                            list = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType), true) as IList;
                        } else {
                            list = Activator.CreateInstance(type, true) as IList;
                        }
                        for (int i = 0; i < newArray.Length; ++i) {
                            list.Add(newArray.GetValue(i));
                        }
                    }
                    // Mark the GUI as changed so the object will be serialized.
                    GUI.changed = true;
                }
                var size = EditorGUILayout.IntField("Size", list.Count);
                if (size != list.Count) {
                    if (!s_EditingArray) {
                        s_EditingArray = true;
                        s_KeybordControl = GUIUtility.keyboardControl;
                        s_FocusHash = hash;
                    }
                    s_ArraySize = size;
                }

                hash = hashPrefix + Serialization.StringHash(type.FullName) + Serialization.StringHash(name);
                for (int i = 0; i < list.Count; ++i) {
                    GUILayout.BeginHorizontal();
                    if (list[i] == null && !typeof(UnityEngine.Object).IsAssignableFrom(elementType) && elementType != typeof(string)) {
                        list[i] = Activator.CreateInstance(elementType);
                    }
                    guiContent.text = "Element " + i;
                    list[i] = DrawObject(guiContent, elementType, parent, list[i], name, hash / (i + 2), valuePositionMap, serialization, fieldProperty, drawFields);
                    GUILayout.Space(6);
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            return list;
        }

        /// <summary>
        /// Draws the specified object.
        /// </summary>
        /// <param name="guiContent">The GUIContent to draw with the associated object.</param>
        /// <param name="type">The type of object to draw.</param>
        /// <param name="value">The value of the object.</param>
        /// <param name="name">The name of the object being drawn.</param>
        /// <param name="hashPrefix">The prefix of the hash from the parent class. This value will prevent collisions with similarly named objects.</param>
        /// <param name="valuePositionMap">A map between the value hash and the position within the positions array.</param>
        /// <param name="serialization">The serialized data.</param>
        /// <param name="fieldProperty">A reference to the field or property that is being drawn.</param>
        /// <param name="drawFields">Should the fields be drawn? If false the properties will be drawn.</param>
        /// <returns>The drawn object.</returns>
        private static object DrawSingleObject(GUIContent guiContent, Type type, object value, string name, int hashPrefix, Dictionary<int, int> valuePositionMap, Serialization serialization, object fieldProperty, bool drawFields)
        {
            if (type == typeof(int)) {
                return EditorGUILayout.IntField(guiContent, (int)value);
            }
            if (type == typeof(float)) {
                // The range attribute may be used instead.
                if (drawFields) {
                    var field = (System.Reflection.FieldInfo)fieldProperty;
                    var rangeAttribute = field.GetCustomAttributes(typeof(RangeAttribute), false) as RangeAttribute[];
                    if (rangeAttribute != null && rangeAttribute.Length > 0) {
                        return EditorGUILayout.Slider(guiContent, (float)value, rangeAttribute[0].min, rangeAttribute[0].max);
                    }
                }
                return EditorGUILayout.FloatField(guiContent, (float)value);
            }
            if (type == typeof(double)) {
                return EditorGUILayout.FloatField(guiContent, Convert.ToSingle((double)value));
            }
            if (type == typeof(long)) {
                return (long)EditorGUILayout.IntField(guiContent, Convert.ToInt32((long)value));
            }
            if (type == typeof(bool)) {
                return EditorGUILayout.Toggle(guiContent, (bool)value);
            }
            if (type == typeof(string)) {
                return EditorGUILayout.TextField(guiContent, (string)value);
            }
            if (type == typeof(byte)) {
                return Convert.ToByte(EditorGUILayout.IntField(guiContent, Convert.ToInt32(value)));
            }
            if (type == typeof(Vector2)) {
                return EditorGUILayout.Vector2Field(guiContent, (Vector2)value);
            }
            if (type == typeof(Vector3)) {
                return EditorGUILayout.Vector3Field(guiContent, (Vector3)value);
            }
            if (type == typeof(Vector4)) {
                return EditorGUILayout.Vector4Field(guiContent, (Vector4)value);
            }
            if (type == typeof(Quaternion)) {
                var quaternion = (Quaternion)value;
                var vectorValue = Vector4.zero;
                vectorValue.Set(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
                vectorValue = EditorGUILayout.Vector4Field(name, vectorValue);
                quaternion.Set(vectorValue.x, vectorValue.y, vectorValue.z, vectorValue.w);
                return quaternion;
            }
            if (type == typeof(Color)) {
                return EditorGUILayout.ColorField(guiContent, (Color)value);
            }
            if (type == typeof(Rect)) {
                return EditorGUILayout.RectField(guiContent, (Rect)value);
            }
            if (type == typeof(Matrix4x4)) {
                var matrix = (Matrix4x4)value;
                if (InspectorUtility.Foldout(value, guiContent)) {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < 4; ++i) {
                        for (int j = 0; j < 4; ++j) {
                            matrix[i, j] = EditorGUILayout.FloatField("E" + i.ToString() + j.ToString(), matrix[i, j]);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                return matrix;
            }
            if (type == typeof(AnimationCurve)) {
                if (value == null) {
                    value = AnimationCurve.EaseInOut(0, 0, 1, 1);
                    GUI.changed = true;
                }
                return EditorGUILayout.CurveField(guiContent, (AnimationCurve)value);
            }
            if (type == typeof(LayerMask)) {
                return DrawLayerMask(guiContent, (LayerMask)value);
            }
            if (type.IsEnum) {
                if (type.IsDefined(typeof(FlagsAttribute), true)) {
                    return EditorGUILayout.MaskField(guiContent, (int)value, Enum.GetNames(type));
                }
                return EditorGUILayout.EnumPopup(guiContent, (Enum)Enum.ToObject(type, value));
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) {
                return EditorGUILayout.ObjectField(guiContent, (UnityEngine.Object)value, type, true);
            }
            if (type.IsClass || (type.IsValueType && !type.IsPrimitive)) { // Classes and structs.
                if (typeof(Delegate).IsAssignableFrom(type)) { // Delegates are not supported.
                    return null;
                }
                if (s_DrawnObjects == null) {
                    s_DrawnObjects = new HashSet<int>();
                }
                // Do not endlessly loop on objects that have already been drawn.
                var hash = name.GetHashCode();
                if (s_DrawnObjects.Contains(hash)) {
                    return null;
                } else {
                    try { // Unity may throw an exception so catch it to clean up.
                        s_DrawnObjects.Add(hash);
                        GUILayout.BeginVertical();
                        if (value == null) {
                            value = Activator.CreateInstance(type);
                            GUI.changed = true;
                        }
                        if (InspectorUtility.Foldout(value, guiContent)) {
                            EditorGUI.indentLevel++;
                            var inspectorDrawer = InspectorDrawerUtility.InspectorDrawerForType(type);
                            if (inspectorDrawer != null) {
                                inspectorDrawer.OnInspectorGUI(value, null);
                            } else if (drawFields) {
                                value = DrawFields(value, true, hashPrefix + Serialization.StringHash(type.FullName) + Serialization.StringHash(name));
                            } else {
                                value = DrawProperties(type, value, hashPrefix + Serialization.StringHash(type.FullName) + Serialization.StringHash(name), valuePositionMap, serialization, MemberVisibility.Public, null, null);
                            }
                            EditorGUI.indentLevel--;
                        }
                        GUILayout.EndVertical();
                        s_DrawnObjects.Remove(hash);
                        return value;
                    } catch (Exception /*e*/) {
                        // Clean up any exceptions.
                        EditorGUI.indentLevel--;
                        GUILayout.EndVertical();
                        s_DrawnObjects.Remove(hash);
                        return null;
                    }
                }
            }
            Debug.LogWarning("Warning: unsupported value type: " + type);
            return null;
        }

        /// <summary>
        /// Draws a LayerMask.
        /// </summary>
        /// <param name="guiContent">The GUIContent to draw with the associated object.</param>
        /// <param name="layerMask">The LayerMask to draw.</param>
        /// <returns>The drawn layerMask object.</returns>
        private static LayerMask DrawLayerMask(GUIContent guiContent, LayerMask layerMask)
        {
            if (s_LayerNames == null) {
                InitLayers();
            }

            // Convert the layermask value to a mask value that does not include the layers with a blank name.
            var maskValue = 0;
            for (int i = 0; i < s_LayerNames.Length; ++i) {
                if ((layerMask.value & s_MaskValues[i]) == s_MaskValues[i]) {
                    maskValue |= 1 << i;
                }
            }

            var newMaskValue = EditorGUILayout.MaskField(guiContent, maskValue, s_LayerNames);

            // Convert the mask value back to the layermask value which does include blank names if the value was changed.
            if (newMaskValue != maskValue) {
                maskValue = 0;
                for (int i = 0; i < s_LayerNames.Length; ++i) {
                    if ((newMaskValue & (1 << i)) != 0) {
                        maskValue |= s_MaskValues[i];
                    }
                }
                layerMask.value = maskValue;
            }

            return layerMask;
        }

        /// <summary>
        /// Initializes the list of layers and masks within the project.
        /// </summary>
        private static void InitLayers()
        {
            var layers = new List<string>();
            var masks = new List<int>();
            for (int i = 0; i < 32; ++i) {
                var name = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(name)) {
                    layers.Add(name);
                    masks.Add(1 << i);
                }
            }
            s_LayerNames = layers.ToArray();
            s_MaskValues = masks.ToArray();
        }

        /// <summary>
        /// Returns the Tooltip attribute for the given field, if it exists.
        /// </summary>
        /// <param name="field">The field to get the Tooltip attribute of.</param>
        /// <returns>The Tooltip for the given field. If it does not exist then null is returned.</returns>
        public static TooltipAttribute GetTooltipAttribute(System.Reflection.FieldInfo field)
        {
            var tooltipAttributes = field.GetCustomAttributes(typeof(TooltipAttribute), false) as TooltipAttribute[];
            if (tooltipAttributes.Length > 0) {
                return tooltipAttributes[0];
            }
            return null;
        }
    }
}
