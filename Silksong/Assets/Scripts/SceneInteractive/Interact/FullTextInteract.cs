using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullTextInteract : InteractTriggerBase<FullTextInteract>
{
    protected override void InteractEvent()
    {
        Debug.Log("全屏文本框");
        Toggle("FullWindowText", true);
        UIAddListener("FullWindowText/Image/Close", () => {
            Toggle("FullWindowText", false);
            ContinueEvent();
        });

        GameObject txtWarp = m_UI_trans.Find("FullWindowText/Image/Text").gameObject;
        if (txtWarp == null)
        {
            Debug.LogError("txtWarp not found");
        }
        else
        {
            Text txt = txtWarp.GetComponent<Text>();
            if (txt == null)
            {
                Debug.LogError("m_text not found");
            }
            else
            {
                txt.text = "传说巴拉巴拉bsuikv建设大街发货 是个很好解释道佛规划普东街扶贫办圣诞节更方便传说巴拉巴拉bsuikv建设大街发货 是个很好解释道佛规划普东街扶贫办圣诞节更方便";
            }
        }
    }
}
