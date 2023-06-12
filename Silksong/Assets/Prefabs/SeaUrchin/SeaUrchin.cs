using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class SeaUrchin : Enemy_FSM
{
    public List<EnemyFSMManager> pricks;
    public List<SeaUrchin_Chain> chains;
    public List<Tutorial_GrapplingGun> hooks;
    public GameObject Water;
    public string idleState, skillState,prickReturn;
    public bool ifInWater;
    public void ChangeMode(){
        
         
    }
    override protected void Update()
    {
        base.Update();//
        if (Input.GetKeyDown(KeyCode.RightAlt))
        {
            Water.SetActive(true);
            ChangeState(idleState);
            ifInWater = true;
            foreach (var prick in pricks)
                prick.ChangeState(prickReturn); 
            foreach(var hook in hooks)
            {
                hook.grappleRope.enabled = true;
                hook.TakeBack_Rope();
            }
        }
    }
}
