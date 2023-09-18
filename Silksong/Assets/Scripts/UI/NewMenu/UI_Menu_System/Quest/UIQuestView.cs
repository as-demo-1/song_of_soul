using DG.Tweening;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
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
	public GameObject selected;

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

	public Text[] titles;
	public Image[] images;
	public Image[] selectedImages;

	public Transform content;

	private bool isFirst = false;
	private bool isSecond = false;
	// Start is called before the first frame update
	void Start()
	{
		inventory = GameManager.Instance.inventory;
		Init();
	}
	public void Init()
	{
		/*
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
		}*/
	}
    // Update is called once per frame
    void Update()
    {
		/*
		if (Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			if (!isSecond)
			{
				if (isFirst)
				{
					isSecond = true;
					selected.SetActive(true);
				}
				else
				{
					isFirst = true;
				}
			}
		}


		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (isFirst)
			{
				if (!isSecond)
				{
					isFirst = false;
				}
				else
				{
					isSecond = false;
					UIClear();
					selected.SetActive(false);
				}
			}
		}

		if (isSecond)
		{
			if (Input.GetKeyDown(KeyCode.Q))
			{
				indexQuest = indexQuest > 0 ? --indexQuest : 0;
				UpdateUIRight(indexQuest);
				Debug.LogFormat(indexQuest.ToString());
			}
			if (Input.GetKeyDown(KeyCode.E))
			{
				indexQuest = indexQuest < indexMax ? ++indexQuest : indexMax;
				UpdateUIRight(indexQuest);
				Debug.LogFormat(indexQuest.ToString());
			}
		}*/
		
	}

	public void Refresh(int index)
	{
		indexQuest = 0;
		content.DOLocalMoveY(index * 128, 2f);

		if (selectedQuest.unlockStatusFirst)
		{
			Title.text = selectedQuest.nameSid;
			Icon.sprite = selectedQuest.icon;
			Icon.gameObject.SetActive(true);
			Description.text = selectedQuest.descriptions[0];
			titles[0].text = selectedQuest.titles[0];
			images[0].gameObject.SetActive(false);
			indexMax = 0;

			if (selectedQuest.unlockStatusSecond)
			{
				titles[1].text = selectedQuest.titles[1];
				images[1].gameObject.SetActive(false);
				indexMax = 1;

				if (selectedQuest.unlockStatusThird)
				{
					titles[2].text = selectedQuest.titles[2];
					images[2].gameObject.SetActive(false);
					indexMax = 2;
				}
				else
				{
					titles[2].text = "";
					images[2].gameObject.SetActive(true);
				}
			}
			else
			{
				titles[1].text = "";
				images[1].gameObject.SetActive(true);

				titles[2].text = "";
				images[2].gameObject.SetActive(true);
			}
			UpdateUIRight(indexQuest);
		}
		else
		{
			UIClear();
		}
	}
	public void UIClear()
	{
		Title.text = "";
		Icon.gameObject.SetActive(false);
		Description.text = "";

		for (int i = 0; i <= 2; i++)
		{
			selectedImages[i].gameObject.SetActive(false);
			titles[i].text = "";
			images[i].gameObject.SetActive(true);
		}
	}
	

	public void UpdateUIRight(int index)
	{
		for(int i = 0; i <= indexMax;i++)
		{
			if(i == index)
			{
				selectedImages[i].gameObject.SetActive(true);
				titles[i].DOColor(Color.black, 1f);
			}
			else
			{
				selectedImages[i].gameObject.SetActive(false);
				titles[i].DOColor(Color.white, 1f);
			}
		}
		Icon.sprite = selectedQuest.images[index];
		Description.text = selectedQuest.descriptions[index];
	}
}
