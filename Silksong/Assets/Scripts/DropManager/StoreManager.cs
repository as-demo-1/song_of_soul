
using UnityEngine;
using System.Collections;

public class StoreManager : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        EventManager.Instance.Register(EventType.onOpenStore, openStore);
        EventManager.Instance.Register(EventType.onCloseStore, closeStore);
    }

    private void openStore()
    {
        Debug.Log("openStore");
    }

    private void closeStore()
    {
        Debug.Log("closeStore");
    }
}
