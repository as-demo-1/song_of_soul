using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TalkSOManager:MonoBehaviour
{

    public List<DialogueSectionSO> DialogueSectionListInstance = new List<DialogueSectionSO>();
    public List<DialogueStatusSO> DialogueStatusListInstance = new List<DialogueStatusSO>();
    public List<DialogueSO> DialogueListInstance = new List<DialogueSO>();


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


}
