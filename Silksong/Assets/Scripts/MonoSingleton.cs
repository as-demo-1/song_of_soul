using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


public class MonoSingleton<T> : SerializedMonoBehaviour where T:MonoSingleton<T>
{
    private static T instance;
    public static T Instance {
        get {
            if (instance == null) 
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    new GameObject("MonoSingleton " + typeof(T)).AddComponent<T>();
                }
                else 
                {
                    instance.Init();   
                }
            }
            return instance;
        }
    }

    protected void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            instance.Init();
        }
    }

    public virtual void Init()
    { 
        
    }
}
