using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    public TutorialInfo info;

    [SerializeField]
    private Text tutorialTile;
    [SerializeField]
    private Text tutorialText;
    [SerializeField]
    private Image tutorialImage;

    //public TutorialPanelSO tutorialPanelSO;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void DisplayTutorial(TutorialInfo _info)
    {
        tutorialTile.text = _info.tutorialTitle;
        tutorialText.text = _info.tutorialStr;
        //TODO: Load image or video
    }
}
