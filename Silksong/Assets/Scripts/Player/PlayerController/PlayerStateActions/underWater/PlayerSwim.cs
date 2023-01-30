using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwim : PlayerAction
{
    public PlayerSwim(PlayerController playerController) : base(playerController) { }

    public override void StateUpdate()
    {
        Transform m_Transform = playerController.transform;

        if (PlayerInput.Instance.horizontal.Value == -1f && PlayerInput.Instance.vertical.Value == 1f)     //左上
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, 45);
            m_Transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (PlayerInput.Instance.horizontal.Value == 1f && PlayerInput.Instance.vertical.Value == 1f)    //右上
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, -45);
            m_Transform.localScale = new Vector3(1, 1, 1);
        }
        else if (PlayerInput.Instance.horizontal.Value == -1f && PlayerInput.Instance.vertical.Value == -1f)    //左下
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, 135);
            m_Transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (PlayerInput.Instance.horizontal.Value == 1f && PlayerInput.Instance.vertical.Value == -1f)    //右下
        {
            m_Transform.localRotation = Quaternion.Euler(0, 0, -135);
            m_Transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            if (PlayerInput.Instance.vertical.Value == 1f)                                //上
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, 0);
                playerController.setRigidVelocity(new Vector2(0, 1) * playerController.playerInfo.getMoveSpeed());
            }
            if (PlayerInput.Instance.vertical.Value == -1f)                                //下
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, 180);
                playerController.setRigidVelocity(new Vector2(0, -1) * playerController.playerInfo.getMoveSpeed());
            }
            if (PlayerInput.Instance.horizontal.Value == -1f)                              //左
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, 90);
                m_Transform.localScale = new Vector3(-1, 1, 1);
            }
            if (PlayerInput.Instance.horizontal.Value == 1f)                                //右
            {
                m_Transform.localRotation = Quaternion.Euler(0, 0, -90);
                m_Transform.localScale = new Vector3(1, 1, 1);
            }
        }
        if (playerController.IsUnderWater)
        {
            playerController.setRigidVelocity(new Vector2(PlayerInput.Instance.horizontal.Value, PlayerInput.Instance.vertical.Value) *playerController.playerInfo.swimSpeed);
        }

    }
    public override void StateStart(EPlayerState oldState)
    {
        playerController.IsUnderWater = true;
    }
    public override void StateEnd(EPlayerState newState)
    {
        playerController.m_Transform.localRotation = Quaternion.Euler(0, 0, 0);
        playerController.IsUnderWater = false;
        //setRigidGravityScaleToNormal();
    }
}