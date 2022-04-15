using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCtrl : MonoBehaviour
{


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            EventManager.Instance.Dispatch<int>(EventType.onKeyChange, 1);
            Destroy(this.gameObject);
        }
    }
}
