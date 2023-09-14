

using System;
using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using Opsive.UltimateInventorySystem.Exchange;
using Opsive.UltimateInventorySystem.Interactions;
using UnityEngine;

/// <summary>
/// 拾取一堆物品
/// 可以在inventory中增加多个多种物品
/// </summary>
public class Trigger2D_InventoryPick: Trigger2DBase
{
    public Inventory inventory;
    public int count;
    public GameObject tip;
    
    [SerializeField]
    private KeyCode pickKey = KeyCode.E;
    private bool isPicking = false;

    private bool isEnter;
    private void Start()
    {
        
    }

    private void Update()
    {
        if (isEnter)
        {
            if (Input.GetKeyDown(pickKey))
            {
                if (!isPicking)
                {
                    Debug.Log("发生");
                    var pickupItems = inventory.MainItemCollection.GetAllItemStacks();
                    GameManager.Instance.inventory.GetItemCollection("Main")
                        .AddItems((pickupItems, 0));
                    isPicking = true;
                    Destroy(this.gameObject);
                }
            }
        }
    }

    protected override void enterEvent()
    {
        tip.SetActive(true);
        isEnter = true;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        
        
    }

    protected override void exitEvent()
    {
        tip.SetActive(false);
        isEnter = false;
    }
}