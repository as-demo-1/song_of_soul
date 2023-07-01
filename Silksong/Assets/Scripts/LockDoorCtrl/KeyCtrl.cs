using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCtrl : MonoBehaviour
{

    //这里调取了钥匙，物品系统可以从这里调取
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            EventManager.Instance.Dispatch<int>(EventType.onKeyChange, 1);
            Destroy(this.gameObject);
        }
    }
}
