using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwisePlayerSprint : MonoBehaviour
{
    public AK.Wwise.Event MyEvent;
    // 使用此函数进行初始化。
    public void Wwiseplayersprint()
    {
        MyEvent.Post(gameObject);
    }
}