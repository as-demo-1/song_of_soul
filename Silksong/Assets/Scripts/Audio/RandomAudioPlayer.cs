using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AS_2D.Audio
{
    /*解决方案一
     * 在一系列音效中随机播放
     * 来自 2d game kit
     */

    [RequireComponent(typeof(AudioSource))]
    public class RandomAudioPlayer : MonoBehaviour
    {
        public AudioClip[] clips;

        public bool randomizePitch = false;
        public float pitchRange = 0.2f;

        protected AudioSource m_Source;

        private void Awake()
        {
            m_Source = GetComponent<AudioSource>();
        }

        /// <summary>
        /// 随机播放clips中的音效
        /// </summary>
        public void PlayRandomSound()
        {
            AudioClip[] source = clips;

            int choice = Random.Range(0, source.Length);

            if(randomizePitch)
                m_Source.pitch = Random.Range(1.0f - pitchRange, 1.0f + pitchRange);

            m_Source.PlayOneShot(source[choice]);
        }

        public void Stop()
        {
            m_Source.Stop();
        }
    }
}