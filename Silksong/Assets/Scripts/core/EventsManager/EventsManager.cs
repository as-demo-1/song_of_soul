/// <summary>
/// Created by Kimo.               ----2021/08/29    
/// Update：添加了RemoveEvent方法  ---- 2021/08/29
/// Update：
///     1.添加了方法传参的功能
///     2.添加了泛型以支持添加任意返回值的方法
///                                ---- 2021/08/30
/// </summary>
/// 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Created by Kimo.
/// 用来实现对场景内（返回值为void类型）的所有事件的生成，储存，接收，激活。
/// </summary>
public class EventsManager
{   /*单例模式*/
    private static EventsManager instance = new EventsManager();
    public static EventsManager Instance
    {
        get { return instance; }
    }


    /*构造委托*/
    public delegate void voidDelegate();
    public delegate void ArgsDelegate(EventDate eventDate);



    private Dictionary<GameObject, Dictionary<EventType, Dictionary<ArgsDelegate,EventDate>>> argsEventDictionary;
    
    private Dictionary<GameObject, Dictionary<EventType, voidDelegate>> voidEventDictionary;
    private void DefaultFunction() { }
    private EventsManager()
    {
        voidEventDictionary = new Dictionary<GameObject, Dictionary<EventType, voidDelegate>>();

        argsEventDictionary = new Dictionary<GameObject, Dictionary<EventType, Dictionary<ArgsDelegate, EventDate>>>();
    }
    /// <summary>
    /// 为某物体的某事件类型添加某方法
    /// </summary>
    /// <param name="target">目标对象，一般是this.gameObject </param>
    /// <param name="eventType"> 事件类型，要是没有就自己添加</param>
    /// <param name="function"> 为该事件添加的方法</param>
    public void AddListener(GameObject target, EventType eventType, voidDelegate function)
    {
        if (!(voidEventDictionary.ContainsKey(target) && voidEventDictionary[target].ContainsKey(eventType)))
            CreatEvent(target, eventType);
        voidEventDictionary[target][eventType] += function;
    }
    /// <summary>
    /// 为某物体的某事件类型添加某方法
    /// </summary>
    /// <param name="target">目标对象，一般是this.gameObject </param>
    /// <param name="eventType"> 事件类型，要是没有就自己添加</param>
    /// <param name="function"> 为该事件添加的方法</param>
    /// <param name="Args">方法所需的参数，归一化为EventDate类型，可修改EventDate类自行添加</param>
    public void AddListener(GameObject target, EventType eventType, ArgsDelegate function, EventDate Args)
    {
        if (!(argsEventDictionary.ContainsKey(target) && argsEventDictionary[target].ContainsKey(eventType)))
            CreatEvent(target, eventType);
        argsEventDictionary[target][eventType].Add(function, Args);

    }
    /// <summary>
    /// 为某物体的某事件类型删去某方法
    /// </summary>
    /// <param name="target">目标对象，一般是this.gameObject </param>
    /// <param name="eventType"> 事件类型，要是没有就自己添加</param>
    /// <param name="function"> 为该事件添加的方法</param>
    public void RemoveListener(GameObject target, EventType eventType, voidDelegate function)
    {
        if (voidEventDictionary.ContainsKey(target) && voidEventDictionary[target].ContainsKey(eventType))
            voidEventDictionary[target][eventType] -= function;
    }
    public void RemoveListener(GameObject target, EventType eventType, ArgsDelegate function)
    {
        if (argsEventDictionary.ContainsKey(target) && argsEventDictionary[target].ContainsKey(eventType))
            if (argsEventDictionary[target][eventType].ContainsKey(function))
                argsEventDictionary[target][eventType].Remove(function);
    }
    /// <summary>
    /// 删去某物体的某事件类型所有注册的方法
    /// </summary>
    /// <param name="target">目标对象，一般是this.gameObject </param>
    /// <param name="eventType"> 事件类型，要是没有就自己添加</param>
    public void RemoveAllListener(GameObject target, EventType eventType)
    {
        if (voidEventDictionary.ContainsKey(target) && voidEventDictionary[target].ContainsKey(eventType))
        {
            System.Delegate[] delegates = voidEventDictionary[target][eventType].GetInvocationList();
            for (int i = 0; i < delegates.Length; i++)
                voidEventDictionary[target][eventType] -= delegates[i] as voidDelegate;
            voidEventDictionary[target][eventType] = new voidDelegate(DefaultFunction);
        }
        if (argsEventDictionary.ContainsKey(target) && argsEventDictionary[target].ContainsKey(eventType))
        {
            argsEventDictionary[target][eventType].Clear();
        }
    }
    /// <summary>
    /// （慎用）移除场景内所有事件，一般在场景切换时使用。
    /// </summary>
    public void RemoveAllEvent()
    {
        voidEventDictionary.Clear();
    }
    /// <summary>
    /// 移除目标身上所有事件
    /// </summary>
    /// <param name="target"></param>
    public void RemoveTargetAllEvent(GameObject target)
    {
        if (voidEventDictionary.ContainsKey(target))
        {
            voidEventDictionary.Remove(target);
        }
    }
    /// <summary>
    /// 触发目标对象的某事件类型所有注册的方法，设定触发完之后没有自动删去所有注册方法，需要手动删去
    /// </summary>
    /// <param name="target">目标对象 </param>
    /// <param name="eventType"> 事件类型，要是没有就自己添加</param>
    public void Invoke(GameObject target, EventType eventType)
    {
        if (voidEventDictionary.ContainsKey(target) && voidEventDictionary[target].ContainsKey(eventType))
        {
            voidEventDictionary[target][eventType].Invoke();
        }
        if (argsEventDictionary.ContainsKey(target) && argsEventDictionary[target].ContainsKey(eventType))
        {
           foreach(var value in argsEventDictionary[target][eventType].Keys)
            {
                value.Invoke(argsEventDictionary[target][eventType][value]);
            }
        }
    }
    /// <summary>
    ///为某物体创建某类型的事件
    /// </summary>
    /// <param name="target">目标对象，一般是this.gameObject </param>
    /// <param name="eventType"> 事件类型，要是没有就自己添加</param>
    public void CreatEvent(GameObject target, EventType eventType)
    {
        if (target == null)
        {
            Debug.LogError("Target gameobject is NULL");
            return;
        }
        if (!voidEventDictionary.ContainsKey(target))
            voidEventDictionary.Add(target, new Dictionary<EventType, voidDelegate>());
        if (!voidEventDictionary[target].ContainsKey(eventType))
            voidEventDictionary[target].Add(eventType, new voidDelegate(DefaultFunction));

        if(!argsEventDictionary.ContainsKey(target))
            argsEventDictionary.Add(target, new Dictionary<EventType, Dictionary<ArgsDelegate, EventDate>>());
        if(!argsEventDictionary[target].ContainsKey(eventType))
            argsEventDictionary[target].Add(eventType, new Dictionary<ArgsDelegate, EventDate>());
    }

}



/// <summary>
/// Created by Kimo.
/// 用来实现对场景内（返回值为T类型）的所有事件的生成，储存，接收，激活。
/// </summary>
public class EventsManager<T>
{   /*单例模式*/
    private static EventsManager<T> instance = new EventsManager<T>();
    public static EventsManager<T> Instance
    {
        get { return instance; }
    }


    /*构造委托*/
    public delegate T voidDelegate();
    public delegate T ArgsDelegate(EventDate eventDate);

    private Dictionary<GameObject, Dictionary<EventType, Dictionary<ArgsDelegate, EventDate>>> argsEventDictionary;

    private Dictionary<GameObject, Dictionary<EventType, voidDelegate>> voidEventDictionary;
    private T DefaultFunction() { return default(T); }
    private EventsManager()
    {
        voidEventDictionary = new Dictionary<GameObject, Dictionary<EventType, voidDelegate>>();

        argsEventDictionary = new Dictionary<GameObject, Dictionary<EventType, Dictionary<ArgsDelegate, EventDate>>>();
    }
    /// <summary>
    /// 为某物体的某事件类型添加某方法
    /// </summary>
    /// <param name="target">目标对象，一般是this.gameObject </param>
    /// <param name="eventType"> 事件类型，要是没有就自己添加</param>
    /// <param name="function"> 为该事件添加的方法</param>
    public void AddListener(GameObject target, EventType eventType, voidDelegate function)
    {
        if (!(voidEventDictionary.ContainsKey(target) && voidEventDictionary[target].ContainsKey(eventType)))
            CreatEvent(target, eventType);
        voidEventDictionary[target][eventType] += function;
    }
    /// <summary>
    /// 为某物体的某事件类型添加某方法
    /// </summary>
    /// <param name="target">目标对象，一般是this.gameObject </param>
    /// <param name="eventType"> 事件类型，要是没有就自己添加</param>
    /// <param name="function"> 为该事件添加的方法</param>
    /// <param name="Args">方法所需的参数，归一化为EventDate类型，可修改EventDate类自行添加</param>
    public void AddListener(GameObject target, EventType eventType, ArgsDelegate function, EventDate Args)
    {
        if (!(argsEventDictionary.ContainsKey(target) && argsEventDictionary[target].ContainsKey(eventType)))
            CreatEvent(target, eventType);
        argsEventDictionary[target][eventType].Add(function, Args);

    }
    /// <summary>
    /// 为某物体的某事件类型删去某方法
    /// </summary>
    /// <param name="target">目标对象，一般是this.gameObject </param>
    /// <param name="eventType"> 事件类型，要是没有就自己添加</param>
    /// <param name="function"> 为该事件添加的方法</param>
    public void RemoveListener(GameObject target, EventType eventType, voidDelegate function)
    {
        if (voidEventDictionary.ContainsKey(target) && voidEventDictionary[target].ContainsKey(eventType))
            voidEventDictionary[target][eventType] -= function;
    }
    public void RemoveListener(GameObject target, EventType eventType, ArgsDelegate function)
    {
        if (argsEventDictionary.ContainsKey(target) && argsEventDictionary[target].ContainsKey(eventType))
            if (argsEventDictionary[target][eventType].ContainsKey(function))
                argsEventDictionary[target][eventType].Remove(function);
    }
    /// <summary>
    /// 删去某物体的某事件类型所有注册的方法
    /// </summary>
    /// <param name="target">目标对象，一般是this.gameObject </param>
    /// <param name="eventType"> 事件类型，要是没有就自己添加</param>
    public void RemoveAllListener(GameObject target, EventType eventType)
    {
        if (voidEventDictionary.ContainsKey(target) && voidEventDictionary[target].ContainsKey(eventType))
        {
            System.Delegate[] delegates = voidEventDictionary[target][eventType].GetInvocationList();
            for (int i = 0; i < delegates.Length; i++)
                voidEventDictionary[target][eventType] -= delegates[i] as voidDelegate;
            voidEventDictionary[target][eventType] = new voidDelegate(DefaultFunction);
        }
        if (argsEventDictionary.ContainsKey(target) && argsEventDictionary[target].ContainsKey(eventType))
        {
            argsEventDictionary[target][eventType].Clear();
        }
    }
    /// <summary>
    /// （慎用）移除场景内所有事件，一般在场景切换时使用。
    /// </summary>
    public void RemoveAllEvent()
    {
        voidEventDictionary.Clear();
    }
    /// <summary>
    /// 移除目标身上所有事件
    /// </summary>
    /// <param name="target"></param>
    public void RemoveTargetAllEvent(GameObject target)
    {
        if (voidEventDictionary.ContainsKey(target))
        {
            voidEventDictionary.Remove(target);
        }
    }
    /// <summary>
    /// 触发某物体的某事件类型所有注册的方法，设定触发完之后没有自动删去所有注册方法，需要手动删去
    /// </summary>
    /// <param name="target">目标对象，一般是this.gameObject </param>
    /// <param name="eventType"> 事件类型，要是没有就自己添加</param>
    public void Invoke(GameObject target, EventType eventType)
    {
        if (voidEventDictionary.ContainsKey(target) && voidEventDictionary[target].ContainsKey(eventType))
        {
            voidEventDictionary[target][eventType].Invoke();
        }
        if (argsEventDictionary.ContainsKey(target) && argsEventDictionary[target].ContainsKey(eventType))
        {
            foreach (var value in argsEventDictionary[target][eventType].Keys)
            {
                value.Invoke(argsEventDictionary[target][eventType][value]);
            }
        }
    }
    /// <summary>
    ///为某物体创建某类型的事件
    /// </summary>
    /// <param name="target">目标对象，一般是this.gameObject </param>
    /// <param name="eventType"> 事件类型，要是没有就自己添加</param>
    public void CreatEvent(GameObject target, EventType eventType)
    {
        if (target == null)
        {
            Debug.LogError("Target gameobject is NULL");
            return;
        }
        if (!voidEventDictionary.ContainsKey(target))
            voidEventDictionary.Add(target, new Dictionary<EventType, voidDelegate>());
        if (!voidEventDictionary[target].ContainsKey(eventType))
            voidEventDictionary[target].Add(eventType, new voidDelegate(DefaultFunction));

        if (!argsEventDictionary.ContainsKey(target))
            argsEventDictionary.Add(target, new Dictionary<EventType, Dictionary<ArgsDelegate, EventDate>>());
        if (!argsEventDictionary[target].ContainsKey(eventType))
            argsEventDictionary[target].Add(eventType, new Dictionary<ArgsDelegate, EventDate>());
    }

}
