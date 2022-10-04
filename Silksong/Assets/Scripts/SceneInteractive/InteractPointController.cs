using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractPointController : MonoBehaviour
{
    public InactItemSO InactItem;

    void Start()
    {
        InactItem.AddGo(gameObject, true);
    }
}
