using System;
using UnityEngine;


[Serializable]
public class ItemStacka
{
    [SerializeField]
    private ItemSO _item;

    public ItemSO Item => _item;

    public int Amount;
    public ItemStacka()
    {
        _item = null;
        Amount = 0;
    }
    public ItemStacka(ItemStacka itemStack)
    {
        _item = itemStack.Item;
        Amount = itemStack.Amount;
    }
    public ItemStacka(ItemSO item, int amount)
    {
        _item = item;
        Amount = amount;
    }
}