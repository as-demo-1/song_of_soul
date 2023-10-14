using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.DataStructures;
using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuestSlot : MonoBehaviour, ISelectHandler
{

	public Image iconImage;
	public Sprite questBgIcon;
	public GameObject selected;

	[HideInInspector]
	public int Id;
	[HideInInspector]
	public UIQuestView questView;
	[HideInInspector]
	public string nameSid;                                          //任务名称
	[HideInInspector]
	public Sprite icon;                                             //任务图片
	[HideInInspector]
	public int questStage;                                          //任务进度
	[HideInInspector]
	public List<string> requires = new List<string>();             //完成任务的条件
	[HideInInspector]
	public List<Sprite> images = new List<Sprite>();               //任务完成后的图片
	[HideInInspector]
	public List<string> descriptions = new List<string>();          //任务完成后的资料

	string state;                                                   //任务状态

	/// <summary>
	/// 未完成
	/// </summary>
	/// <param name="Item"></param>
	/// <param name="QuestView"></param>
	/// <param name="Index"></param>
	/// <param name="transform"></param>
	public void Init(string questName, UIQuestView QuestView)
	{
		questView = QuestView;

		state = DialogueLua.GetQuestField(questName, "State").AsString;

		nameSid = DialogueLua.GetQuestField(questName, "Name").AsString;

		icon = ResourceManager.Instance.Load<Sprite>(DialogueLua.GetQuestField(questName, "Pictures").AsString);
		//Id = ID;

		questStage = DialogueLua.GetQuestField(questName, "QuestStage").AsInt;

		requires.Add(DialogueLua.GetQuestField(questName, "FirstRequire").AsString);
		//images.Add(ResourceManager.Instance.Load<Sprite>(DialogueLua.GetQuestField(questName, "FirstImage").AsString));
		descriptions.Add(DialogueLua.GetQuestField(questName, "FirstDescription").AsString);

		ResourceManager.Instance.Load<Sprite>("aaa");
		requires.Add(DialogueLua.GetQuestField(questName, "SecondRequire").AsString);
		//images.Add(ResourceManager.Instance.Load<Sprite>(DialogueLua.GetQuestField(questName, "SecondImage").AsString));
		descriptions.Add(DialogueLua.GetQuestField(questName, "SecondDescription").AsString);

		requires.Add(DialogueLua.GetQuestField(questName, "ThirdRequire").AsString);
		//images.Add(ResourceManager.Instance.Load<Sprite>(DialogueLua.GetQuestField(questName, "ThirdImage").AsString));
		descriptions.Add(DialogueLua.GetQuestField(questName, "ThirdDescription").AsString);


		iconImage.sprite = icon;
	}
	public void OnSelect(BaseEventData eventData)
	{

		questView.selectedQuest = this;
		questView.Refresh();
		questView.selected.SetActive(true);
		questView.selected.transform.DOLocalMove(this.transform.localPosition, 0.5f);
	}
}
