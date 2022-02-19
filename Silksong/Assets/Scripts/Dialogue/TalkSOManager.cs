using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkSOManager : MonoBehaviour
{
    public List<DialogueSectionSO> DialogueSectionList;
    public List<DialogueStatusSO> DialogueStatusList;
    public List<DialogueSO> DialogueList;

    private static TalkSOManager _instance;
    public static TalkSOManager Instance => _instance;

    private string FilePath;

    // Start is called before the first frame update
    public void Awake()
    {
        _instance = this;
    }

    private void OnEnable()
    {
        FilePath = Application.dataPath + "/Resources/AllDialogue.xlsx";
        ExcelLoad.ReadExcelStream(FilePath);
    }

}
