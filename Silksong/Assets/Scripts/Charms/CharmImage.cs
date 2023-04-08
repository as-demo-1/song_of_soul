using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharmImage : MonoBehaviour, ISelectHandler
{
    // [SerializeField]
    // private GameObject equipIcon;
    // [SerializeField]
    // private GameObject lockIcon;
    // [SerializeField]
    // private GameObject selectIcon;
    [SerializeField]
    private Image charmSprite;

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
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log("select");
        charmUIPanel.ChangeSelect(this);
        //isSelected = true;
    }
    public void Init(Charm _charm, CharmUIPanel _charmUIPanel)
    {
        charm = _charm;
        charmUIPanel = _charmUIPanel;
        charmSprite.sprite = _charm.charmImage;
    }
}

