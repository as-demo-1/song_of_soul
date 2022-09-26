using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractManager
{
    public GameObject CollidingObject { get; set; }

    public static InteractManager Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = new InteractManager();
            }

            return s_instance;
        }
    }

    private static InteractManager s_instance;

    public void StopEvent()
    {
        //todo:停止键鼠监测事件 停止正在进行的动作 动画
        Debug.Log("StopEvent");
        PlayerInput.Instance.ToggleFrozen(true);
    }

    public void ContinueEvent()
    {
        //todo:继续键鼠监测事件 继续原来的事件
        Debug.Log("ContinueEvent");
        PlayerInput.Instance.ToggleFrozen(false);
    }
}
