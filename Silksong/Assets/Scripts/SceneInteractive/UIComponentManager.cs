using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIComponentManager
{
    private static UIComponentManager _instance;
    public static UIComponentManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UIComponentManager();
            }
            return _instance;
        }
    }

    private Transform _ui_trans;
    public Transform UI_trans
    {
        get
        {
            if (_ui_trans == null)
            {
                _ui_trans = GameObject.Find("UI").transform;
            }
            return _ui_trans;
        }
    }

    public GameObject FindUI(string path)
    {
        Transform tmpTrans = UI_trans.Find(path);

        if (tmpTrans)
        {
            return tmpTrans.gameObject;
        }
        else
        {
            return null;
        }
    }

    public GameObject FindUI(GameObject parent, string path)
    {
        Transform tmpTrans = parent.transform.Find(path);

        if (tmpTrans)
        {
            return tmpTrans.gameObject;
        }
        else
        {
            return null;
        }
    }

    public bool UIAddListener(string path, UnityAction action)
    {
        GameObject go = FindUI(path);

        if (go != null)
        {
            Button btn = go.GetComponent<Button>();

            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(action);
                return true;
            }
            Debug.LogError(path + "未找到Button组件");
        }
        Debug.LogError(path + "未找到gameObject");
        return false;
    }

    public bool UIAddListener(GameObject parent, string path, UnityAction action)
    {
        GameObject go = FindUI(parent, path);

        if (go != null)
        {
            Button btn = go.GetComponent<Button>();

            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(action);
                return true;
            }
            Debug.LogError(path + "未找到Button组件");
        }
        Debug.LogError(path + "未找到gameObject");
        return false;
    }

    public bool SetText(string path, string txt)
    {
        GameObject go = FindUI(path);

        if (go != null)
        {
            Text text = go.GetComponent<Text>();

            if (text != null)
            {
                text.text = txt;
                return true;
            }
            Debug.LogError(path + "未找到Text组件");
        }
        Debug.LogError(path + "未找到gameObject");
        return false;
    }

    public bool SetText(GameObject parent, string path, string txt)
    {
        GameObject go = FindUI(parent, path);

        if (go != null)
        {
            Text text = go.GetComponent<Text>();

            if (text != null)
            {
                text.text = txt;
                return true;
            }
            Debug.LogError(path + "未找到Text组件");
        }
        Debug.LogError(path + "未找到gameObject");
        return false;
    }

    // =================

    public bool ShowUI(string path)
    {
        return toggleUIActive(path, true);
    }

    public bool HideUI(string path)
    {
        return toggleUIActive(path, false);
    }

    private bool toggleUIActive(string path, bool isActive)
    {
        GameObject go = FindUI(path);

        if (go != null)
        {
            go.SetActive(isActive);
            return true;
        }
        Debug.LogError(path + "未找到gameObject");
        return false;
    }
}
