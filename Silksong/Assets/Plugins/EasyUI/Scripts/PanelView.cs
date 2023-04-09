using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// 队列选择功能
/// </summary>
public class PanelView : MonoBehaviour
{
    [Header("面板列表")]
    public List<RectTransform> panelList = new List<RectTransform>();

    [Header("当前选中的索引")]
    private int selectIndex;

    public int SelectIndex
    {
        get => selectIndex;
    }

    [FormerlySerializedAs("onPanelChange")] public UnityEvent OnPanelChange;
    /// <summary>
    /// 当前选中的面板
    /// </summary>
    public RectTransform selectPanel
    {
        get
        {
            if (selectIndex < 0 || selectIndex >= panelList.Count) return null;
            return panelList[selectIndex];
        }
    }

    public bool withAnim;

    [Header("x轴方向间距")]
    public float xGap;

    // Start is called before the first frame update
    void Start()
    {
        UpdatePanelPos();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectPre()
    {
        
        if (selectIndex > 0)
        {
            selectIndex--;
            OnPanelChange.Invoke();
            UpdatePanelPos();
        }

        
    }

    public void SelectNext()
    {
        if (selectIndex < panelList.Count-1)
        {
            selectIndex++;
            OnPanelChange.Invoke();
            UpdatePanelPos();
        }
    }

    public void UpdatePanelPos()
    {
        for (int i = 0; i < panelList.Count; i++)
        {
            if (withAnim)
            {
                panelList[i].DOLocalMove(new Vector3((i - selectIndex) * xGap, 0.0f, 0.0f), 0.5f);
                panelList[i].DOScale(new Vector3(1 - Mathf.Abs(i - selectIndex) * 0.1f
                    , 1 - Mathf.Abs(i - selectIndex) * 0.1f, 1.0f), 0.5f);
            }
            else
            {
                panelList[i].localPosition = new Vector3((i - selectIndex) * xGap, 0.0f, 0.0f);
                panelList[i].localScale = new Vector3(1 - Mathf.Abs(i - selectIndex) * 0.1f
                    , 1 - Mathf.Abs(i - selectIndex) * 0.1f, 1.0f);
            }
        }

    }
}
