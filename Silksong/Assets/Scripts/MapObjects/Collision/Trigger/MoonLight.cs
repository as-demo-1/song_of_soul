using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonLight :MonoBehaviour
{
    public float g;
    public Vector3 direction;
    private PlayerController playerController;
    private void OnTriggerExit2D(Collider2D collision)
    {    
        if (collision.gameObject.CompareTag("Player"))
        {
            if (playerController.gravityLock == false)
            {
                playerController.RB.gravityScale = playerController.playerInfo.normalGravityScale;
            }
            playerController = null;
        }            
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        if (playerController && playerController.gravityLock==false)
        {
            playerController.RB.gravityScale = g;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerController = collision.GetComponent<PlayerController>();
        }
    }
}
