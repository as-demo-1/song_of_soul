using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvokeTest : MonoBehaviour
{
    public GameObject[] target;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            EventsManager<int>.Instance.Invoke(target[0], EventType.onDie);
            EventsManager<bool>.Instance.Invoke(target[1], EventType.onDie);
            EventsManager.Instance.Invoke(target[0], EventType.onDie);
            EventsManager.Instance.Invoke(target[1], EventType.onDie);
        }
        if(Input.GetKeyDown(KeyCode.Backspace))
        {
            EventsManager.Instance.RemoveAllEvent();
        }
    }
}
