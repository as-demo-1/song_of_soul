using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NPC", menuName = "Interactive/NPC")]
public class NPCSO : SingleInteractiveBaseSO
{
    public int Step = 30;

    private GameObject m_player;

    private Vector3 _tmpTalkCoord;

    [HideInInspector]
    [SerializeField] private EInteractiveItemType _itemType = EInteractiveItemType.DIALOG;
    public override EInteractiveItemType ItemType => _itemType;

    [Tooltip("The talk_coo offset of the NPC")]
    [SerializeField] private Vector3 _talk_coord = default;
    [Tooltip("The state list of the NPC")]
    [SerializeField] private List<NPC_State_SO_Config> _stateList = default;


    public Vector3 TalkCoord => _talk_coord;
    // todo:
    // npc动画
    public List<NPC_State_SO_Config> StateList => _stateList;

    protected override void SetField(GameObject go)
    {
        base.SetField(go);
        go.AddComponent<NPCController>();
    }



    // =========

    protected override void DoInteract()
    {
        // todo:
        // 1.播放行走动画并移动 在走到相应坐标时停止
        Debug.Log("对话框");

        m_player = GameObject.FindGameObjectWithTag("Player");

        if (m_player != null)
        {
            move(base.DoInteract);
        }
        else
        {
            Debug.LogError("Player not found");
        }
    }

    protected override void AfterInteract()
    {
        NPCController npc = InteractManager.Instance.GetInteractiveItemComponent<NPCController>();

        npc.Run(base.AfterInteract);
    }

    private void showDialog(UnityAction callback)
    {
        // todo:
        // 2.调用对话系统的方法
        Debug.Log("调用对话系统方法");

        TalkController.Instance.StartTalk(InteractiveID, callback);

        UIComponentManager.Instance.UIAddListener(InteractConstant.UITalkNext, () => {
            TalkController.Instance.StartTalk(InteractiveID, callback);
        });
    }

    private void move(UnityAction callback)
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
                actions.Enqueue(() => {
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
                actions.Enqueue(() => {
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
