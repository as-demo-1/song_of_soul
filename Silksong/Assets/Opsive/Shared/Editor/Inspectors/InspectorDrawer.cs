/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.Inspectors
{
    using System;

    /// <summary>
    /// InspectorDrawers allow non-Unity Objects to draw custom objects to the editor inspector.
    /// </summary>
    public abstract class InspectorDrawer
    {
        /// <summary>
        /// Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public abstract void OnInspectorGUI(object target, UnityEngine.Object parent);
    }

    /// <summary>
    /// Specifies the type of object the Inspector Drawer should belong to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class InspectorDrawerAttribute : Attribute
    {
        private Type m_Type;
        public Type Type { get { return m_Type; } }

        public InspectorDrawerAttribute(Type type)
        {
            m_Type = type;
        }
    }
}