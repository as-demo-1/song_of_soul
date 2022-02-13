using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkSOManager : MonoBehaviour
{
    public DialogContainerSO DialogueContainer;
<<<<<<< Updated upstream
=======
    public List<DialogueStatusSO> DialogueStatusList;
    public List<DialogueSO> DialogueList;
>>>>>>> Stashed changes

    private static TalkSOManager _instance;
    public static TalkSOManager Instance => _instance;

<<<<<<< Updated upstream
    // Start is called before the first frame update
    void Awake()
=======
    private string FilePath;

    // Start is called before the first frame update
    public void Awake()
>>>>>>> Stashed changes
    {
        _instance = this;
    }

<<<<<<< Updated upstream
=======
    private void OnEnable()
    {
        FilePath = Application.dataPath + "/Resources/AllDialogue.xlsx";
        ExcelLoad.ReadExcelStream(FilePath);
    }

>>>>>>> Stashed changes
}
