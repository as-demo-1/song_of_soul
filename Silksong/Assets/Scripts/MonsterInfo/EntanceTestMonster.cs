using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntanceTestMonster : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MonsterInfoManager.Instance.Init();
        ItemTableSystem.Instance.Init();
        ResSystem.Instance.Init();
        Debug.Log("dsadsada");
        //DropManager.Instance.GetDrop4Monster("20000001");
        //DropManager.Instance.GetDrop4Monster("20000002");
    }
}
