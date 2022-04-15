using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 挂在主界面的需要选择音效和按键音效的按钮上
/// </summary>
public class ButtonSE : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        StartMenu.Instance.PlaySelectSoundEffect();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StartMenu.Instance.PlayClickSoundEffect();
    }
}
