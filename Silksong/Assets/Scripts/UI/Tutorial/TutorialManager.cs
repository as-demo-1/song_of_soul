using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TutorialPanelSO tutorialPanelSO;

    [SerializeField] private GameObject TutorialUIPrefab;

    private GameObject openPanel;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        tutorialPanelSO.GenerateDic();
        tutorialPanelSO.OnTutorialRequested += PopUpPanel;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PopUpPanel(string _title)
    {
        if (tutorialPanelSO.TutorialDic.ContainsKey(_title))
        {
            Debug.Log("Has key");
            if (tutorialPanelSO.TutorialDic[_title].PopTutorial())
            {
                openPanel = Instantiate(TutorialUIPrefab);
                openPanel.GetComponent<TutorialUI>().DisplayTutorial(tutorialPanelSO.GetInfo(_title));
            }  
        }
        

        //TODO:多弹窗队列
    }
    public void ClosePanel()
    {

    }
}
