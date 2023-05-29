using DG.Tweening;
using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.DataStructures;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestView : MonoBehaviour
{
	public Text Title;
	public TextMeshProUGUI Description;
	public Image Icon;

	public QuestSlot selectedQuest;

	private Inventory inventory;

	public GameObject questPrefab;

	public Transform monsterContent;	
	public Transform npcContent;	
	public Transform bossContent;
	public ItemCategory ShowItemCategoryMoster;
	public ItemCategory ShowItemCategoryNpc;
	public ItemCategory ShowItemCategoryBoss;
	public List<ItemCategory> itemCategory;
	private int indexQuest = 0;
	private int indexMax;

	public Text[] firstTitle;
	public Image[] firstImage;
	public Transform content;
	// Start is called before the first frame update
	void Start()
	{
		inventory = InventorySystemManager.GetInventoryIdentifier(1).Inventory;
		Init();
	}
	public void Init()
	{
		int a = 0;
		int b = 0;
		int c = 0;
		foreach (ItemStack o in inventory.GetItemCollection("Quests").GetAllItemStacks())
		{
			if(o.Item.Category == ShowItemCategoryMoster)
			{
				QuestSlot questSlot = Instantiate(questPrefab, monsterContent).GetComponent<QuestSlot>();
				questSlot.Init(o.Item, this,a, monsterContent);
				a++;
			}
			else if (o.Item.Category == ShowItemCategoryNpc)
			{
				QuestSlot questSlot = Instantiate(questPrefab, npcContent).GetComponent<QuestSlot>();
				questSlot.Init(o.Item, this,b, npcContent);
				b++;
			}
			else
			{
				QuestSlot questSlot = Instantiate(questPrefab, bossContent).GetComponent<QuestSlot>();
				questSlot.Init(o.Item, this,c, bossContent);
				c++;
			}
		}
	}
    // Update is called once per frame
    void Update()
    {
	}

	public void Refesh(int index)
	{
		Title.text = selectedQuest.firstTitle;
		Icon.sprite = selectedQuest.firstImage;
		Icon.gameObject.SetActive(true);
		Description.text = selectedQuest.firstDescription;
		content.DOLocalMoveY(index * 128, 2f);

	}

	public void UpdateUI()
	{
	}
}
