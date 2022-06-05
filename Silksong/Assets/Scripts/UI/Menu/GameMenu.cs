using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
/// <summary>
/// 游戏中的菜单，包括道具、护符、地图、成就等
/// </summary>作者：次元
public class GameMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject MainPanel;

    [SerializeField]
    private List<GameObject> panelList = new List<GameObject>();

    private bool isPlaying;
    private bool isShowing;

    private int currentPanelIndex = 2;

    // Start is called before the first frame update
    void Start()
    {
        MainPanel.transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerInput.Instance.charmMenu.Down)
        {
            if (!isPlaying && !isShowing)
            {
                isPlaying = true;
                MainPanel.transform.DOScale(1.0f, 0.5f).SetEase(Ease.InOutFlash).OnComplete(() => 
                { isShowing = true; isPlaying = false; });
                panelList[currentPanelIndex].SetActive(true);
            }
            else if (!isPlaying && isShowing)
            {
                isPlaying = true;
                MainPanel.transform.DOScale(0.0f, 0.5f).SetEase(Ease.InOutFlash).OnComplete(() => 
                { isShowing = false; isPlaying = false; });
                panelList[currentPanelIndex].SetActive(false);
            }
        }
        
    }
}
