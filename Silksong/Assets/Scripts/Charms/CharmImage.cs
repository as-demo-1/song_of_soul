using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharmImage : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField]
    private GameObject equipIcon;
    [SerializeField]
    private GameObject lockIcon;
    [SerializeField]
    private GameObject selectIcon;
    [SerializeField]
    private Image charmSprite;

    public CharmSO charmSO;
    public CharmListSO charmListSO;

    public CharmUIPanel charmUIPanel;

    private bool isSelected;

    private bool slotEmpty = true;
    public bool SlotEmpty { get => slotEmpty; set => slotEmpty = value; }


    [SerializeField]
    private CharmUIState CharmUIState = CharmUIState.ICON;


    // Start is called before the first frame update
    void Start()
    {
        if (CharmUIState == CharmUIState.ICON)
        {
            GetComponent<Image>().sprite = charmSO.charmImage;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isSelected && PlayerInput.Instance.normalAttack.Down)
        {
            if (CharmUIState == CharmUIState.ICON && charmSO.HasCollected && !charmSO.HasEquiped)
            {
                charmUIPanel.TryEquipCharm(gameObject);
            }
            else if (CharmUIState == CharmUIState.SLOT && !SlotEmpty)
            {
                charmUIPanel.TryDisEquipCharm(gameObject);
            }
            
        }
    }
    public void CharmLockDisplay()
    {
        equipIcon.SetActive(false);
        lockIcon.SetActive(true);
    }
    public void CharmEquipDisplay()
    {
        equipIcon.SetActive(true);
        lockIcon.SetActive(false);
    }
    public void CharmCollectDisplay()
    {
        equipIcon.SetActive(false);
        lockIcon.SetActive(false);
    }
    public void CharmSlotDisplay(bool _option)
    {
        if (_option)
        {
            charmSprite.sprite = charmSO.charmImage;
            charmSprite.gameObject.SetActive(true);
        }
        else
        {
            charmSprite.sprite = null;
            charmSprite.gameObject.SetActive(false);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (charmSO != null)
        {
            charmUIPanel.ShowCharmText(charmSO);
        }
        
        selectIcon.SetActive(true);
        isSelected = true;
    }
    public void OnDeselect(BaseEventData eventData)
    {
        selectIcon.SetActive(false);
        isSelected = false;
    }
    public void SetSOData( CharmSO _charmSO, CharmListSO _charmListSO, CharmUIPanel _charmUIPanel)
    {
        charmSO = _charmSO;
        charmListSO = _charmListSO;
        charmUIPanel = _charmUIPanel;
    }
}

enum CharmUIState
{
    ICON, SLOT
}
