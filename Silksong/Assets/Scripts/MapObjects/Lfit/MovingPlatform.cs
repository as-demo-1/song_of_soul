using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;


public class MovingPlatform : MonoBehaviour
{
    public float speed;
    public float waitTime;
    public Transform[] movePoint;
    public bool isActive;
    private int idx; // movePoint's index
    void Start()
    {
        idx = 0;
    }


    void Update()
    {
        if (isActive)
        {
            Move();
        }
        
    }


    void Move() 
    {
        transform.position = Vector2.MoveTowards(transform.position, movePoint[idx].position, speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, movePoint[idx].position) < 0.1f)
        {
            if (movePoint.Length == 1) return;
            if (waitTime < 0.0f)
            {
                idx = idx == 1 ? 0 : 1;
                waitTime = 0.5f;  //reset waitTime
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.transform.SetParent(this.transform);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        collision.transform.SetParent(null);
        DontDestroyOnLoad(collision.gameObject);
    }
}
