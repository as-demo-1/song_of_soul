using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 描述该教学弹窗的类型，REGION表示经过指定区域触发，ABILITY表示获得新能力时触发
/// </summary>
public enum TutorialType
{
    REGION, ABILITY
}

[System.Serializable]
/// <summary>
/// 教程信息，包含教程标题、说明文本、演示视频等。
/// </summary>
public class TutorialInfo
{
    [Header("教学标题")]
    public string tutorialTitle;
    [Header("教学内容文本")]
    [TextArea(4, 8)]
    public string tutorialStr;

    /// <summary>
    /// 该教学弹窗已被触发
    /// </summary>
    [SerializeField] private bool hasTriggered = false;

    public TutorialType tutorialType;

    public string imagePath;

    public TutorialInfo(string _title, string _str)
    {
        this.tutorialTitle = _title;
        this.tutorialStr = _str;
    }

    /// <summary>
    /// 查询教程触发情况
    /// </summary>
    /// <returns></returns>
    public bool PopTutorial()
    {
        if (this.hasTriggered)
        {
            return false;
        }
        else
        {
            this.hasTriggered = true;
            return true;
        }
    }
}

/// <summary>
/// 保存所有教学弹窗信息以及对应的图像视频配置，同时作为弹窗触发事件通道，事件由TutorialManager监听
/// </summary>
[CreateAssetMenu(fileName = "NewTutorial", menuName = "Tutorial Popup Window/New Tutorial")]
public class TutorialPanelSO : ScriptableObject
{
    public List<TutorialInfo> tutorialInfos = new List<TutorialInfo>();

    /// <summary>
    /// 自动生成的字典，key：教程标题，value：教程信息
    /// </summary>
    public Dictionary<string, TutorialInfo> TutorialDic = new Dictionary<string, TutorialInfo>();

    public UnityAction<string> OnTutorialRequested;

    public void GenerateDic()
    {
        foreach (TutorialInfo info in tutorialInfos)
        {
            TutorialDic[info.tutorialTitle] = info;
        }
    }
    public TutorialInfo GetInfo(string _title)
    {
        if (TutorialDic.ContainsKey(_title))
        {
            return TutorialDic[_title];
        }
        else
        {
            return new TutorialInfo("???????", "教学内容丢失，工具人上班啦！");
        }
    }
    public void RasieEvent(string _title)
    {
        if (OnTutorialRequested!= null)
        {
            OnTutorialRequested.Invoke(_title);
        }
        else
        {
            Debug.LogWarning("无监听，请检查教程管理器TutorialManager");
        }
    }
    
}
