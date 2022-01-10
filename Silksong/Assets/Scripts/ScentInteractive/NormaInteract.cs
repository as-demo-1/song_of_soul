using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NormaInteract : InteractTriggerBase<NormaInteract>
{
    private Transform m_UI_trans;

    protected override void InteractEvent()
    {
        if (m_UI_trans == null)
        {
            m_UI_trans = GameObject.Find("UI").transform;
        }
        normaInteractEvent();
    }

    private void normaInteractEvent()
    {
        // todo:
        // 具体事件
        switch (TriggerItemType)
        {
            case InteractiveItemType.NONE:
                Debug.Log("无界面");
                break;
            case InteractiveItemType.JUDGE:
                Debug.Log("判断框");
                Toggle("Judge", true);
                UIAddListener("Judge/Image/LButton", () => {
                    Toggle("Judge", false);
                    Debug.Log("yes");
                });
                UIAddListener("Judge/Image/RButton", () => {
                    Toggle("Judge", false);
                    Debug.Log("no");
                });
                break;
            case InteractiveItemType.FULLWINDOW:
                Debug.Log("全屏文本框");
                Toggle("FullWindowText", true);
                UIAddListener("FullWindowText/Image/Close", () => Toggle("FullWindowText", false));

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
                break;
            default:
                break;
        }
    }

    private void Toggle(string path, bool isActive)
    {
        m_UI_trans.Find(path).gameObject.SetActive(isActive);
    }

    private void UIAddListener(string path, UnityAction action)
    {
        GameObject go = m_UI_trans.Find(path).gameObject;

        if (go != null)
        {
            Button btn = go.GetComponent<Button>();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
    }
}
