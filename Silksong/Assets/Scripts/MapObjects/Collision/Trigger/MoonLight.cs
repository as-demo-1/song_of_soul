using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonLight :MonoBehaviour
{
    public float g;
    public Vector3 direction;
    public LayerMask Ground;
    private PlayerController playerController;
    LineRenderer line;
    RaycastHit2D[] hit2Ds;
    //List<Vector2> poins=new List<Vector2>();
    private void Update()
    {
        line.SetPosition(1, transform.GetChild(0).transform.position);
        line.SetPosition(0, transform.GetChild(1).transform.position);
    }
    private void Awake()
    {
        Physics2D.queriesStartInColliders = false;
        line = GetComponent<LineRenderer>();
        //Radial(transform.GetChild(1).transform.position, transform.GetChild(0).transform.position);
    }
    #region ÆúÓÃ2
    //public Vector3 Radial(Vector2 shootPos, Vector2 target)
    //{
    //    if(line==null) line = GetComponent<LineRenderer>();
    //    line.SetPosition(0, transform.GetChild(1).transform.position);
    //    Vector3 direct = target - shootPos;
    //    RaycastHit2D hit = Physics2D.Raycast(shootPos, direct, 100, Ground);
    //    if (hit.collider != null)
    //    {
    //        line.SetPosition(1, hit.point);
    //        return hit.point;
    //    }
    //    else
    //    {
    //        Vector2 pos1 = (Vector3)shootPos + direct * 20;
    //        line.SetPosition(1, pos1);
    //        return pos1;          
    //    }
    //}
    //public void MoonLightEffect()
    //{
    //    for(int i=0; i < poins.Count-1; i++)
    //    {
    //        hit2Ds = Physics2D.RaycastAll(poins[i], poins[i + 1] - poins[i],100);
    //        foreach(var hit in hit2Ds)
    //        {
    //            if (hit.collider.gameObject.CompareTag("Player"))
    //            {
    //                playerController = hit.collider.GetComponent<PlayerController>();
    //                if (playerController && playerController.gravityLock == false)
    //                {
    //                    playerController.RB.gravityScale = g;
    //                    return;
    //                }
    //            }
    //            else if (hit.collider.GetComponent<Reflector>())
    //            {
    //                if(!hit.collider.GetComponent<Reflector>().newLight)
    //                hit.collider.GetComponent<Reflector>().ReflectLight(this,hit.point);
    //            }
    //        }
    //    }
    //    if (playerController&&playerController.gravityLock == false)
    //    {            playerController.RB.gravityScale = playerController.playerInfo.normalGravityScale;
    //        playerController = null;
    //    }
    //}
    //private void FixedUpdate()
    //{
    //    MoonLightEffect();
    //}
    #endregion
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

        if (playerController && playerController.gravityLock == false)
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
