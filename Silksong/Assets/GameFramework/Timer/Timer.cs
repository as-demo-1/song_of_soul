using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;


public class Ticker
{
    private string _actionName;
    private uint _loopTime;
    private float _timePerLoop;
    private UnityAction _action;
    private float _passedTime;
    private float _loopCnt;
    
    // loopTime为0则无限循环
    public Ticker(String actionName, uint loopTime, float timePerLoop, UnityAction action)
    {
        _actionName = actionName;
        _loopTime = loopTime;
        _timePerLoop = timePerLoop;
        _action = action;
        _passedTime = 0;
        _loopCnt = 0;
    }
    
    public void Tick()
    {
        if (_loopCnt >= _loopTime && _loopTime != 0)
        {
            Timer.Instance.EndTickActionLoop(_actionName);
            return;
        }
        float dt = Time.deltaTime;
        _passedTime += dt;
        if (_passedTime >= _timePerLoop)
        {
            _action.Invoke();
            _passedTime -= _timePerLoop;
            _loopCnt++;
        }
    }
    
}

public class Timer : Singleton<Timer>
{
    private Dictionary<String, Ticker> _tikers = new Dictionary<String, Ticker>();
    // Start is called before the first frame update
    public void StartTickActionLoop(string actionName, uint loopTime, float tickTime, UnityAction action)
    {
        if (_tikers.ContainsKey(actionName))
        {
            Debug.Log(actionName + " is ticking!");
        }
        else
        {
            Ticker tiker = new Ticker(actionName, loopTime, tickTime, action);
            _tikers.Add(actionName, tiker);
        }
    }

    public void EndTickActionLoop(string actionName)
    {
        //Debug.LogError(actionName + " end ticking!");
        if (_tikers.ContainsKey(actionName))
        {
            _tikers.Remove(actionName);
        }
    }

    public void TimerUpdate()
    {
        foreach (var ticker in _tikers)
        {
            ticker.Value.Tick();
        }
    }
}
