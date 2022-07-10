using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Inventory", menuName = "Inventory/Inventory")]
public class InventorySO : ScriptableObject
{
	[Tooltip("The collection of items and their quantities.")]
	[SerializeField]
	private List<ItemStack> _items = new List<ItemStack>();
	[SerializeField]
	private List<ItemStack> _defaultItems = new List<ItemStack>();
	public List<ItemStack> Items => _items;

	public void Add(ItemSO item, int count = 1)
	{
		if (count <= 0)
			return;

		for (int i = 0; i < _items.Count; i++)
		{
			ItemStack currentItemStack = _items[i];
			if (item == currentItemStack.Item)
			{
				//only add to the amount if the item is usable
				currentItemStack.Amount += count;
				return;
			}
		}

		_items.Add(new ItemStack(item, count));
	}

	public void Remove(ItemSO item, int count = 1)
	{
		if (count <= 0)
			return;

		for (int i = 0; i < _items.Count; i++)
		{
			ItemStack currentItemStack = _items[i];

			if (currentItemStack.Item == item)
			{
				currentItemStack.Amount -= count;

				if (currentItemStack.Amount <= 0)
					_items.Remove(currentItemStack);

				return;
			}
		}
	}

	public bool Contains(ItemSO item)
	{
		for (int i = 0; i < _items.Count; i++)
		{
			if (item == _items[i].Item)
			{
				return true;
			}
		}

		return false;
	}

	public int Count(ItemSO item)
	{
		for (int i = 0; i < _items.Count; i++)
		{
			ItemStack currentItemStack = _items[i];
			if (item == currentItemStack.Item)
			{
				return currentItemStack.Amount;
			}
		}

		return 0;
	}
	public ItemSO Search(string id)
    {
		foreach(ItemStack i in _items)
        {
			if (i.Item.ID == id)
				return i.Item;
        }
		return null;
    }

	public void Init()
	{
		if (_items == null)
		{
			_items = new List<ItemStack>();
		}
		_items.Clear();
		foreach (ItemStack item in _defaultItems)
		{
			_items.Add(new ItemStack(item));

		}
	}
}
