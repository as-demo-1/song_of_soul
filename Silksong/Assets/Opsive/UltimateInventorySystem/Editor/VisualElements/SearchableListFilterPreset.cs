namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using UnityEngine;

    /// <summary>
    /// An object used to create filter and sort presets for the searchable list visual element.
    /// </summary>
    public class SearchableListFilterPreset : ScriptableObject
    {
        [Tooltip("The default search string.")]
        [SerializeField] protected string m_SearchString;
        [Tooltip("The default Sort option, indicated by index.")]
        [SerializeField] protected int m_SortOptionIndex;
        
        public string SearchString { get => m_SearchString; set => m_SearchString = value; }
        public int SortOptionIndex { get => m_SortOptionIndex; set => m_SortOptionIndex = value; }

        /// <summary>
        /// Returns whether the object is valid (the string search is done separately).
        /// This function can be overwritten to make custom searchable list filters with more advanced properties.
        /// </summary>
        /// <param name="obj">The object to check if valid.</param>
        /// <returns>True if the object is valid.</returns>
        public virtual bool IsValid(object obj)
        {
            return true;
        }
    }
}