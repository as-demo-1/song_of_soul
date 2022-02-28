using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullTextInteract : InteractTriggerBase<FullTextInteract>
{
    protected override void InteractEvent()
    {
        Debug.Log("全屏文本框");
        FulltextInteractiveSO fi = (InteractManager.Instance.InteractiveItem as FulltextInteractiveSO);
        setText(fi.Tip);
        Toggle("FullWindowText", true);
        UIAddListener("FullWindowText/Image/Close", () => {
            ContinueEvent();
        });
    }

    private void setText(string txt)
    {
        m_UI_trans.Find("FullWindowText/Image/Text").gameObject.GetComponent<Text>().text = txt;
    }

    protected override void ContinueEvent()
    {
        Toggle("FullWindowText", false);
        base.ContinueEvent();
    }
}
