using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogInteract : InteractTriggerBase<DialogInteract>
{
    public Vector3 DialogPos;
    public int Step = 30;

    private GameObject m_player;

    protected override void InteractEvent()
    {
        // todo:
        // 1.播放行走动画并移动 在走到相应坐标时停止
        Debug.Log("对话框");

        m_player = GameObject.Find("Player");

        if (m_player != null)
        {
            move();
        }
        else
        {
            Debug.LogError("Player not found");
        }
    }

    private void showDialog()
    {
        // todo:
        // 2.调用对话系统的方法
        Debug.Log("调用对话系统方法");
    }

    private void move()
    {
        int times = Step;

        Queue<System.Action> actions = PlayerInput.Instance.actions;
        Animator anim = m_player.GetComponent<Animator>();
        SpriteRenderer sprite = m_player.GetComponent<SpriteRenderer>();
        while (--times >= 0)
        {
            if (times == 0)
            {
                actions.Enqueue(() => {
                    Vector3 tmpPos = m_player.transform.position;
                    tmpPos.x += (DialogPos.x - tmpPos.x) / Step;
                    m_player.transform.position = tmpPos;
                    sprite.flipX = true;
                    PlayerController.Instance.playerInfo.playerFacingRight = false;
                    showDialog();
                });
            }
            else
            {
                actions.Enqueue(() => {
                    sprite.flipX = false;
                    PlayerController.Instance.playerInfo.playerFacingRight = true;

                    Vector3 tmpPos = m_player.transform.position;

                    anim.SetInteger("HorizontalInput", 1);
                    anim.SetFloat("HorizontalSpeed", (DialogPos.x - tmpPos.x) / Step);

                    tmpPos.x += (DialogPos.x - tmpPos.x) / Step;
                    m_player.transform.position = tmpPos;
                });
            }
        }
    }
}


