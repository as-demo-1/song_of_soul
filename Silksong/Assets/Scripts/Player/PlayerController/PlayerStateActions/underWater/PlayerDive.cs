using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDive : PlayerAction
{
    public PlayerDive(PlayerController playerController) : base(playerController) { }
    public override void StateStart(EPlayerState oldState)
    {
        Vector3 scale = playerController.transform.localScale;
        playerController.transform.localScale = (new Vector3(Mathf.Abs(scale.x),scale.y,scale.z));
    }
    public override void StateUpdate()
    {
        Transform m_Transform = playerController.transform;
        Vector2 dir = new Vector2(PlayerInput.Instance.horizontal.Value, PlayerInput.Instance.vertical.Value).normalized;
        //Debug.Log(dir);
        float x = Vector2.SignedAngle(Vector2.right, dir);
        //Debug.Log(x);
        m_Transform.localRotation = Quaternion.Euler(0, 0, x);

        if (x >= -45 && x <= 90) playerController.playerInfo.playerFacingRight = true;
        else playerController.playerInfo.playerFacingRight = false;

        Vector3 scale = m_Transform.localScale;
        if (Mathf.Abs(x) > 90 && scale.y>0)
        {      
            m_Transform.localScale = new Vector3(scale.x, -scale.y, scale.z);
        }
        else if(Mathf.Abs(x) <= 90 && scale.y<0) m_Transform.localScale = new Vector3(scale.x, -scale.y, scale.z); 

        playerController.setRigidVelocity(dir* Constants.PlayerDiveSpeed);
      
    }
    public override void StateEnd(EPlayerState newState)
    {
        playerController.m_Transform.localRotation = Quaternion.Euler(0, 0, 0);

        Vector3 scale = playerController.m_Transform.localScale;
        scale.x = Mathf.Abs(scale.x) *( playerController.playerInfo.playerFacingRight ? 1 : -1);
        scale.y = Mathf.Abs(scale.y);
        playerController.m_Transform.localScale = scale;
        
    }
}