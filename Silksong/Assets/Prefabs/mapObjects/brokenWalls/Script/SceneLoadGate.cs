using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoadGate : SceneTransitionPoint
{
    public GameObject tip;
    public KeyCode EnterKey;
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        //base.OnTriggerEnter2D(collision);
        tip.SetActive(true);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Input.GetKeyDown(EnterKey))
        {
            SceneController.TransitionToScene(this);
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
        tip.SetActive(false);
    }
}
