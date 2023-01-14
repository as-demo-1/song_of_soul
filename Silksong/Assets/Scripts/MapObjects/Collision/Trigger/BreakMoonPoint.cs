using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakMoonPoint:MonoBehaviour
{
    private PlayerBreakMoon playerBreakMoon;
#if UNITY_EDITOR 
    [DisplayOnly]
#endif
    public bool isPicked;
    public LayerMask targetLayer;
    // public float cd;
#if UNITY_EDITOR
    [DisplayOnly]
#endif
    public bool ready;

    private SpriteRenderer spriteRenderer;
    private Collider2D collider2d;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider2d = GetComponent<Collider2D>();
        ready = true;
    }
    protected  void OnTriggerEnter2D(Collider2D collision)
    {
        if (ready && targetLayer.Contains(collision.gameObject))
        {
            playerBreakMoon = collision.GetComponent<PlayerController>().playerStatesBehaviour.StateActionsDic[EPlayerState.BreakMoon] as PlayerBreakMoon ;
            playerBreakMoon.availableTargets.Add(this);
            playerBreakMoon.findCurrentTarget();
        }
    }

    protected  void OnTriggerExit2D(Collider2D collision)
    {
        if (targetLayer.Contains(collision.gameObject))
        {
            playerBreakMoon.availableTargets.Remove(this);
            unPicked();
            playerBreakMoon.findCurrentTarget();
        }
    }

    public void bePicked()
    {
        isPicked = true;
        spriteRenderer.color = new Color(0, 255, 0);//被选中后变为绿色
    }

    private void broken()
    {
        //spriteRenderer.enabled = false;
      /*  playerBreakMoon.availableTargets.Remove(this);
        playerBreakMoon.findCurrentTarget();*/
        collider2d.enabled = false;
        unPicked();
        spriteRenderer.color = new Color(255, 0, 0);
    }

    private void recover()
    {
        //spriteRenderer.enabled = true;
        spriteRenderer.color = new Color(255, 255, 255);
        collider2d.enabled = true;
    }

    public void unPicked()
    {
        isPicked = false;
        spriteRenderer.color = new Color(255, 255, 255);
    }

    public void atBreakMoonPoint()//在玩家使用碎月，接触碎月点后调用
    {
        StartCoroutine(countCd());
    }

   private IEnumerator countCd()
    {
        ready = false;
        broken();
        yield return new WaitForSeconds(Constants.BreakMoonPointCd);
        ready = true;
        recover();
    }
}
