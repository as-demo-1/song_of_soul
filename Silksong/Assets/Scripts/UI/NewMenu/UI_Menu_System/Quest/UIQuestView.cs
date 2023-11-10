using Language.Lua;
using Opsive.UltimateInventorySystem.UI.CompoundElements;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIQuestView : MonoBehaviour
{

	public GameObject selected;                 //选中的背景图

	public QuestSlot selectedQuest;             //选中的任务

	public QuestSlot[] mainQuestSlots;          //主线任务
	public QuestSlot[] branchQuestSlots;        //支线任务
	public QuestSlot[] legendQuestSlots;        //传说任务

	public Text Title;                          //任务名称
	public Image Icon;                          //右边栏任务图片
	public TextMeshProUGUI Description;         //解锁后的信息
	public Text requireText;                    //任务的目标UI文本
	public GameObject successedIcon;			//完成任务后图标

	int switchIndex = 0;                        //任务类型切换index
	public GameObject[] switchImage;            //任务类型切换时需要改变的Image
	public GameObject[] switchPanel;            //任务类型切换时需要改变的Panel

	int indexQuest = 0;                         //任务资料切换index
	public GameObject LockPanel;
	public GameObject[] rightSwitchImage;       //任务资料切换时需要改变的Image

	private void Awake()
	{
		selectedQuest = mainQuestSlots[0];
	}
	// Start is called before the first frame update
	private void Start()
	{
		mainQuestSlots[0].GetComponent<ActionButton>().Select();
	}
	void OnEnable()
	{
		//mainQuestSlots[0].GetComponent<ActionButton>().Select();
		Refresh(0);
	}
	/// <summary>
	/// 任务初始化
	/// </summary>
	public void Init()
	{
		//未激活任务
		string[] unassignedQuests = PixelCrushers.DialogueSystem.QuestLog.GetAllQuests(QuestState.Unassigned);
		foreach (var quest in unassignedQuests)
		{
			Debug.Log(quest);
			switch (DialogueLua.GetQuestField(quest, "questType").AsInt)
			{
				case 0:
					mainQuestSlots[DialogueLua.GetQuestField(quest, "Id").AsInt].Clear();
					mainQuestSlots[DialogueLua.GetQuestField(quest, "Id").AsInt].Init(quest, this);
					mainQuestSlots[DialogueLua.GetQuestField(quest, "Id").AsInt].GetComponent<ActionButton>().OnMoveE += (eventData, moved) => OnMove(eventData, moved);
					break;
				case 1:
					branchQuestSlots[DialogueLua.GetQuestField(quest, "Id").AsInt].Clear();
					branchQuestSlots[DialogueLua.GetQuestField(quest, "Id").AsInt].Init(quest, this);
					branchQuestSlots[DialogueLua.GetQuestField(quest, "Id").AsInt].GetComponent<ActionButton>().OnMoveE += (eventData, moved) => OnMove(eventData, moved);
					break;
				case 2:
					legendQuestSlots[DialogueLua.GetQuestField(quest, "Id").AsInt].Clear();
					legendQuestSlots[DialogueLua.GetQuestField(quest, "Id").AsInt].Init(quest, this);
					legendQuestSlots[DialogueLua.GetQuestField(quest, "Id").AsInt].GetComponent<ActionButton>().OnMoveE += (eventData, moved) => OnMove(eventData, moved);
					break;
			}
		}
		//已激活任务
		/*string[] activeQuests = PixelCrushers.DialogueSystem.QuestLog.GetAllQuests(QuestState.Active);
		
		foreach (var quest in activeQuests)
		{
			Debug.Log(quest);
			switch (DialogueLua.GetQuestField(quest, "questType").AsInt)
			{
				case 0:
					mainQuestSlots[DialogueLua.GetQuestField(quest, "Id").AsInt].Init(quest, this);
					break;
				case 1:
					branchQuestSlots[DialogueLua.GetQuestField(quest, "Id").AsInt].Init(quest, this);
					break;
				case 2:
					legendQuestSlots[DialogueLua.GetQuestField(quest, "Id").AsInt].Init(quest, this);
					break;
			}
		}

		string[] successQuests = PixelCrushers.DialogueSystem.QuestLog.GetAllQuests(QuestState.Success);

		foreach (var quest in activeQuests)
		{
			Debug.Log(quest);
			switch (DialogueLua.GetQuestField(quest, "questType").AsInt)
			{
				case 0:
					mainQuestSlots[DialogueLua.GetQuestField(quest, "Id").AsInt].Init(quest, this);
					break;
				case 1:
					branchQuestSlots[DialogueLua.GetQuestField(quest, "Id").AsInt].Init(quest, this);
					break;
				case 2:
					legendQuestSlots[DialogueLua.GetQuestField(quest, "Id").AsInt].Init(quest, this);
					break;
			}
		}*/
	}
	// Update is called once per frame

	void Update()
	{

		if (Input.GetKeyDown(KeyCode.J))
		{
			if (indexQuest == 0) return;

			if (indexQuest == 1)
			{
				Refresh(0);
			}
			else
			{
				Refresh(1);
			}
		}
		if (Input.GetKeyDown(KeyCode.K))
		{
			if (indexQuest == 2) return;

			if (indexQuest == 1)
			{
				Refresh(2);

			}
			else
			{
				Refresh(1);
			}
		}
	}

	private void OnMove(AxisEventData eventData, bool moved)
	{
		switch (eventData.moveDir)
		{
			case MoveDirection.Left:
				if (!moved) { PreviousSlice(); }
				break;
			case MoveDirection.Right:
				if (!moved) { NextSlice(); }
				break;
		}
	}
	public void PreviousSlice()
	{
		if (switchIndex == 0) return;
		
		int index = selectedQuest.Id / 4 * 4 + 3;
		RefreshUIRight(switchImage, switchPanel, false);

		if (switchIndex == 0)
		{
			mainQuestSlots[index].GetComponent<ActionButton>().Select();
		}
		else if (switchIndex == 1)
		{
			branchQuestSlots[index].GetComponent<ActionButton>().Select();
		}
	}
	public void NextSlice()
	{
		if (switchIndex == 2) return;			
		RefreshUIRight(switchImage, switchPanel, true);
		int index = selectedQuest.Id / 4 * 4;
		if (switchIndex == 1)
		{
			branchQuestSlots[index].GetComponent<ActionButton>().Select();
		}
		else if (switchIndex == 2)
		{
			legendQuestSlots[index].GetComponent<ActionButton>().Select();
		}
	}


	public void RefreshUIRight(GameObject[] images, GameObject[] panel,bool Add)
	{
		images[switchIndex].SetActive(false);
		panel[switchIndex].SetActive(false);
		if (Add)
		{
			switchIndex++;
		}
		else
		{
			switchIndex--;
		}
		images[switchIndex].SetActive(true);
		panel[switchIndex].SetActive(true);
	}

	/// <summary>
	/// 切换任务后刷新
	/// </summary>
	public void Refresh(int questState)
	{			
		Debug.Log("questStates"+selectedQuest.questStates[questState]);
		if(selectedQuest.questStates[questState] == QuestState.Unassigned)
		{
			Title.text = "";
			Icon.gameObject.SetActive(false);
			//requireText.text = "";
			LockPanel.SetActive(false);
			successedIcon.SetActive(false);
		}
		else if(selectedQuest.questStates[questState] == QuestState.Active)
		{

			Title.text = selectedQuest.nameSid;
			Icon.sprite = selectedQuest.icon;
			Icon.gameObject.SetActive(true);
			requireText.text = selectedQuest.requires[questState];
			LockPanel.SetActive(true);
			successedIcon.SetActive(false);
		}
		else if(selectedQuest.questStates[questState] == QuestState.Success)
		{
			Title.text = selectedQuest.nameSid;
			Icon.sprite = selectedQuest.icon;
			Icon.gameObject.SetActive(true);
			requireText.text = selectedQuest.requires[questState];
			LockPanel.SetActive(true);
			successedIcon.SetActive(true);
			//Description.text = selectedQuest.descriptions[0];
			//Description.gameObject.SetActive(true);
		}
		rightSwitchImage[indexQuest].SetActive(false);
		indexQuest = questState;
		rightSwitchImage[indexQuest].SetActive(true);
	}
}
