/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.DataStructures
{
    using Opsive.Shared.Utility;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The base class of amounts array of an object.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <typeparam name="Ta">The object amount type.</typeparam>
    [System.Serializable]
    public class ObjectAmounts<T, Ta> : IReadOnlyList<Ta>, IObjectAmounts<Ta> where Ta : IObjectAmount<T> where T : class
    {
        [Tooltip("The array of object amount.")]
        [SerializeField] protected Ta[] m_Array;

        public Ta[] Array => m_Array;

        public int Count => m_Array.Length;
        public virtual Ta this[int index] {
            get => m_Array[index];
            set => m_Array[index] = value;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ObjectAmounts()
        {
            m_Array = new Ta[0];
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="arrayStartSize">The array start size.</param>
        public ObjectAmounts(int arrayStartSize)
        {
            m_Array = new Ta[arrayStartSize];
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="array">The array.</param>
        public ObjectAmounts(Ta[] array)
        {
            m_Array = array;
        }

        /// <summary>
        /// Add an item to the array.
        /// </summary>
        /// <param name="item">The item that will be added to the array.</param>
        /// <return>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection.</return>
        public virtual int Add(Ta item)
        {
            if (m_Array == null) {
                m_Array = new Ta[1];
            } else {
                System.Array.Resize(ref m_Array, m_Array.Length + 1);
            }

            m_Array[m_Array.Length - 1] = item;
            return m_Array.Length - 1;
        }

        /// <summary>
        /// Removes the item at the index provided from the array by shifting the content and reducing the count.
        /// </summary>
        /// <param name="index">The index of the item that should be removed.</param>
        public virtual void RemoveAt(int index)
        {
            // Shift all of the array elements down one.
            for (int j = index; j < m_Array.Length - 1; ++j) {
                m_Array[j] = m_Array[j + 1];
            }

            System.Array.Resize(ref m_Array, m_Array.Length - 1);
        }
        
        /// <summary>
        /// Reset the array to 0.
        /// </summary>
        public virtual void Clear()
        {
            System.Array.Resize(ref m_Array, 0);
        }

        /// <summary>
        /// Index of the object amount that matches the object.
        /// </summary>
        /// <param name="newArraySize">The object to search for.</param>
        public virtual int IndexOf(T obj)
        {
            if (m_Array == null) { return -1; }
            for (int i = 0; i < m_Array.Length; i++) {
                if (m_Array[i].Object == obj) { return i; }
            }

            return -1;
        }

        /// <summary>
        /// Resize the array.
        /// </summary>
        /// <param name="newArraySize">The new size.</param>
        public virtual void Resize(int newArraySize)
        {
            System.Array.Resize(ref m_Array, newArraySize);
        }

        /// <summary>
        /// Get the enumerator.
        /// </summary>
        /// <returns>The IEnumerator.</returns>
        public IEnumerator<Ta> GetEnumerator()
        {
            for (int i = 0; i < Count; i++) {
                yield return m_Array[i];
            }
        }

        /// <summary>
        /// Get the enumerator.
        /// </summary>
        /// <returns>The IEnumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns a readable format of the array. 
        /// </summary>
        /// <returns>The names and amounts of the objects in the list.</returns>
        public override string ToString()
        {
            if (m_Array == null || m_Array.Length == 0) { return "(none)"; }
            var s = string.Empty;
            for (int i = 0; i < m_Array.Length; i++) {
                s += string.Format("{2}{0} x{1} ",
                    m_Array[i].Object != null ? m_Array[i].Object.ToString() : "(none)",
                    m_Array[i].Amount, i != 0 ? "| " : "");
            }
            return s;
        }

        public static implicit operator Ta[](ObjectAmounts<T, Ta> x) => x?.m_Array;
        public static implicit operator ListSlice<Ta>(ObjectAmounts<T, Ta> x) => (x, 0, x.Count);
    }
}