using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 需要快速点按多次，进度条充满后调用完成回调的按钮
/// </summary>
public class QuickPressBtn : MonoBehaviour
{
    [SerializeField]
    private EasySlider slider;
    private bool isFinished;

    /// <summary>
    /// 快速点按进度增长速度
    /// </summary>
    [SerializeField]
    private float upSpeed;
    
    /// <summary>
    /// 进度条自然减少的速度
    /// </summary>
    [SerializeField]
    private float downSpeed;
    private float timer;

    public UnityEvent finishEvent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFinished)
        {
            if(timer > 1.0f)
            {
                isFinished = true;
                finishEvent.Invoke();
            }
            else if (timer > 0.0f)
            {
                timer -= Time.deltaTime * downSpeed;
                slider.SetValue(timer);
            }
            
        }
    }
    public void OnPress()
    {
        if (isFinished) return;
        
        timer += upSpeed;
        slider.SetValue(timer);
    }
}
