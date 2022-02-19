using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesLoad : MonoBehaviour
{
    public const string DialogueSOPath = "/Scripts/Dialogue/ScriptableObject/DialogueItem/";
    DialogueSO dialogueso;
    public static void resourcesload()
    {
        /*TalkSOManager.Instance.DialogueList = new List<DialogueSO>();
        dialogueso = Resources.Load<DialogueSO>(DialogueSOPath + "9001-9005.asset");
        //DialogueSO dialogue = Instantiate(dialogueso);
        TalkSOManager.Instance.DialogueList.Add(dialogueso);*/
    }
}
