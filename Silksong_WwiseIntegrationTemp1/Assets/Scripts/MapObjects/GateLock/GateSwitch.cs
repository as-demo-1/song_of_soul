using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 机关门的开关，需要绑定对应的机关门。
/// 【缺陷】两者必须处于同一个场景
/// </summary>作者：次元
public class GateSwitch : MonoBehaviour
{
    private bool switchOpen;
    public Gate gate;

    // Start is called before the first frame update
    void Start()
    {
        if (switchOpen)
        {
            //TODO: 设置进入动画为机关已打开
            GetComponent<Collider2D>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchChange()
    {
        if (!switchOpen)
        {
            gate.GateOpen();
            //TODO: 播放机关拉杆的动画
            GetComponent<Animator>().SetTrigger("SwitchTrigger");
            //临时方案
            //GetComponent<SpriteRenderer>().flipX = true;
        }
        GetComponent<Collider2D>().enabled = false;
        switchOpen = true;      
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, gate.transform.position);
    }
}
