using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogInteract : InteractTriggerBase<DialogInteract>
{
    public int Step = 30;

    private GameObject m_player;

    protected override void InteractEvent()
    {
        // todo:
        // 1.播放行走动画并移动 在走到相应坐标时停止
        Debug.Log("对话框");

        m_player = GameObject.FindGameObjectWithTag("Player");

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

        TalkController.Instance.StartTalk(InteractManager.Instance.InteractiveItemID);

        UIAddListener("Talk/TalkPanel/Next", () => {
            TalkController.Instance.StartTalk(InteractManager.Instance.InteractiveItemID);
        });
    }

    private void move()
    {
        int times = Step;

        // todo:
        // 控制角色移动并播放动画
        Queue<System.Action> actions = PlayerInput.Instance.actions;
        SpriteRenderer sprite = m_player.GetComponent<SpriteRenderer>();
        while (--times >= 0)
        {
            if (times == 0)
            {
                actions.Enqueue(() => {
                    Vector3 tmpPos = m_player.transform.position;
                    float tmpStep = (InteractManager.Instance.InteractiveItemPos.x - tmpPos.x) / Step;
                    tmpPos.x += tmpStep;
                    m_player.transform.position = tmpPos;
                    sprite.flipX = tmpStep > 0;
                    PlayerController.Instance.playerInfo.playerFacingRight = tmpStep <= 0;
                    showDialog();
                });
            }
            else
            {
                actions.Enqueue(() => {
                    Vector3 tmpPos = m_player.transform.position;
                    float tmpStep = (InteractManager.Instance.InteractiveItemPos.x - tmpPos.x) / Step;
                    sprite.flipX = tmpStep <= 0;
                    PlayerController.Instance.playerInfo.playerFacingRight = tmpStep > 0;

                    PlayerController.Instance.PlayerAnimatorStatesControl.ChangePlayerState(EPlayerState.Run);

                    tmpPos.x += tmpStep;
                    m_player.transform.position = tmpPos;
                });
            }
        }
    }
}


