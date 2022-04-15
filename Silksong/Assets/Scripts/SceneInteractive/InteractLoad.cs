using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractLoad : MonoBehaviour
{
    public InteractiveContainerSO InteractiveContainer;
    public GameObject ItemPrefab;
    public SaveSystem SaveSystem;

    // Use this for initialization
    void Start()
    {
        foreach (InteractiveBaseSO interactiveItem in InteractiveContainer.InteractiveItemList)
        {
            interactiveItem.Init(this);

            //Debug.Log(npcTalkController.DialogueSection);
            //TalkManager.Instance.NPCAllCondition[interactiveItem.ID] = TalkManager.Instance.TalkStatus;
        }
    }
}
