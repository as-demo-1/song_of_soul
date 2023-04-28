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

	public Image icon;
	public int Id;
	public Text nameText;

	private Item item;

	public UIQuestView questView;
	public string nameSid;
	public Transform content;

	public bool unlockStatusFirst;
	public bool unlockStatusSecond;
	public bool unlockStatusThird;

	public string unlockCondition;
	public string unlockDescription;

	public string firstTitle;
	public Sprite firstImage;
	public string firstDescription;

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
    public void Init(Item Item, UIQuestView QuestView,int Index,Transform transform)
    {
		item = Item;
		/*unlockStatus = (bool)item.GetAttribute("Unlock Status").GetOverrideValueAsObject();
		unlockStatusFirst = (bool)item.GetAttribute("Unlock Status First").GetOverrideValueAsObject();
		unlockStatusSecond = (bool)item.GetAttribute("Unlock Status Second").GetOverrideValueAsObject();
		unlockStatusThird = (bool)item.GetAttribute("Unlock Status Third").GetOverrideValueAsObject();*/
		questView = QuestView;
		item.TryGetAttributeValue<string>("NameSid", out var NameSid);
		nameSid = NameSid;
		item.TryGetAttributeValue<Sprite>("Icon", out var Icon);
		icon.sprite = Icon;

		/*
		item.TryGetAttributeValue<int>("NextId", out var NextId);
		item.TryGetAttributeValue<string>("UnlockCondition", out var UnlockCondition);
		unlockCondition = UnlockCondition;
		item.TryGetAttributeValue<string>("UnlockDescription", out var UnlockDescription);
		unlockDescription = UnlockDescription;
		item.TryGetAttributeValue<string>("FirstTitle", out var FirstTitle);
		firstTitle = FirstTitle;
		item.TryGetAttributeValue<Sprite>("FirstImage", out var FirstImage);
		firstImage = FirstImage;
		item.TryGetAttributeValue<string>("FirstDescription", out var FirstDescription);
		firstDescription = FirstDescription;
		item.TryGetAttributeValue<string[]>("FirstItems", out var FirstItems);
		item.TryGetAttributeValue<string>("SecondTitle", out var SecondTitle);
		secondTitle = SecondTitle;
		item.TryGetAttributeValue<Sprite>("SecondImage", out var SecondImage);
		secondImage = SecondImage;
		item.TryGetAttributeValue<string>("SecondDescription", out var SecondDescription);
		secondDescription = SecondDescription;
		item.TryGetAttributeValue<string[]>("SecondItems", out var SecondItems);
		item.TryGetAttributeValue<string>("ThirdTitle", out var ThirdTitle);
		thirdTitle = ThirdTitle;
		item.TryGetAttributeValue<Sprite>("ThirdImage", out var ThirdImage);
		thirdImage = ThirdImage;
		item.TryGetAttributeValue<string>("ThirdDescription", out var ThirdDescription);
		thirdDescription = ThirdDescription;
		item.TryGetAttributeValue<string[]>("ThirdItems", out var ThirdItems);*/

		nameText.text = nameSid;
		content = transform;
		index = Index;
	}
	public void OnSelect(BaseEventData eventData)
	{
		questView.selectedQuest = this;
		questView.content = content;
		questView.Refesh(index);
	}
}
