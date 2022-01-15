using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindObjectOfType<SfxManager>();

            if (instance != null)
                return instance;

            GameObject SfxManager = new GameObject("SfxManager");
            instance = SfxManager.AddComponent<SfxManager>();

            return instance;
        }
    }//单例

    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        gameObject.AddComponent<SfxManager>();
    }
    protected static SfxManager instance;

    public void creatHittedSfx(Vector2 hitPosition,Vector2 dirToHitPosition,SfxSO sfxSO)
    {
        //Debug.Log("creatSfx");
        GameObject[] temp = sfxSO.GetSfxs();
        foreach(GameObject t in temp)
        {
            GameObject sfx= Instantiate(t);
            sfx.transform.position = hitPosition;
            if(dirToHitPosition.x>0)//特效的默认攻击方向在左边，如果右边则反转
            {
                changeAllChlidSfxLocalscale(sfx);
            }
            Destroy(sfx, 1);
        }

    }

    protected void changeAllChlidSfxLocalscale(GameObject sfx)
    {
        foreach(var t in  sfx.GetComponentsInChildren<Transform>())
        {
            Vector3 temp = t.localScale;
            temp.x *= -1;
            t.localScale = temp;
        }
    }
}
