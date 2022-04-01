using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TalkSOManager:MonoBehaviour
{
    /*public static List<DialogueSectionSO> DialogueSectionList;
    public static List<DialogueStatusSO> DialogueStatusList;
    public static List<DialogueSO> DialogueList;*/

    public List<DialogueSectionSO> DialogueSectionListInstance = new List<DialogueSectionSO>();
    public List<DialogueStatusSO> DialogueStatusListInstance = new List<DialogueStatusSO>();
    public List<DialogueSO> DialogueListInstance = new List<DialogueSO>();

    //public DialogueContainerSO Container;

    private static TalkSOManager _instance;
    public static TalkSOManager Instance => _instance;

    private string FilePath;

    public void Awake()
    {
        _instance = this;
        //Debug.Log(TalkSOManager.Instance.DialogueSectionListInstance);
        ExcelLoad.ReadExcelStream();
        DialogueSO.MultyCreateSO();
        DialogueStatusSO.CreateStatusSO();
        DialogueSectionSO.CreateSectionSO();
        ExcelLoad.Load();
        }

    // Start is called before the first frame update
    /*[MenuItem("Dialogue/Awakelist/Awake",false,1)]
    public static void AwakeList()
    {
        DialogueList = new List<DialogueSO>();
        DialogueSectionList = new List<DialogueSectionSO>();
        DialogueStatusList = new List<DialogueStatusSO>();

        Instance = new TalkSOManager();
    
    }*/

}
