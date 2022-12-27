using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourceManager : Singleton<ResourceManager>
{
    public T Load<T>(string path) where T:Object
    {
        T obj = Resources.Load<T>(path);
        if(obj is GameObject)
        {
           return GameObject.Instantiate(obj);
        }
        else return obj;
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <param name="action">获取到资源后的行为</param>
    /// <typeparam name="T">需要获取的资源类型</typeparam>
    public void LoadAsyn<T>(string path,UnityAction<T> action) where T:Object
    {
        MonoManager.Instance.StartCoroutine(LoadAsynIE<T>(path,action));
    }

    private IEnumerator LoadAsynIE<T>(string path, UnityAction<T> callback) where T:Object
    {
        ResourceRequest res = Resources.LoadAsync<T>(path); // 异步加载资源
        yield return res; // 等待资源好了
        if(res.asset is GameObject) // 如果资源是GameObject就实例化并回调
        {
            callback(GameObject.Instantiate(res.asset) as T);
        }
        else
        {
            callback(res.asset as T);
        }
    }
}
