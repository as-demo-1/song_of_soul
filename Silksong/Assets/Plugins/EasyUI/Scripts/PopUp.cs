using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 弹出窗口
/// </summary>
public class PopUp : MonoBehaviour
{
    [Header("停留时间")]
    [SerializeField]
    private float stayTime;

    [Header("结束位置")]
    public Vector3 endPos;

    [Header("起始位置")]
    public Vector3 startPos;

    private RectTransform rect;

    [SerializeField]
    private Text infoText;
    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        rect.localPosition = startPos;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Sequence seq;
    public void TriggerPopUp()
    {
        seq = DOTween.Sequence();
        MoveIn();
        seq.AppendInterval(stayTime);
        MoveOut();
    }

    public void MoveIn()
    {
        seq.Append(rect.DOLocalMove(endPos, 0.5f));
    }

    public void MoveOut()
    {
        seq.Append(rect.DOLocalMove(startPos, 0.5f));
    }

    public void SetInfo(string _info)
    {
        infoText.text = _info;
    }
}
