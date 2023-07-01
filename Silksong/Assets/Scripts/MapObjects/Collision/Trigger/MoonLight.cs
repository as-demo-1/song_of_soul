using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 这个文件夹下统一了对应组件的触发器，包括碰撞进入和退出
/// </summary>
public class MoonLight :MonoBehaviour
{
    public float g;

    private PlayerController playerController;
    private void OnTriggerExit2D(Collider2D collision)
    {    
        if (collision.gameObject.CompareTag("Player"))
        {
            playerController.setRigidGravityScaleToNormal();
            playerController = null;
        }            
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        if (playerController)
        {
            playerController.setRigidGravityScale(g);
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
