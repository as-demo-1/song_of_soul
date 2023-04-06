using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBullect : MonoBehaviour
{
    // Start is called before the first frame update

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
        this.ShootDir = Dir;
        transform.eulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(Dir));
      
        Destroy(gameObject,7f);
    }
    
    void Update()
    {
        float movespeed = 1f;
        transform.position += ShootDir * movespeed * Time.deltaTime;
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
