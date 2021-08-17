using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class Grenade : MonoBehaviour
    {
        public Vector2 initialForce;
        public float timer = 1;
        public GameObject explosion;
        public float explosionTimer = 3;

        Rigidbody2D Rigidbody;

        void OnEnable()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        IEnumerator Start()
        {
            Rigidbody.AddForce(initialForce);
            yield return new WaitForSeconds(timer);
            var eGo = Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(eGo, explosionTimer);
            Destroy(gameObject);
        }
    }
}