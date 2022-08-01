using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelLabel : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private bool isSelected;
    [SerializeField]
    private GameObject selectIcon;

    [Tooltip("标签对应的菜单栏")]
    [SerializeField]
    private PanelName panelName;

    [SerializeField]
    private GameMenu gameMenu = default;
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
        selectIcon.SetActive(true);
        isSelected = true;
    }
    public void OnDeselect(BaseEventData eventData)
    {
        selectIcon.SetActive(false);
        isSelected = false;
    }
}

public enum PanelName
{
    [Tooltip("地图")]
    MAP,
    [Tooltip("装备")]
    EQUIP,
    [Tooltip("护符")]
    CHARM,
    [Tooltip("图鉴")]
    BOOK,
    [Tooltip("成就")]
    ACHIEVEMENT,
    [Tooltip("选项")]
    OPTION,
}
