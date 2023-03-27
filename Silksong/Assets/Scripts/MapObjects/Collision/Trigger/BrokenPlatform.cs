using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenPlatform : Trigger2DBase
{
    public float brokeTime;
    public float recreatTime;
    public Animator anim;
    protected override void enterEvent()
    {
        StartCoroutine(platfromBroke());
    }

    IEnumerator platfromBroke()
    {
        if (anim != null)
        {
            anim.Play("broken");
            anim.speed = (0.5f/ brokeTime);
        }
        yield return new WaitForSeconds(brokeTime);
        hide();
        yield return new WaitForSeconds(recreatTime);
        show();
    }

    void hide()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        foreach (var c in GetComponents<Collider2D>())
        {
            c.enabled = false;
        }

    }

    void show()
    {
        GetComponent<SpriteRenderer>().enabled = true;
        foreach (var c in GetComponents<Collider2D>())
        {
            c.enabled = true;
        }
    }
}
