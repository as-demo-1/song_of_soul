using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 简易的开关按钮
/// </summary>
public class EasySwitch : MonoBehaviour
{
    
    private bool state;

    public bool State
    {
        get { return state; }
        set
        {
            state = value;
            SwitchChange();
        }
    }

    public RectTransform btn;

    public bool withAnim;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeState()
    {
        State = !State;
    }
    public void SwitchChange()
    {
        if (!withAnim)
        {
            Debug.Log("change");
            btn.localPosition = new Vector3(-btn.localPosition.x, btn.localPosition.y, btn.localPosition.z);
            btn.GetComponent<Image>().color = State? Color.green : Color.red;
        }
        else
        {
            btn.DOLocalMove(
                    new Vector3(-btn.localPosition.x, btn.localPosition.y
                        , btn.localPosition.z), 0.5f)
                .SetEase(Ease.InOutCubic);
            //btn.GetComponent<Image>().DOColor(State ? Color.green : Color.red, 0.5f);
        }
    }
}
