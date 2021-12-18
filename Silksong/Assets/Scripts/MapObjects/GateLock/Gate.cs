using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 机关门，场景加载时确定该门的状态。
/// </summary>作者：次元
public class Gate : MonoBehaviour
{
    bool gateOpen;
    // Start is called before the first frame update
    void Start()
    {
        if (gateOpen)
        {
            GateOpen();
            //TODO: 设置门动画的进入状态
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GateOpen()
    {
        GetComponent<Collider2D>().enabled = false;
        gateOpen = true;

        //TODO: 播放机关门打开（缓缓落下或上升）的动画
        //临时方案
        gameObject.SetActive(false);
    }
}
