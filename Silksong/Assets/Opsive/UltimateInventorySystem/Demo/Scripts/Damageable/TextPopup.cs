/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.Damageable
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using System.Collections;
    using UnityEngine;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// Text pop up component.
    /// </summary>
    public class TextPopup : MonoBehaviour
    {
        [Tooltip("The text component.")]
        [SerializeField] protected Text m_Text;
        [Tooltip("The pop up life time before getting returned to the pool.")]
        [SerializeField] protected float m_LifeTime = 1;

        /// <summary>
        /// Pop the text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Pop(string text)
        {
            m_Text.text = text;
            if (gameObject.activeInHierarchy) {
                StartCoroutine(PopAnimate());
            } else {
                ObjectPool.Destroy(gameObject);
            }
        }

        /// <summary>
        /// Pop animation coroutine.
        /// </summary>
        /// <returns>The IEnumerator.</returns>
        protected IEnumerator PopAnimate()
        {
            var thirdLifeTime = m_LifeTime / 3f;
            var time = 0f;

            transform.localScale = Vector3.zero;
            while (time < thirdLifeTime) {
                time += Time.deltaTime;
                transform.localScale = new Vector3(time, time, time) * 3;
                yield return null;
            }

            yield return new WaitForSeconds(thirdLifeTime);

            while (time > 0) {
                time -= Time.deltaTime;
                transform.localScale = new Vector3(time, time, time) * 3;
                yield return null;
            }

            ObjectPool.Destroy(gameObject);
        }
    }
}