using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueChangeItem : MonoBehaviour
{

    private static DialogueChangeItem _instance;
    public static DialogueChangeItem Instance => _instance;

    public enum ItemAction
    {
        Add,
        Remove
    }
    
    [System.Serializable]
    public struct ItemDIc
    {
        public int BelongID;
        public ItemAction Action;
        public List<ItemSO> ItemSOList;
        public List<int> Amount;
    }

    [Tooltip("Player's Bag")]
    public InventorySO Inventory;


    public ItemDIc[] Itemdics;
    public static Dictionary<int, List<ItemSO>> Itemdic;
    public static Dictionary<int, List<int>> ItemAmount;
    public static Dictionary<int, ItemAction> Action;


    public void Start()
    {
        _instance = this;
        // 字典内容
        Itemdic = new Dictionary<int,List<ItemSO>>();
        ItemAmount = new Dictionary<int, List<int>>();
        Action = new Dictionary<int, ItemAction>();

        for (int i = 0; i < Itemdics.Length; i++)
        {
            if (!Itemdic.ContainsKey(Itemdics[i].BelongID))
            {
                Itemdic.Add(Itemdics[i].BelongID, Itemdics[i].ItemSOList);
                ItemAmount.Add(Itemdics[i].BelongID, Itemdics[i].Amount);
                Action.Add(Itemdics[i].BelongID, Itemdics[i].Action);
            }
        }
        //Debug.Log(Itemdic.Count + "-" + ItemAmount.Count + "-" + Action.Count);
    }

    public void AddToBag(int talkID)
    {
        List<ItemSO> list = Itemdic[talkID];
        List<int> amount = ItemAmount[talkID];
        for (int i = 0;i < list.Count; i++)
        {
            Inventory.Add(list[i], amount[i]);
        }
        Debug.Log("已完成对话完后添加物品到PlayerInventory");
    }

    public void RemoveFromBag(int talkID)
    {
        List<ItemSO> list = Itemdic[talkID];
        List<int> amount = ItemAmount[talkID];
        for (int i = 0; i < list.Count; i++)
        {
            Inventory.Remove(list[i], amount[i]);
        }
        Debug.Log("已完成对话完后从PlayerInventory删除物品");
    }

    public void ItemAct(int talkID)
    {
        //Debug.Log(Itemdic.Count);
        if (Itemdic.ContainsKey(talkID))
        {
            ItemAction action = Action[talkID];
            Debug.Log(action.GetType());
            if (action.ToString().Equals("Add"))
            {
                AddToBag(talkID);
            }
            else if (action.ToString().Equals("Remove"))
            {
                RemoveFromBag(talkID);
            }
        }
    }
}
