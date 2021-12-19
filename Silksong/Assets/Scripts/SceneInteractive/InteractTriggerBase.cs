using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class InteractTriggerBase<T> where T: class, new()
{
    public bool IsOnTrigger;
    public EInteractiveItemType TriggerItemType;
    protected bool m_isOnInteract = false;
    protected static Transform m_UI_trans;

    private static T s_instance;
    public static T Instance
    {
        get
        {
            if (s_instance == null)
                s_instance = new T();

            if (m_UI_trans == null)
            {
                m_UI_trans = GameObject.Find("UI").transform;
            }

            return s_instance;
        }
    }

    public void Interact()
    {
        if (m_isOnInteract || !IsOnTrigger)
            return;

        StopEvent();
        InteractEvent();
    }
    protected abstract void InteractEvent();
    protected virtual void StopEvent() {
        //todo:停止键鼠监测事件 停止正在进行的动作 动画
        Debug.Log("StopEvent");
        PlayerInput.Instance.ToggleFrozen(true);
        m_isOnInteract = true;
    }
    public virtual void ContinueEvent()
    {
        //todo:继续键鼠监测事件 继续原来的事件
        Debug.Log("ContinueEvent");
        PlayerInput.Instance.ToggleFrozen(false);
        m_isOnInteract = false;
    }


    protected void UIAddListener(string path, UnityAction action)
    {
        GameObject go = m_UI_trans.Find(path).gameObject;

        if (go != null)
        {
            Button btn = go.GetComponent<Button>();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
    }

    protected void Toggle(string path, bool isActive)
    {
        m_UI_trans.Find(path).gameObject.SetActive(isActive);
    }
}
