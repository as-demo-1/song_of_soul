using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/*
缓存池管理器
作用：避免大量实例和销毁化物体，减少GC次数
方法：
1.从预制物实例化缓存池（同步和异步加载）
2.缓存池分类存取
3.缓存池清空
*/

/// <summary>
/// 内部类，定义了同类GameObject使用的容器
/// </summary>
public class ObjList {
    private GameObject myObj; // 用一个空物体作为容器内obj的父物体，方便分类
    public List<GameObject> objList; // 保存obj的容器

    /// <summary>
    /// 初始化容器，创建容器并push最初的obj
    /// </summary>
    /// <param name="gameObj">初始化容器时最初的GameObject</param>
    /// <param name="fatherObj">容器物体的父物体</param>
    public ObjList (GameObject gameObj, GameObject fatherObj) {
        myObj = new GameObject (gameObj.name);
        myObj.transform.SetParent (fatherObj.transform, false);
        objList = new List<GameObject> ();
        PushPool (gameObj);
    }

    /// <summary>
    /// 获取容器内的obj
    /// </summary>
    /// <returns>GameObject</returns>
    public GameObject GetObj () {
        GameObject gameObj;
        gameObj = objList[0];
        objList.RemoveAt (0);
        gameObj.transform.SetParent (null, false);
        gameObj.SetActive (true);
        return gameObj;
    }

    /// <summary>
    /// 把GameObject放回容器
    /// </summary>
    /// <param name="gameObj">需要放入容器的GameObject</param>
    public void PushPool (GameObject gameObj) {
        gameObj.transform.SetParent (myObj.transform, false);
        gameObj.SetActive (false);
        objList.Add (gameObj);
    }
}

/// <summary>
/// 缓存池的管理者
/// </summary>
public class BufferPoolManager : Singleton<BufferPoolManager> {
    private Dictionary<string, ObjList> bufferPool; // 缓存池容器
    private Dictionary<string, string> objPath; // 按预制物的名称存储路径
    public Transform poolObj;
    private string name;

    /// <summary>
    /// 初始化，创建容器并解析保存预制物信息的Json
    /// </summary>
    public BufferPoolManager () {
        bufferPool = new Dictionary<string, ObjList> ();
        ParseObjPathJson ();
    }

    /// <summary>
    /// 从资源加载模块同步加载配置文件
    /// </summary>
    private void ParseObjPathJson () {
        objPath = new Dictionary<string, string> ();
        TextAsset objPathJson = ResourceManager.Instance.Load<TextAsset> ("ObjPath");
        Debug.Log (objPathJson.text);
        ObjPathList objPathList = JsonUtility.FromJson<ObjPathList> (objPathJson.text);
        //Debug.Log ("GetJson");
        foreach (ObjPathInfo info in objPathList.pathInfoList) {
            objPath.Add (info.objName, info.objPath);
            //Debug.Log (info.objName);
        }
    }

    /// <summary>
    /// 按名称从缓存池获取实例
    /// </summary>
    /// <param name="objName">需要获取的对象名称</param>
    /// <returns>GameObject</returns>
    public GameObject GetObj (string objName) {
        GameObject obj = null;
        if (bufferPool.ContainsKey (objName)) {
            obj = bufferPool[objName].GetObj ();
        } else {
            // 如果缓存池内没有实例，就取预制物实例化一个
            string path = objPath[objName];
            obj = ResourceManager.Instance.Load<GameObject> (path);
            obj.name = objName;
        }
        //Debug.Log ("从缓冲池获取了" + objName);
        return obj;
    }

    /// <summary>
    /// 异步加载实例,加载出来后执行action函数。异步加载需要注意资源争用问题
    /// </summary>
    /// <param name="objName">需要获取的对象名称</param>
    /// <param name="action">需要执行的回调函数</param>
    public void GetObjAsyn (string objName, UnityAction<GameObject> action) {
        if (bufferPool.ContainsKey (objName)) {
            action (bufferPool[objName].GetObj ()); // 如果缓存池有东西，直接给外部方法调用
        } else {
            // 如果缓存池内没有实例，就取预制物实例化一个,并使用回调函数
            string path = objPath[objName];
            ResourceManager.Instance.LoadAsyn<GameObject> (path, (obj) => {
                obj.name = objName;
                action (obj);
            });
        }
        //Debug.Log ("从缓冲池获取了" + objName);
    }

    /// <summary>
    /// 把实例放入缓存池
    /// </summary>
    /// <param name="objName">需要进入的缓存池名称</param>
    /// <param name="obj">需要进入缓存池的对象</param>
    public void PushPool (string objName, GameObject obj) {
        if (poolObj == null) {
            poolObj = new GameObject ("pool").transform;
        }
        if (bufferPool.ContainsKey (objName)) {
            bufferPool[objName].PushPool (obj);
        } else {
            bufferPool.Add (objName, new ObjList (obj, poolObj.gameObject));
        }
    }

    /// <summary>
    /// 清空缓存池
    /// </summary>
    public void ClearPool () {
        bufferPool.Clear ();
        poolObj = null;
    }
}