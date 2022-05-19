using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwisePlayerLand : MonoBehaviour
{
    public AK.Wwise.Event MyEvent;
    // 使用此函数进行初始化。
    public void Wwiseplayerland()
    {
        MyEvent.Post(gameObject);
    }
}