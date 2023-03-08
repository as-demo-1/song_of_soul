using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 选项卡切换面板
/// </summary>
public class TabPanel : MonoBehaviour
{
    [Header("面板列表")]
    public List<BtnPanelPair> panelList = new List<BtnPanelPair>();

    public BtnPanelPair showingTab;

    public UnityEvent tabChange;
    // Start is called before the first frame update
    private void Awake()
    {
        
        for (int j = 0; j < panelList.Count; j++)
        {
            int i = j;
            panelList[j].btn.OnBtnClick.AddListener(()=>ShowTab(i));
            panelList[j].panel.gameObject.SetActive(false);
        }

        showingTab = panelList[0];
        ShowTab(0);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowTab(int _index)
    {
        //Debug.Log("显示第"+_index+"个tab");
        showingTab.panel.gameObject.SetActive(false);
        showingTab.btn.interactable = true;
        
        panelList[_index].panel.gameObject.SetActive(true);
        panelList[_index].btn.interactable = false;
        showingTab = panelList[_index];
        
        tabChange.Invoke();
    }

}
/// <summary>
/// 按钮与面板绑定
/// </summary>
[System.Serializable]
public class BtnPanelPair
{
    public EasyButton btn;
    public RectTransform panel;
}
