using UnityEngine;
using System.Collections;

public class InteractLoadXml : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        InteractiveItemConfigManager.Instance.Load();
    }
}
