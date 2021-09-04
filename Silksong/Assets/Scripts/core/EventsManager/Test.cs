using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
/// <summary>
/// 这是关于EventsManager的测试程序。
/// 
/// </summary>
public class Test : MonoBehaviour
{
    void Start()
    {
        EventsManager.Instance.CreatEvent(gameObject, EventType.onDie);
        EventsManager.Instance.AddListener(gameObject, EventType.onDie, debug1, new EventDate( true,1.222f));
        EventsManager<int>.Instance.AddListener(gameObject, EventType.onDie, debug2);
        EventsManager<bool>.Instance.AddListener(gameObject, EventType.onDie, debug3, new EventDate(true, 1.222f));
    }
    void debug1(EventDate date)
    {
        Debug.Log("111" + this.gameObject.name+date.floatValue.ToString());

    }

    int debug2()
    {
        Debug.Log("222");
        return 1;
    }
    bool debug3(EventDate date)
    {
        Debug.Log("333");
        return date.boolValue;
    }
    void Update()
    {
      
    }

}
