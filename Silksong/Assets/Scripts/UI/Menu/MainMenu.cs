using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public List<ListBtnInfo> btnInfos;

    public bool isEnter = false;

    GComponent _mainView;
    GList _list;
    OnSureWindow onSureWindow;
    void Awake()
    {
        btnInfos = new List<ListBtnInfo>(){
            new ListBtnInfo("开始游戏", NewGame),
            new ListBtnInfo("继续游戏", ContinueGame),
            new ListBtnInfo("选项", ShowSettings),
            new ListBtnInfo("额外内容", ShowExtra),
            new ListBtnInfo("退出游戏", ClickExit),
        };
    }

    void Start()
    {
        _mainView = this.GetComponent<UIPanel>().ui;
        _list = _mainView.GetChild("list_btn").asList;
        _list.itemRenderer = RenderListItem;
        _list.onClickItem.Add(onClickItem);
        _list.numItems = 5;
        EventCallback1 callback = this.onClickItem;
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (!isEnter)
            {
                isEnter = true;
                Controller _curController = _mainView.GetController("enter");
                _curController.selectedIndex = 1;
                Transition t1 = _mainView.GetTransition("t1");
                t1.Stop();
                Transition t0 = _mainView.GetTransition("t0");
                t0.Play();
            }
        }
    }

    void RenderListItem(int index, GObject obj)
    {
        obj.name = index.ToString();
        GComponent comp = obj.asCom;
        GTextField text = comp.GetChild("title").asTextField;
        text.text = btnInfos[index].text;
        obj.onRollOut.Add(onRollOut);
        obj.onRollOver.Add(onRollOver);
    }

    void onRollOver(EventContext context)
    {
        GComponent item = (GComponent)context.sender;
        Controller _curController = item.GetController("select");
        _curController.selectedIndex = 1;
    }

    void onRollOut(EventContext context)
    {
        GComponent item = (GComponent)context.sender;
        Controller _curController = item.GetController("select");
        _curController.selectedIndex = 0;
    }

    void onClickItem(EventContext context)
    {
        GObject item = (GObject)context.data;
        int index = int.Parse(item.name);
        ListBtnInfo info = btnInfos[index];
        info.callback.Invoke(index);
    }

    void NewGame(int index)
    {
        Debug.Log("on click New Game");
        SceneManager.LoadScene("Level1-1");
        UIManager.Instance.ShowGameUI();
    }

    void ContinueGame(int index)
    {
        Debug.Log("on click continue game");
        SceneManager.LoadScene("Level1-1");
        UIManager.Instance.ShowGameUI();
    }

    void ShowSettings(int index)
    {
        Debug.Log("on click show settings");
    }

    void ShowExtra(int index)
    {
        Debug.Log("on click show extra");
    }

    void ClickExit(int index)
    {
        Debug.Log("on click exit");
        onSureWindow = new OnSureWindow("", "确认退出游戏?", (()=>{
            Application.Quit();
        }), (()=>{onSureWindow.Hide();}));
        onSureWindow.Show();
    }
}

public class ListBtnInfo
{
    public string text;
    public UnityAction<int> callback;
    public ListBtnInfo(string text, UnityAction<int> callback)
    {
        this.text = text;
        this.callback = callback;
    }
}
