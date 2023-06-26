/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Audio
{
    using UnityEngine;

    /// <summary>
    /// Container class for playing background music.
    /// </summary>
    public class AudioMusic : MonoBehaviour
    {
        [Tooltip("The music that should be playd.")]
        [SerializeField] protected AudioClipSet m_AudioClipSet;

        /// <summary>
        /// Plays the music.
        /// </summary>
        [ContextMenu("Play")]
        public void Play()
        {
            m_AudioClipSet.PlayAudioClip(gameObject, true);
        }

        /// <summary>
        /// Stops playing the music.
        /// </summary>
        [ContextMenu("Stop")]
        public void Stop()
        {
            m_AudioClipSet.Stop(gameObject);
        }
    }
}