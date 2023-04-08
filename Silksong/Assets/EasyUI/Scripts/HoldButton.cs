using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class HoldButton : MonoBehaviour
{
    [SerializeField]
    private EasySlider slider;

    private bool isHolding;

    [Header("按住时间")]
    [SerializeField] 
    private float holdTime;

    private float timer;

    [Header("长按后回调")]
    /// <summary>
    /// 长按后回调
    /// </summary>
    public UnityEvent finishedEvent;

    private bool isFinished;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFinished)
        {
            if (isHolding)
            {
                timer += Time.deltaTime / holdTime;
                slider.SetValue(timer);
                if (timer > 1.0f)
                {
                    finishedEvent.Invoke();
                    isFinished = true;
                }
            }
            else
            {
                if (timer > 0.0f)
                {

                    timer -= Time.deltaTime / holdTime;
                    slider.SetValue(timer);
                }
            }
        }



    }

    public void ButtonChange(bool _option)
    {
        isHolding = _option;
    }
    
}
