using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwisePlayerRun : MonoBehaviour
{
    public AK.Wwise.Event MyEvent;
   
    public void Wwiseplayerrun()
    {
        MyEvent.Post(gameObject);
    }
}