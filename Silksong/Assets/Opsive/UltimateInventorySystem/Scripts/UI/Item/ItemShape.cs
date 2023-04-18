/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Item Shape is used to define a shape and size of an item. usually used as an Attribute value.
    /// </summary>
    [Serializable]
    public class ItemShape
    {
        [Tooltip("The item shape size in units.")]
        [SerializeField] protected Vector2Int m_Size = Vector2Int.one;
        [Tooltip("Use a custom shape? If not the shape will be rectangle.")]
        [SerializeField] protected bool m_UseCustomShape;
        [Tooltip("An array of boolean used to create a custom shape.")]
        [SerializeField] internal bool[] m_CustomShape;
        [Tooltip("The anchor position of the shape.")]
        [SerializeField] protected Vector2Int m_Anchor;

        public Vector2Int Size => m_Size;
        public bool UseCustomShape => m_UseCustomShape;
        public int Rows => m_Size.y;
        public int Cols => m_Size.x;
        public int Count => m_Size.x * m_Size.y;
        public Vector2Int Anchor => m_Anchor;
        public int AnchorIndex => m_Anchor.y * m_Size.x + m_Anchor.x;

        /// <summary>
        /// Set the anchor position within the size.
        /// </summary>
        /// <param name="newAnchor">The new anchor point.</param>
        public bool IsAnchor(int x, int y)
        {
            return m_Anchor.x == x && m_Anchor.y == y;
        }

        /// <summary>
        /// Set the size.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        public void SetSize(int rows, int columns)
        {
            SetSize(new Vector2Int(columns, rows));
        }

        /// <summary>
        /// Set the size.
        /// </summary>
        /// <param name="newSize">The new size.</param>
        public void SetSize(Vector2Int newSize)
        {
            var previousSize = m_Size;
            m_Size = new Vector2Int(Mathf.Max(1, newSize.x), Mathf.Max(1, newSize.y));

            if (m_UseCustomShape == false) { return; }

            if (m_CustomShape == null) {
                m_CustomShape = new bool[m_Size.x * m_Size.y];
            } else if (previousSize.x != m_Size.x || previousSize.y != m_Size.y) {
                m_CustomShape = ResizeArray(m_CustomShape, previousSize, m_Size);
            }

            if (m_Anchor.x >= m_Size.x || m_Anchor.y >= m_Size.y) {
                m_Anchor = Vector2Int.zero;
            }

            if (m_CustomShape[AnchorIndex] == false) {
                m_CustomShape[AnchorIndex] = true;
            }
        }

        /// <summary>
        /// Set the anchor position within the size.
        /// </summary>
        /// <param name="newAnchor">The new anchor point.</param>
        public void SetAnchor(Vector2Int newAnchor)
        {
            if (IsIndexOccupied(newAnchor.x, newAnchor.y)) { m_Anchor = newAnchor; }
        }

        /// <summary>
        /// Set to use a custom shape?
        /// </summary>
        /// <param name="useCustomShape">Use a custom shape?</param>
        public void SetUseCustomShape(bool useCustomShape)
        {
            if (m_UseCustomShape == useCustomShape) { return; }

            m_UseCustomShape = useCustomShape;

            if (m_UseCustomShape == false) { return; }

            m_CustomShape = new bool[m_Size.x * m_Size.y];
            for (int i = 0; i < m_CustomShape.Length; i++) { m_CustomShape[i] = true; }

        }

        /// <summary>
        /// Starts at the top left corner and starts horizontally.
        /// </summary>
        /// <param name="x">The horizontal index.</param>
        /// <param name="y">The vertical index.</param>
        /// <returns></returns>
        public bool IsIndexOccupied(int x, int y)
        {
            if (m_Size.x < x || m_Size.y < y) { return false; }

            if (m_UseCustomShape == false) { return true; }

            return m_CustomShape[y * m_Size.x + x];
        }

        /// <summary>
        /// Resize the 2D array.
        /// </summary>
        /// <param name="original">The original array.</param>
        /// <param name="previousSize">The previous size.</param>
        /// <param name="newSize">The new size.</param>
        /// <typeparam name="T">The element type.</typeparam>
        /// <returns>The new resized array.</returns>
        protected T[] ResizeArray<T>(T[] original, Vector2Int previousSize, Vector2Int newSize)
        {
            var newArray = new T[newSize.x * newSize.y];
            int minRows = Math.Min(newSize.y, previousSize.y);
            int minCols = Math.Min(newSize.x, previousSize.x);
            for (int y = 0; y < minRows; y++) {
                for (int x = 0; x < minCols; x++) {

                    newArray[y * newSize.x + x] = original[y * previousSize.x + x];
                }
            }

            return newArray;
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return string.Format(m_UseCustomShape ? "[{0},{1}] Custom Shape" : "[{0},{1}]", m_Size.x, m_Size.y);
        }
    }
}
