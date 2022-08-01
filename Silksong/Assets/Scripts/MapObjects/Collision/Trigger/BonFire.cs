using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonFire : MonoBehaviour
{
    public int stayTime = 1;

    public int reduceValue = 0;
    
    private float recordTime = 0;   

    private float time = 0;

    private PlayerCharacter playerCharacter;

    private  void OnTriggerEnter2D(Collider2D collision)
    {
         if (collision.gameObject.CompareTag("Player")) 
        {
            playerCharacter = collision.GetComponent<PlayerCharacter>();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && playerCharacter != null) 
        {
            recordTime = recordTime + Time.deltaTime;
            if (recordTime >= stayTime)
            {
                time = time + Time.deltaTime;
                if(time >= 1)
                {
                    playerCharacter.reduceColdValue(reduceValue);
                    time = 0;
                }
            }
        } 
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        recordTime = 0;
        time = 0;
        playerCharacter = null;
    }
}
