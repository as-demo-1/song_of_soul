/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Audio;
using UnityEngine.Serialization;

namespace Opsive.UltimateInventorySystem.UI.Audio
{
    using UnityEngine;

    /// <summary>
    /// A scriptable object used to map categories to Item Actions.
    /// </summary>
    [CreateAssetMenu(fileName = "ButtonAudio", menuName = "Ultimate Inventory System/UI/Button Audio", order = 51)]
    public class ButtonAudio : ScriptableObject
    {
        [FormerlySerializedAs("m_Click")]
        [Tooltip("The audio of clicking a button.")]
        [SerializeField] protected AudioClip m_ClickClip;
        [Tooltip("The audio of clicking a button.")]
        [SerializeField] protected AudioConfig m_ClickConfig;
        [FormerlySerializedAs("m_Select")]
        [Tooltip("The audio of selecting a button.")]
        [SerializeField] protected AudioClip m_SelectClip;
        [Tooltip("The audio of clicking a button.")]
        [SerializeField] protected AudioConfig m_SelectConfig;

        public AudioClip ClickClipClip => m_ClickClip;
        public AudioClip SelectClipClip => m_SelectClip;
        
        public AudioConfig ClickConfig => m_ClickConfig;
        public AudioConfig SelectConfig => m_SelectConfig;
    }
}