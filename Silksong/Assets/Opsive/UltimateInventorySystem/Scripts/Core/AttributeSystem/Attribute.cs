/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.AttributeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// Generic class for attributes.
    /// </summary>
    /// <typeparam name="T">The type of the attribute value.</typeparam>
    [System.Serializable]
    [UnityEngine.Scripting.Preserve]
    public class Attribute<T> : AttributeBase//, IEquatable<Attribute<T>>
    {
        [Tooltip("The override value of this attribute.")]
        [SerializeField] protected T m_OverrideValue;
        [Tooltip("The pre-evaluated value used to improve performance by pre-computing attributes in the editor.")]
        [SerializeField] protected T m_PreEvaluatedValue;

        [Tooltip("The Collection that contains this attribute.")]
        [System.NonSerialized] protected AttributeBinding<T> m_Binding;

        /// <summary>
        /// Returns the override value.
        /// </summary>
        public virtual T OverrideValue => m_OverrideValue;

        #region Initialize

        /// <summary>
        /// Default constructor, required by the Editor, the extensions must also have one.
        /// </summary>
        public Attribute() : base("NewAttribute", VariantType.Override, "")
        {
            m_OverrideValue = default(T);
        }

        /// <summary>
        /// Constructor with a value for the override value, the extensions must have the same constructor, with the same order of parameter.
        /// The reason is that we use reflection to create attributes at runtime.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="variantType">The variant Type of the attribute.</param>
        /// <param name="overrideValue">The override value of the attribute.</param>
        /// <param name="modifyExpression">The modify expression of the attirbute.</param>
        public Attribute(string name, T overrideValue = default(T), VariantType variantType = VariantType.Override, string modifyExpression = "")
            : base(name, variantType, modifyExpression)
        {
            m_OverrideValue = overrideValue;
        }

        /// <summary>
        /// Duplicate the attribute using reflection.
        /// </summary>
        /// <returns>Returns a attribute identical the this one.</returns>
        public override AttributeBase Duplicate()
        {
            Attribute<T> duplicateAttribute;
            var attributeType = GetType();
            if (attributeType == typeof(Attribute<T>)) {
                duplicateAttribute = new Attribute<T>(Name, OverrideValue, m_VariantType, ModifyExpression);
            } else {
                //Use reflection if the type is inherited
                duplicateAttribute = Activator.CreateInstance(attributeType,
                    BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance | BindingFlags.OptionalParamBinding, null,
                    new object[] { Name, OverrideValue, m_VariantType, ModifyExpression },
                    CultureInfo.CurrentCulture) as Attribute<T>;
                
            }
            
            duplicateAttribute.m_IsPreEvaluated = m_IsPreEvaluated;
            duplicateAttribute.m_PreEvaluatedValue = m_PreEvaluatedValue;

            return duplicateAttribute;
        }

        public override AttributeBase NewCopy(string name, VariantType variantType)
        {
            var attributeType = GetType();
            if (attributeType == typeof(Attribute<T>)) {
                return new Attribute<T>(name, default, variantType);
            } else {
                return AttributeBase.CreateInstance(attributeType, name, Type.Missing, variantType);
            }
        }

        #endregion

        /// <summary>
        /// Set the override value as an object without notifying external listeners.
        /// </summary>
        /// <param name="overrideValue">The new override value.</param>
        internal override void SetOverrideValueAsObjectWithoutNotify(object overrideValue)
        {
            if (!(overrideValue is T value)) {
                value = (T)Convert.ChangeType(overrideValue, typeof(T));
            }
            
            m_OverrideValue = value;
        }

        /// <summary>
        /// Set the override value as an object.
        /// </summary>
        /// <param name="overrideValue">The override value.</param>
        /// <param name="setVariantTypeToOverride">Should the variant type be set to override?</param>
        /// <param name="reevaluate">Should the attribute re-evaluate.</param>
        public override void SetOverrideValueAsObject(object overrideValue,
            bool setVariantTypeToOverride = true, bool reevaluate = false)
        {

            if (!(overrideValue is T value)) {

                //Flag Enum values cannot be converted the same way as the rest.
                Type type = typeof(T);
                if (type.IsEnum) {
                    value = (T) Enum.ToObject(type, overrideValue);
                } else {
                    var changeType = Convert.ChangeType(overrideValue, typeof(T));
                    if (changeType is T convertedValue) {
                        value = convertedValue;
                    } else if (overrideValue == null) {
                        value = default(T);
                    } else{
                        Debug.LogWarning($"Cannot convert object '{overrideValue}' to type '{type}'");
                        return;
                    }
                    
                }
            }

            SetOverrideValue(value, setVariantTypeToOverride, reevaluate);
        }

        /// <summary>
        /// Set the override value of the attribute.
        /// </summary>
        /// <param name="overrideValue">The override value.</param>
        /// <param name="setVariantTypeToOverride">Should the variant type be set to override?</param>
        /// <param name="reevaluate">Should the attribute re-evaluate.</param>
        public void SetOverrideValue(T overrideValue, bool setVariantTypeToOverride = true, bool reevaluate = false)
        {

            var canSet = IsMutable;

#if UNITY_EDITOR
            if (!Application.isPlaying) { canSet = true; }
#endif

            if (!canSet) {
                Debug.LogWarning("You are not allowed to change the overrideValue of an Immutable Attribute.");
                return;
            }

            m_OverrideValue = overrideValue;
            if (setVariantTypeToOverride) { SetVariantType(VariantType.Override); }

            if (reevaluate) {
                ReevaluateValue(true);
            } else {
                m_IsPreEvaluated = false;
            }

            NotifyChange();
        }

        /// <summary>
        /// Notify that the attribute value changed.
        /// </summary>
        public override void NotifyChange()
        {
            base.NotifyChange();
            m_Binding?.Refresh();
        }

        #region values
        /// <summary>
        /// Get the inherited value.
        /// </summary>
        /// <returns>Returns the inherited value.</returns>
        public virtual T GetInheritedValue()
        {
            if (Application.isPlaying) {
                return GetInheritedValueInternal(this, true);
            }

            return GetInheritedValueInternal(this, false);
        }

        /// <summary>
        /// Get the inherited value, returns default(T) if it does not exist.
        /// </summary>
        /// <param name="relativeAttribute">The relative attribute (the first attribute in case of a recursive inherit).</param>
        /// <param name="checkIfPreEvaluated">Use the pre-evaluated value if it exists?</param>
        /// <returns>Returns the inherit attribute value.</returns>
        protected virtual T GetInheritedValueInternal(Attribute<T> relativeAttribute, bool checkIfPreEvaluated)
        {
            var inheritAttribute = GetInheritAttribute();
            if (inheritAttribute == null) { return default(T); }
            var inheritAttributeT = inheritAttribute as Attribute<T>;
            if (inheritAttributeT == null) {
                Debug.LogError("Attribute should never be able to inherit from a different type.");
                return default(T);
            }
            return inheritAttributeT.GetValueInternal(relativeAttribute, checkIfPreEvaluated, true);
        }

        /// <summary>
        /// Try get the modifyValue.
        /// </summary>
        /// <param name="modifyValue">Outputs the modifyValue, it will output a default value if the evaluation fails.</param>
        /// <returns>Returns false if the evaluation of the expression fails.</returns>
        public virtual bool TryGetModifyValue(out T modifyValue)
        {
            return TryGetModifyValueInternal(this, out modifyValue);
        }

        /// <summary>
        /// Try get the modifyValue.
        /// </summary>
        /// <param name="relativeAttribute">The relative attribute (the first attribute in case of a recursive inherit).</param>
        /// <param name="modifyValue">Outputs the modifyValue, it will output a default value if the evaluation fails.</param>
        /// <returns>Returns false if the evaluation of the expression fails.</returns>
        protected virtual bool TryGetModifyValueInternal(Attribute<T> relativeAttribute, out T modifyValue)
        {
            return ModifyFunction(m_ModifyExpression, m_OverrideValue, GetInheritedValue(), relativeAttribute, null, out modifyValue);
        }

        /// <summary>
        /// Returns the type of the value of the attribute.
        /// </summary>
        /// <returns>Returns the type of the attribute value.</returns>
        public override Type GetValueType()
        {
            return typeof(T);
        }

        /// <summary>
        /// Static function to get the attribute value Type.
        /// </summary>
        /// <returns>Returns the type of the attribute value.</returns>
        public static Type GetAttributeValueType()
        {
            return typeof(T);
        }

        /// <summary>
        /// Returns the modify value as an object.
        /// </summary>
        /// <param name="modifyValue">Outputs the modify value as an object.</param>
        /// <returns>Returns false if the modify expression failed the evaluation.</returns>
        public override bool TryGetModifyValueAsObject(out object modifyValue)
        {
            var result = TryGetModifyValue(out var modifyValueT);
            modifyValue = modifyValueT;
            return result;
        }
        /// <summary>
        /// Bind the attribute to the binding.
        /// </summary>
        /// <param name="binding">the attribute binding.</param>
        public override void Bind(AttributeBinding binding)
        {
            Unbind(true);
            
            if (!(binding is AttributeBinding<T> bindingT)) {
                return;
            }

            m_Binding = bindingT;
        }

        /// <summary>
        /// Unbind the attribute.
        /// </summary>
        public override void Unbind(bool applyValueToBinding)
        {
            if (m_Binding == null) { return; }

            var bindingValue = m_Binding.Getter.Invoke();
            
            m_Binding = null;
            
            if (!GetValue().Equals(bindingValue)) {
                SetOverrideValue(bindingValue);
            }
        }

        /// <summary>
        /// Returns the inherit value as an object.
        /// </summary>
        /// <returns>Returns the inherit value as an object.</returns>
        public override object GetInheritValueAsObject()
        {
            return GetInheritedValue();
        }

        /// <summary>
        /// Returns the override value as an object.
        /// </summary>
        /// <returns>Returns the override value as an object.</returns>
        public override object GetOverrideValueAsObject()
        {
            return OverrideValue;
        }

        /// <summary>
        /// Returns the actual value as an object.
        /// </summary>
        /// <returns>Returns the actual value as an object.</returns>
        public override object GetValueAsObject()
        {
            return GetValue();
        }

        /// <summary>
        /// Returns the actual value as an object.
        /// </summary>
        /// <returns>Returns the actual value as an object.</returns>
        public override object GetVariantValueAsObject()
        {
            return GetVariantValue();
        }

        /// <summary>
        /// Reevaluates the attribute value and return the result, Only use this function if re-evaluation is required.
        /// If that is not the case use GetValue().
        /// </summary>
        public override void ReevaluateValue(bool setAsPreEvaluated)
        {
            ReevaluateReturnValue(setAsPreEvaluated);
        }

        /// <summary>
        /// Reevaluates the attribute value and return the result, Only use this function if re-evaluation is required.
        /// If that is not the case use GetValue().
        /// </summary>
        /// <returns>Returns the evaluated value.</returns>
        public virtual T ReevaluateReturnValue(bool setAsPreEvaluated)
        {
            var newValue = GetVariantValue();
            
            //if the binding is not null, set the bound value as the attribute value.
            if (m_Binding != null) {
                
                var bindingValue = m_Binding.Getter.Invoke();

                if (!newValue.Equals(bindingValue)) {
                    SetOverrideValue(bindingValue);
                }

                newValue = bindingValue;
            }
            
            bool notifyChange = !EqualityComparer<T>.Default.Equals(m_PreEvaluatedValue, newValue);

            m_PreEvaluatedValue = newValue;
            if (setAsPreEvaluated) {
                if (!m_IsPreEvaluated) { notifyChange = true; }
                m_IsPreEvaluated = true;
            }

            if (notifyChange) {
                NotifyChange();
            }

            return m_PreEvaluatedValue;
        }

        /// <summary>
        /// Returns the value of this attribute.
        /// </summary>
        /// <returns>Return the value of the attribute.</returns>
        public virtual T GetValue()
        {
            return GetValueInternal(this, true, true);
        }

        /// <summary>
        /// Returns the value of this attribute depending on its variant type.
        /// This will NOT check the PreEvaluated Value (Use GetValue unless you want to reEvaluate the value).
        /// </summary>
        /// <returns>Return the attribute value.</returns>
        public virtual T GetVariantValue()
        {
            return GetValueInternal(this, false, false);
        }

        /// <summary>
        /// Get the value of the attribute as if it was not bound to a property.
        /// </summary>
        /// <returns>The unbound attribute value.</returns>
        public virtual T GetUnboundValue()
        {
            return GetValueInternal(this, true, false);
        }

        /// <summary>
        /// Returns the value of this attribute by choosing between the inherit, override or modify value.
        /// </summary>
        /// <param name="relativeAttribute">The relative attribute (the first attribute in case of a recursive inherit).</param>
        /// <param name="checkIfPreEvaluated">Use the pre-evaluated value if it exists.</param>
        /// <param name="checkIfBound">Use the property bound to the attribute if it exists.</param>
        /// <returns>Returns the attribute value.</returns>
        protected virtual T GetValueInternal(Attribute<T> relativeAttribute, bool checkIfPreEvaluated, bool checkIfBound)
        {
            if (checkIfBound && m_Binding?.Getter != null) { return m_Binding.Getter.Invoke(); }

            if (checkIfPreEvaluated && m_IsPreEvaluated && VariantType != VariantType.Override) {
                return m_PreEvaluatedValue;
            }

            switch (m_VariantType) {
                case VariantType.Override: {
                        return m_OverrideValue;
                    }
                case VariantType.Inherit: {
                        return GetInheritedValueInternal(relativeAttribute, checkIfPreEvaluated);
                    }
                case VariantType.Modify: {
                        TryGetModifyValueInternal(relativeAttribute, out var modifyValue);
                        return modifyValue;
                    }
                default: {
                        Debug.LogErrorFormat("VariantType is not set to any of the available values, it's value is {0}", m_VariantType);
                        return default(T);
                    }
            }
        }

        #endregion

        /// <summary>
        /// Returns a value for for any modify expression by using the parameters provided.
        /// </summary>
        /// <param name="modifyExpression">The modify Expression that will be evaluated.</param>
        /// <param name="value">The current attribute value.</param>
        /// <param name="parentValue">The parent attribute value.</param>
        /// <param name="relativeAttribute">The relative attribute (the first attribute in case of a recursive inherit).</param>
        /// <param name="otherAttributes">The attributes in scope which can be referenced as parameters in the expression.</param>
        /// <param name="modifyValue">Output of the value evaluated.</param>
        /// <returns>Returns success or fail message.</returns>
        public virtual bool ModifyFunction(string modifyExpression, T value, T parentValue, Attribute<T> relativeAttribute, IReadOnlyDictionary<string, AttributeBase> otherAttributes, out T modifyValue)
        {
            // string type special case
            if (typeof(T) == typeof(string)) {
                if (string.IsNullOrEmpty(modifyExpression)) {
                    // The expression is empty.
                    modifyValue = (T)Convert.ChangeType((parentValue + " " + value), typeof(string));
                    return false;
                }
                var (result, message) = ExpressionRegexReplaceAttributes(modifyExpression, relativeAttribute, otherAttributes, out var modifyValueString);

                modifyValue = (T)Convert.ChangeType(modifyValueString, typeof(string));
                return result;
            }

            // Other cases.
            var (evaluationResult, evaluationMessage) = ValueTypeDefaultModifyFunction(
                this, modifyExpression, parentValue, relativeAttribute, otherAttributes, out var evaluatedValue);

            if (evaluationResult) {

                if (evaluatedValue.GetType() == typeof(T)) {
                    modifyValue = (T)evaluatedValue;
                    return true;
                }

                var parsedValue = Convert.ChangeType(evaluatedValue, typeof(T));
                if (parsedValue.GetType() == typeof(T)) {
                    modifyValue = (T)parsedValue;
                    return true;
                }
            }

            if (value != null) {
                modifyValue = value;
            } else if (parentValue != null) {
                modifyValue = parentValue;
            } else {
                modifyValue = default(T);
            }

            if (string.IsNullOrWhiteSpace(modifyExpression) == false) {
                Debug.LogWarning($"Expression Evaluation Error: failed to parse result of expression: {modifyExpression}; evaluated to: {evaluatedValue}; Details: {evaluationMessage}");
            }

            return false;
        }

        #region Equality

        /// <summary>
        /// Is the value of this attribute equivalent to another.
        /// </summary>
        /// <param name="otherValue">The other value.</param>
        /// <returns>Returns true if equivalent.</returns>
        public virtual bool ValueEquivalentTo(T otherValue)
        {
            return GetValue().Equals(otherValue);
        }

        /// <summary>
        /// Is the value of this attribute equivalent to another.
        /// </summary>
        /// <param name="otherValue">The other value as an object.</param>
        /// <returns>Returns true if equivalent.</returns>
        public override bool ValueEquivalentTo(object otherValue)
        {
            return ValueEquivalentTo((T)otherValue);
        }

        /// <summary>
        /// Check if the attributes are equivalent.
        /// </summary>
        /// <param name="other">The other attribute.</param>
        /// <returns>True if equivalent.</returns>
        public override bool AreEquivalent(AttributeBase other)
        {
            if (ReferenceEquals(null, other)) { return false; }

            if (ReferenceEquals(this, other)) { return true; }

            if (other.GetValueType() != typeof(T)) { return false; }

            return AreEquivalent((Attribute<T>)other);
        }

        /// <summary>
        /// Check if the attributes are equivalent.
        /// </summary>
        /// <param name="other">The other attribute.</param>
        /// <returns>True if equivalent.</returns>
        public virtual bool AreEquivalent(Attribute<T> other)
        {
            if (ReferenceEquals(null, other)) { return false; }

            if (ReferenceEquals(this, other)) { return true; }

            return base.AreEquivalent(other) && EqualityComparer<T>.Default.Equals(m_OverrideValue, other.m_OverrideValue);
        }

        #endregion
    }
}