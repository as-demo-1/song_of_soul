using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JudgeInteract : InteractTriggerBase<JudgeInteract>
{
    protected override void InteractEvent()
    {
        Debug.Log("判断框");
        JudgeInteractiveSO ji = (InteractManager.Instance.InteractiveItem as JudgeInteractiveSO);
        setText(ji.Tip);
        Toggle("Judge", true);
        UIAddListener("Judge/Image/LButton", () => {
            ji.Yes();
            ContinueEvent();
        });
        UIAddListener("Judge/Image/RButton", () => {
            ji.No();
            ContinueEvent();
        });
    }

    private void setText(string txt)
    {
        m_UI_trans.Find("Judge/Image/Content").gameObject.GetComponent<Text>().text = txt;
    }

    protected override void ContinueEvent()
    {
        Toggle("Judge", false);
        base.ContinueEvent();
    }
}
