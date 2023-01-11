using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript_Straight : MonoBehaviour
{
    [SerializeField]
    float startspeed = 0f;

    [SerializeField]
    float targetSpeed = 10f;

    [SerializeField]
    float speedUpTime;

    Collider2D coll;
    Transform bulletTransform;
    float currentSpeed;
    float smoothSpeed;
    

    private void Start()
    {
        bulletTransform = transform;
        coll = GetComponent<Collider2D>();
        currentSpeed = startspeed;  
    }

    private void Update()
    {
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref smoothSpeed, Time.deltaTime * speedUpTime); //平滑加速
        Move();
    }

    private void Move()
    {

        bulletTransform.Translate(Vector2.right * currentSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //玩家受击
            Debug.Log("玩家受击");
            Destroy(this.gameObject);
        }
        if (collision.gameObject.layer == 6)
        {
            Debug.Log("Bullet Hit Ground");
            Destroy(this.gameObject);
        }
    }
}

