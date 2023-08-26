using System;
using System.Collections;
using System.Collections.Generic;
using Opsive.UltimateInventorySystem.UI.Panels;
using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
using UnityEngine;


/// <summary>
/// 用于联动物品插件的panel开关
/// </summary>
public class SmartPanel : MonoBehaviour
{
    public List<DisplayPanel> panels = new List<DisplayPanel>();

    private void OnEnable()
    {
        foreach (var panel in panels)
        {
            
            panel.GetComponent<ItemViewSlotsContainerPanelBinding>().BindInventory(GameManager.Instance.inventory);
            
            panel.SmartOpen();
        }
    }
    private void OnDisable()
    {
        foreach (var panel in panels)
        {
            panel.SmartClose();
        }
    }
}
