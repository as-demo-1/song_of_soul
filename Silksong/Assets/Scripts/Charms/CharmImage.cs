using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharmImage : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    // [SerializeField]
    // private GameObject equipIcon;
    // [SerializeField]
    // private GameObject lockIcon;
    // [SerializeField]
    // private GameObject selectIcon;
    [SerializeField]
    private Image charmSprite;
    //
    // public CharmSO charmSO;
    // public Charm charm;
    // public CharmListSO charmListSO;
    //
    private CharmUIPanel charmUIPanel;

    private bool isSelected;

    private bool slotEmpty = true;
    public bool SlotEmpty { get => slotEmpty; set => slotEmpty = value; }

    public Charm charm;

    public CharmSlot slot;
    // [SerializeField]
    // private CharmUIState CharmUIState = CharmUIState.ICON;


    // Start is called before the first frame update
    void Start()
    {
        // if (CharmUIState == CharmUIState.ICON)
        // {
        //     GetComponent<Image>().sprite = charmSO.charmImage;
        // }
    }

    // Update is called once per frame
    void Update()
    {
        //PlayerInput.Instance.normalAttack.Down
        // if (isSelected && Input.GetKeyDown(KeyCode.J))
        // {
        //     if (CharmUIState == CharmUIState.ICON && charmSO.HasCollected && !charmSO.HasEquiped)
        //     {
        //         charmUIPanel.TryEquipCharm(gameObject);
        //     }
        //     else if (CharmUIState == CharmUIState.SLOT && !SlotEmpty)
        //     {
        //         charmUIPanel.TryDisEquipCharm(gameObject);
        //     }
        //     
        // }
    }
    // public void CharmLockDisplay()
    // {
    //     equipIcon.SetActive(false);
    //     lockIcon.SetActive(true);
    // }
    // public void CharmEquipDisplay()
    // {
    //     equipIcon.SetActive(true);
    //     lockIcon.SetActive(false);
    // }
    // public void CharmCollectDisplay()
    // {
    //     equipIcon.SetActive(false);
    //     lockIcon.SetActive(false);
    // }
    // public void CharmSlotDisplay(bool _option)
    // {
    //     if (_option)
    //     {
    //         charmSprite.sprite = charmSO.charmImage;
    //         charmSprite.gameObject.SetActive(true);
    //     }
    //     else
    //     {
    //         charmSprite.sprite = null;
    //         charmSprite.gameObject.SetActive(false);
    //     }
    // }
    //
    public void OnSelect(BaseEventData eventData)
    {
        charmUIPanel.ChangeSelect(this);
        //isSelected = true;
    }
    public void OnDeselect(BaseEventData eventData)
    {
        // selectIcon.SetActive(false);
        //isSelected = false;
    }
    // public void SetSOData( CharmSO _charmSO, CharmListSO _charmListSO, CharmUIPanel _charmUIPanel)
    // {
    //     charmSO = _charmSO;
    //     charmListSO = _charmListSO;
    //     charmUIPanel = _charmUIPanel;
    // }
    // public void SetData( Charm _charm, CharmListSO _charmListSO, CharmUIPanel _charmUIPanel)
    // {
    //     charm = _charm;
    //     charmListSO = _charmListSO;
    //     charmUIPanel = _charmUIPanel;
    // }
    public void Init(Charm _charm, CharmUIPanel _charmUIPanel)
    {
        charm = _charm;
        charmUIPanel = _charmUIPanel;
        charmSprite.sprite = _charm.charmImage;
    }
}

// enum CharmUIState
// {
//     ICON, SLOT
// }
