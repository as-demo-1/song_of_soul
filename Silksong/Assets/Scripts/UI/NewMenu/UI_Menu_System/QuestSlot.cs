using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.DataStructures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuestSlot : MonoBehaviour, ISelectHandler
{
	public int index;

	public Image iconImage;
	public int Id;
	public Text IdText;
	public Text nameText;


	private Item item;

	public UIQuestView questView;
	public Transform content;



	public string nameSid;
	public Sprite icon;

	public bool unlockStatusFirst;
	public bool unlockStatusSecond;
	public bool unlockStatusThird;

	public List<string> titles = new List<string> ();
	public List<Sprite> images = new List<Sprite> ();
	public List<string> descriptions = new List<string> ();



	public string secondTitle;
	public Sprite secondImage;
	public string secondDescription;

	public string thirdTitle;
	public Sprite thirdImage;
	public string thirdDescription;
	// Start is called before the first frame update
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
	}
	/// <summary>
	/// Î´Íê³É
	/// </summary>
	/// <param name="Item"></param>
	/// <param name="QuestView"></param>
	/// <param name="Index"></param>
	/// <param name="transform"></param>
	public void Init(Item Item, UIQuestView QuestView, int Index, Transform transform)
	{
		item = Item;
		questView = QuestView;
		unlockStatusFirst = (bool)item.GetAttribute("UnlockStatusFirst").GetOverrideValueAsObject();
		unlockStatusSecond = (bool)item.GetAttribute("UnlockStatusSecond").GetOverrideValueAsObject();
		unlockStatusThird = (bool)item.GetAttribute("UnlockStatusThird").GetOverrideValueAsObject();

		item.TryGetAttributeValue<string>("NameSid", out var NameSid);
		nameSid = NameSid;
		item.TryGetAttributeValue<Sprite>("Icon", out var Icon);
		icon = Icon;
		item.TryGetAttributeValue<int>("NextId", out var NextId);
		item.TryGetAttributeValue<int>("ID", out var ID);
		Id = ID;

		item.TryGetAttributeValue<string>("FirstTitle", out var FirstTitle);
		titles.Add(FirstTitle);
		//titles[0] = FirstTitle;
		item.TryGetAttributeValue<Sprite>("FirstImage", out var FirstImage);
		images.Add(FirstImage);
		//images[0] = FirstImage;
		item.TryGetAttributeValue<string>("FirstDescription", out var FirstDescription);
		descriptions.Add(FirstDescription);
		//descriptions[0] = FirstDescription;

		item.TryGetAttributeValue<string>("SecondTitle", out var SecondTitle);
		titles.Add(SecondTitle);
		//titles[1] = SecondTitle;
		item.TryGetAttributeValue<Sprite>("SecondImage", out var SecondImage);
		images.Add(SecondImage);
		//images[1] = SecondImage;
		item.TryGetAttributeValue<string>("SecondDescription", out var SecondDescription);
		descriptions.Add(SecondDescription);
		//descriptions[1] = SecondDescription;
		item.TryGetAttributeValue<string>("ThirdTitle", out var ThirdTitle);
		titles.Add(ThirdTitle);
		//titles[2] = ThirdTitle;
		item.TryGetAttributeValue<Sprite>("ThirdImage", out var ThirdImage);
		images.Add(ThirdImage);
		//images[2] = ThirdImage;
		item.TryGetAttributeValue<string>("ThirdDescription", out var ThirdDescription);
		descriptions.Add(ThirdDescription);
		//descriptions[2] = ThirdDescription;
		
		content = transform;
		index = Index;
		if (unlockStatusFirst)
		{
			nameText.text = nameSid;
			iconImage.sprite = icon;
			IdText.text = "NO." + Id;
		}
	}
	public void OnSelect(BaseEventData eventData)
	{
		questView.selectedQuest = this;
		questView.content = content;
		questView.Refresh(index);
	}
}
