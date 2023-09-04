using UnityEngine;

namespace PixelCrushers.DialogueSystem.Demo
{

    /// <summary>
    /// This is a very simple example script that destroys a GameObject if
    /// it receives the message "TakeDamage(float)" or "Damage(float)". You
    /// can also assign an optional "corpse" prefab to replace the GameObject.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class DieOnTakeDamage : MonoBehaviour
    {

        public GameObject deadPrefab;

        void TakeDamage(float damage)
        {
            if (deadPrefab != null)
            {
                GameObject dead = Instantiate(deadPrefab, transform.position, transform.rotation) as GameObject;
                if (dead != null) dead.transform.parent = transform.parent;
            }
            Destroy(gameObject);
        }

        void Damage(float damage)
        {
            TakeDamage(damage);
        }

    }

}
