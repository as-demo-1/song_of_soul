using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaUrchin_Chain : Trigger2D_Touch
{
    //public Rigidbody2D rb;
    public float moveSpeed;
    public float maxRange;
    public void LockDown(Vector2 startPos,Vector2 endPos,float stopTime)
    {
        StartCoroutine(Moving(startPos, endPos, stopTime));
    }
    IEnumerator Moving(Vector2 startPos, Vector2 endPos, float stopTime)
    {
        transform.position = startPos;
        Vector2 dir = endPos - startPos;
        transform.right = -dir;
        //rb.velocity = -transform.right * moveSpeed;
        
        while (Vector2.Distance(transform.position, endPos) > 0.33f)
        {
            transform.position=Vector2.MoveTowards(transform.position,endPos,moveSpeed*Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(stopTime);
        //rb.velocity = -transform.right * moveSpeed;
        while (Vector2.Distance(transform.position, endPos) < maxRange)
        {
            transform.position = Vector2.MoveTowards(transform.position, (Vector2)transform.position+dir, moveSpeed * Time.deltaTime);
            yield return null;
        }
        gameObject.SetActive(false);
    }
    public void Stop()
    {
        //rb.velocity = Vector2.zero;
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision);
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            base.OnTriggerEnter2D(collision);
        }
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.transform.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
             
        }
    }
}
