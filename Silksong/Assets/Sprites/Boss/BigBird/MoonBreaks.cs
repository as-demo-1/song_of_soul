using UnityEngine;

namespace Sprites.Boss.BigBird
{
    public class MoonBreaks : MonoBehaviour
    {
       
        private BoxCollider2D _boxCollider2D;
        public Vector3 startloc;

        private Vector3 ShootDir;
        void Start()
        {
            Setup(GameObject.FindGameObjectWithTag("Player").transform.position + new Vector3(Random.Range(0,5),Random.Range(0,4),Random.Range(0,4)));
            _boxCollider2D = GetComponent<BoxCollider2D>();
        }

        public static float GetAngleFromVectorFloat(Vector3 dir)
        {
            dir = dir.normalized;
            float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (n < 0) n += 360;
            return n;
        }
        public void Setup(Vector3 Dir)
        {
            transform.eulerAngles = Dir;
      
            Destroy(gameObject,14f);
        }
    
        void Update()
        {
            float movespeed = 1f;
            transform.position += ShootDir * movespeed * Time.deltaTime;
           // transform.Rotate(Vector3.one * 4 * Time.deltaTime);
        }
    
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
            {
                //TODO::接入伤害系统
                Debug.Log("Damage");
            }
        }
    }
}