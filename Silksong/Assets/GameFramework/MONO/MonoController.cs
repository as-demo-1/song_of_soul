using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class MonoController : MonoBehaviour
{
    private event UnityAction updateEvent;
    private event UnityAction fixedUpdateEvent;
    public object innerInfo;

    private void Start() {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update() {
        if(updateEvent != null)
        {
            updateEvent.Invoke();
        }
    }

    private void FixedUpdate() {
        if(fixedUpdateEvent != null)
        {
            fixedUpdateEvent.Invoke();
        }
    }

    public void AddUpdateEvent(UnityAction action)
    {
        updateEvent += action;
    }

    public void RemoveUpdateEvent(UnityAction action)
    {
        updateEvent -= action;
    }

    public void AddFixedUpdateEvent(UnityAction action)
    {
        fixedUpdateEvent += action;
    }

    public void RemoveFixedUpdateEvent(UnityAction action)
    {
        fixedUpdateEvent -= action;
    }
}
