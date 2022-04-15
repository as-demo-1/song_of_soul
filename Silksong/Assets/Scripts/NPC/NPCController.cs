using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum NPCStatus
{
    IDLE,
    START_RUN,
    RUN,
    END_RUN,
}

public class NPCController : MonoBehaviour
{
    private NPCStatus _status = NPCStatus.IDLE;
    private UnityAction _callback;
    private int _times = 30;

    void Update()
    {
        switch (_status)
        {
            case NPCStatus.IDLE:
                break;
            case NPCStatus.START_RUN:
                _status = NPCStatus.RUN;
                break;
            case NPCStatus.RUN:
                --_times;
                RunFn();
                if (_times == 0)
                {
                    _callback();
                    _status = NPCStatus.END_RUN;
                }
                break;
            case NPCStatus.END_RUN:
                _status = NPCStatus.IDLE;
                _times = 30;
                break;
            default:
                break;
        }
    }

    public void RunFn()
    {
        Vector3 tmpPos = transform.position;
        tmpPos.x -= 1;
        transform.position = tmpPos;
    }

    public void Run(UnityAction callback)
    {
        _callback = callback;
        _status = NPCStatus.START_RUN;
    }
}


