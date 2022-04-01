using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

[CreateAssetMenu(fileName = "fulltext", menuName = "Interactive/fulltext")]
public class FulltextInteractiveBaseSO : SingleInteractiveBaseSO
{
    private GameObject m_player;
    public int Step = 30;
    private Vector3 _tmpTalkCoord;

    [HideInInspector]
    [SerializeField] private EInteractiveItemType _itemType = EInteractiveItemType.FULLTEXT;
    public override EInteractiveItemType ItemType => _itemType;

    [Tooltip("The talk_coo offset of the NPC")]
    [SerializeField] private Vector3 _talk_coord = default;
    public Vector3 TalkCoord => _talk_coord;

    [Tooltip("Draw sprite")]
    [SerializeField] public Sprite sprite;

    [Tooltip("the tip displayed in the fulltext")]
    [SerializeField] private string _tip = default;
    public string Tip => _tip;

    protected override void SetField(GameObject go)
    {
        base.SetField(go);
        if (IsDraw == true)
        {
            go.AddComponent<NPCController>();
        }
    }


    protected override void DoInteract()
    {
        UIComponentManager.Instance.ShowUI(InteractConstant.UIFullText);
        if (IsDraw == true)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
            GameObject go = GameObject.Find("FullWindowText/Image");
            if (go != null)
            {
                Debug.Log("go");
                Image image = go.GetComponent<Image>();
                image.sprite = sprite;
            }
            Debug.Log("对话框");

            m_player = GameObject.FindGameObjectWithTag("Player");

        }
        UIComponentManager.Instance.SetText(InteractConstant.UIFullTextText, Tip);
        UIComponentManager.Instance.UIAddListener(InteractConstant.UIFullTextClose, base.DoInteract);
        move(base.DoInteract);
    }

    protected override void Finish()
    {
        UIComponentManager.Instance.HideUI(InteractConstant.UIFullText);
        base.Finish();
    }   

    private void showDialog(UnityAction callback)
    {
        Debug.Log("123");
        // todo:
        // 2.调用对话系统的方法
        if (IsDraw == true)
        {
            Debug.Log("调用对话系统方法");

            TalkController.Instance.StartTalk(InteractiveID, callback);

            UIComponentManager.Instance.UIAddListener(InteractConstant.UITalkNext, () =>
            {
                TalkController.Instance.StartTalk(InteractiveID, callback);
            });
        }
    }


    private void move(UnityAction callback)
    {
        if (IsDraw == true)
        {
            int times = Step;

            // todo:
            // 控制角色移动并播放动画
            Queue<System.Action> actions = PlayerInput.Instance.actions;
            //SpriteRenderer sprite = m_player.GetComponent<SpriteRenderer>();
            _tmpTalkCoord = InteractManager.Instance.InteractObjectTransform.position + TalkCoord;
            while (--times >= 0)
            {
                if (times == 0)
                {
                    actions.Enqueue(() =>
                    {
                        Vector3 tmpPos = m_player.transform.position;
                        float tmpStep = (_tmpTalkCoord.x - tmpPos.x) / Step;
                        tmpPos.x += tmpStep;
                        m_player.transform.position = tmpPos;

                        //sprite.flipX = tmpStep > 0;
                        PlayerController.Instance.playerInfo.playerFacingRight = tmpStep <= 0;

                        showDialog(callback);
                    });
                }
                else
                {
                    actions.Enqueue(() =>
                    {
                        Vector3 tmpPos = m_player.transform.position;
                        float tmpStep = (_tmpTalkCoord.x - tmpPos.x) / Step;
                        //sprite.flipX = tmpStep <= 0;
                        PlayerController.Instance.playerInfo.playerFacingRight = tmpStep > 0;

                        PlayerController.Instance.playerAnimatorStatesControl.ChangePlayerState(EPlayerState.Run);

                        tmpPos.x += tmpStep;
                        m_player.transform.position = tmpPos;
                    });
                }
            }
        }
    }

}
