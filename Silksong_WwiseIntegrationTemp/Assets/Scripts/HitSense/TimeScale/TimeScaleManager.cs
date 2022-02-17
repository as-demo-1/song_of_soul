using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleManager : MonoBehaviour
{
    public static TimeScaleManager Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindObjectOfType<TimeScaleManager>();

            if (instance != null)
                return instance;

            GameObject sceneControllerGameObject = new GameObject("TimeScaleManager");
            instance = sceneControllerGameObject.AddComponent<TimeScaleManager>();

            return instance;
        }
    }//单例

    protected static TimeScaleManager instance;
    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void changeTimeScaleForFrames(int framesCount,float scale)
    {
        float time = framesCount / 60f;//默认60fps 需要修改
        changeTimeScaleForSeconds(time,scale);
    }

    public void changeTimeScaleForSeconds(float time,float scale)
    {
        StartCoroutine(IChangeTimeScaleForSeconds(time, scale));
    }
    private IEnumerator IChangeTimeScaleForSeconds(float time,float scale)
    {
        //print(time);
        Time.timeScale = scale;
        yield return new WaitForSecondsRealtime(time);
        Time.timeScale = 1;
    }
}
