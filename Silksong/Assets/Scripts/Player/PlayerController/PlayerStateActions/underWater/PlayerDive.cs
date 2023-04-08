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

        if(oldState==EPlayerState.Swim)
        {
            //Debug.Log(66);
            Vector2 pos = playerController.transform.position;
            playerController.transform.position = new Vector2(pos.x,pos.y-0.7f);
        }
        reversePlayerBoxColliderSize();

    }
    public override void StateUpdate()
    {
        Transform m_Transform = playerController.transform;
        Vector2 dir = new Vector2(PlayerInput.Instance.horizontal.Value, PlayerInput.Instance.vertical.Value).normalized;
        //Debug.Log(dir);
        float x = Vector2.SignedAngle(Vector2.right, dir);
        //Debug.Log(x);
        m_Transform.localRotation = Quaternion.Euler(0, 0, x);

        if (x >= -45 && x <= 45) playerController.playerInfo.playerFacingRight = true;
        else if(Mathf.Abs(x)!=90) playerController.playerInfo.playerFacingRight = false;

        Vector3 scale = m_Transform.localScale;//change scale.y to make player being right dir
        if (Mathf.Abs(x) > 90 && scale.y > 0)
        {
            m_Transform.localScale = new Vector3(scale.x, -scale.y, scale.z);
        }
        else if (Mathf.Abs(x) < 90 && scale.y < 0)
        {
            m_Transform.localScale = new Vector3(scale.x, -scale.y, scale.z);
        }//below are up dir and down dir
        else if (((x == 90 && !playerController.playerInfo.playerFacingRight) || (x == -90 && playerController.playerInfo.playerFacingRight)) && scale.y > 0)//
        {
            m_Transform.localScale = new Vector3(scale.x, -scale.y, scale.z);
        }
        else if (((x == 90 && playerController.playerInfo.playerFacingRight) || (x == -90 && !playerController.playerInfo.playerFacingRight)) && scale.y < 0)
        {
            m_Transform.localScale = new Vector3(scale.x, -scale.y, scale.z);
        }

        playerController.setRigidVelocity(dir* Constants.PlayerDiveSpeed);

        //only when up ,check if reach the water surface
        if(x>0 && Mathf.Abs(x)<180)
        {
            playerController.checkWaterSurface();
        }
      
    }
    public override void StateEnd(EPlayerState newState)
    {
        playerController.m_Transform.localRotation = Quaternion.Euler(0, 0, 0);

        Vector3 scale = playerController.m_Transform.localScale;
        scale.x = Mathf.Abs(scale.x) *( playerController.playerInfo.playerFacingRight ? 1 : -1);
        scale.y = Mathf.Abs(scale.y);
        playerController.m_Transform.localScale = scale;

        playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.WaterSurfaceParamHash, false);
        reversePlayerBoxColliderSize();
    }

    private void reversePlayerBoxColliderSize()
    {
        Vector2 newSize = new Vector2(playerController.boxCollider.size.y, playerController.boxCollider.size.x);

        playerController.boxCollider.size = newSize;
    }
}