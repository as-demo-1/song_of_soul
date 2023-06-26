/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using System;
    using System.Reflection;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Attribute Binding is a class used to bind an attribute to a property.
    /// </summary>
    [Serializable]
    public abstract class AttributeBinding
    {
        [Tooltip("The attribute name.")]
        [SerializeField] protected string m_AttributeName;
        [Tooltip("The object bound to the attribute.")]
        [SerializeField] protected Object m_BoundComponent;
        [Tooltip("The object path to the property bound to the attribute.")]
        [SerializeField] protected string m_PropertyPath;

        protected PropertyInfo m_Property;

        public string AttributeName {
            get => m_AttributeName;
            set => m_AttributeName = value;
        }

        public Object BoundComponent {
            get => m_BoundComponent;
            set => m_BoundComponent = value;
        }

        public string PropertyPath {
            get => m_PropertyPath;
            set => m_PropertyPath = value;
        }

        /// <summary>
        /// Bind the attribute.
        /// </summary>
        /// <param name="attribute">The attribute to bind.</param>
        public abstract void BindAttribute(AttributeBase attribute);

        /// <summary>
        /// Create the property delegates.
        /// </summary>
        public void CreatePropertyDelegates()
        {
            m_Property = m_BoundComponent?.GetType().GetProperty(m_PropertyPath);
            if (m_Property == null) { return; }

            CreatePropertyDelegatesInternal(m_Property);
        }

        /// <summary>
        /// Create the property delegates.
        /// </summary>
        /// <param name="property">The property info.</param>
        protected abstract void CreatePropertyDelegatesInternal(PropertyInfo property);

        /// <summary>
        /// Unbind an attribute.
        /// </summary>
        public abstract void UnBindAttribute();
    }

    /// <summary>
    /// The Attribute Binding class used to bind attributes to properties.
    /// </summary>
    /// <typeparam name="T">The attribute type.</typeparam>
    [Serializable]
    public class AttributeBinding<T> : AttributeBinding
    {
        [NonSerialized] protected Attribute<T> m_Attribute;
        [NonSerialized] protected Action<T> m_Setter;
        [NonSerialized] protected Func<T> m_Getter;

        public Action<T> Setter => m_Setter;
        public Func<T> Getter => m_Getter;

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public AttributeBinding()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="boundObject">The object to bind.</param>
        /// <param name="path">The path of the property to bind.</param>
        public AttributeBinding(string name, Object boundObject, string path)
        {
            m_AttributeName = name;
            m_BoundComponent = boundObject;
            m_PropertyPath = path;
        }

        /// <summary>
        /// Create the property delegates.
        /// </summary>
        /// <param name="property">The property.</param>
        protected override void CreatePropertyDelegatesInternal(PropertyInfo property)
        {
            var setMethod = property.GetSetMethod(false);
            if (setMethod != null) {
                m_Setter = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), m_BoundComponent, setMethod);
            }

            var getMethod = property.GetGetMethod(false);
            if (getMethod != null) {
                m_Getter = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), m_BoundComponent, getMethod);
            }
        }

        /// <summary>
        /// The attribute to bind
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        public override void BindAttribute(AttributeBase attribute)
        {
            if (m_Property == null) { return; }
            BindAttribute(attribute as Attribute<T>);
        }

        /// <summary>
        /// Bind the attribute.
        /// </summary>
        /// <param name="attribute">The attribute to bind.</param>
        public void BindAttribute(Attribute<T> attribute)
        {
            if (m_Property == null) { return; }
            if (attribute == null) {
                UnBindAttribute();
                return;
            }
            m_Attribute = attribute;
            m_Attribute.Bind(this);
            Refresh();
        }

        /// <summary>
        /// Unbind an attribute.
        /// </summary>
        public override void UnBindAttribute()
        {
            if (m_Attribute == null) { return; }
            m_Attribute.Unbind(true);
            m_Attribute = null;
        }

        /// <summary>
        /// Refresh.
        /// </summary>
        public void Refresh()
        {
            if (m_Attribute == null) {
                return;
            }
            m_Setter?.Invoke(m_Attribute.GetUnboundValue());
        }
    }

    /// <summary>
    /// The attribute name binding. Used to bind attribute names to string properties.
    /// </summary>
    [Serializable]
    public class AttributeNameBinding : AttributeBinding
    {
        [NonSerialized] protected AttributeBase m_Attribute;
        [NonSerialized] protected Action<string> m_Setter;
        [NonSerialized] protected Func<string> m_Getter;

        public Action<string> Setter => m_Setter;
        public Func<string> Getter => m_Getter;

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public AttributeNameBinding()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="boundObject">The object to bind.</param>
        /// <param name="path">The path of the property to bind.</param>
        public AttributeNameBinding(string name, Object boundObject, string path)
        {
            m_AttributeName = name;
            m_BoundComponent = boundObject;
            m_PropertyPath = path;
        }

        /// <summary>
        /// Create the property delegates.
        /// </summary>
        /// <param name="property">The property.</param>
        protected override void CreatePropertyDelegatesInternal(PropertyInfo property)
        {
            var setMethod = property.GetSetMethod(false);
            if (setMethod != null) {
                m_Setter = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), m_BoundComponent, setMethod);
            }

            var getMethod = property.GetGetMethod(false);
            if (getMethod != null) {
                m_Getter = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), m_BoundComponent, getMethod);
            }
        }

        /// <summary>
        /// Bind attributes.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        public override void BindAttribute(AttributeBase attribute)
        {
            if (m_Property == null) { return; }
            if (attribute == null) { return; }
            m_Attribute = attribute;
            Refresh();
        }
        
        /// <summary>
        /// Unbind an attribute.
        /// </summary>
        public override void UnBindAttribute()
        {
            m_Attribute = null;
        }

        /// <summary>
        /// Refresh.
        /// </summary>
        public void Refresh()
        {
            m_Setter?.Invoke(m_Attribute.Name);
        }
    }

}