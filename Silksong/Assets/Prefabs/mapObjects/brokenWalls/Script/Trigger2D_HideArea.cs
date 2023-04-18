using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger2D_HideArea : Trigger2DBase
{
    public Animator animator;
    public List<Trigger2D_HideArea> connectArea;

    protected void Start()
    {

    }
    protected override void enterEvent()
    {
        base.enterEvent();
        Disappear();
    }
    public void Disappear()
    {
        DisappearSelf();
        for(int i = 0; i < connectArea.Count; i++)
        {
            connectArea[i].DisappearSelf();
        }
    }
    public void DisappearSelf()
    {
         animator.Play("hide");
        GamingSaveObj<bool> gamingSave;
        if (TryGetComponent(out gamingSave) &&!gamingSave.ban)
        {
            gamingSave.saveGamingData(true);
        }
    }
}
