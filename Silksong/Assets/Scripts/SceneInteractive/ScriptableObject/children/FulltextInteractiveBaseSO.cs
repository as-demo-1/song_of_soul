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
                //Debug.Log("go");
                Image image = go.GetComponent<Image>();
                image.sprite = sprite;
            }
            //Debug.Log("对话框");

            //m_player = GameObject.FindGameObjectWithTag("Player");

        }
        UIComponentManager.Instance.SetText(InteractConstant.UIFullTextText, Tip);
        UIComponentManager.Instance.UIAddListener(InteractConstant.UIFullTextClose, base.DoInteract);
        showDialog(base.DoInteract);
    }

    protected override void Finish()
    {
        UIComponentManager.Instance.HideUI(InteractConstant.UIFullText);
        base.Finish();
    }   

    private void showDialog(UnityAction callback)
    {
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

}
