using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 用image实现的滑动调
/// </summary>
public class EasySlider : MonoBehaviour
{
    [SerializeField]
    [Range(0,1)]
    private float value;
    
    [SerializeField]
    private Image image;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        image.fillAmount = value;
    }
    
    public float GetValue()
    {
        return value;
    }

    public void SetValue(float _value, bool _withAnim = false)
    {
        if (_value > 1.0f)
        {
            value = 1.0f;
        }
        else if(_value < 0.0f)
        {
            value = 0.0f;
        }
        else
        {
            value = _value;
        }

        if (_withAnim)
        {
            
            image.DOFillAmount(value,0.5f);
        }
        else
        {
            image.fillAmount = value;
        }
    }
}
