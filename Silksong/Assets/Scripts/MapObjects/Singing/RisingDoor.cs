using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RisingDoor : MonoBehaviour
{
    public float risingDis;
    public float risingSpeed;
    float moveDis;
    bool ifMove = false;
    IEnumerator MoveDoor()
    {
        while (moveDis < risingDis)
        {
            moveDis += risingSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, (Vector2)transform.position + Vector2.up, risingSpeed * Time.deltaTime);
            yield return null;
        }

    }
    public void Rising()
    {
        if (!ifMove)
        {
            ifMove = true;
            StartCoroutine(MoveDoor());
        }
    }
}
