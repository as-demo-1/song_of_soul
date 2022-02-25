using UnityEngine;



public class MovingPlatform : MonoBehaviour
{
    public float speed;
    public float waitTime;
    public Transform[] movePoint;
    public bool isActive;
    private int idx; // movePoint's index
    private bool flag;
    private Transform playerTransform;
    private float offset;
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
        Vector3 position = transform.position;
        transform.position = Vector2.MoveTowards(transform.position, movePoint[idx].position, speed * Time.deltaTime);
        offset = transform.position.x - position.x;
        if (flag)
        {
            playerTransform.position = (new Vector2(playerTransform.position.x + offset,playerTransform.position.y));
        }
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
        if (collision.transform.CompareTag("Player"))
        {
            flag = true;
            playerTransform = collision.transform;
        }
      
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            flag = false;
        }
    }
}
