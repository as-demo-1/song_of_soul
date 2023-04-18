using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class FireBullect : Action
{
    public GameObject BullectPrefab;
    private bool fire = false;
    private LaserBullect _bullect;

    public bool secfinalaction;
    public BullectController _BullectController;
 

    public int count = 10;

    public override void OnStart()
    {
        _BullectController.start(secfinalaction);
    }
    
  
    public float timer = 0f;
    private float timecount = 0f;

    void Func()
    {
        int x = Random.Range(-10, 10);
        int y = Random.Range(-10, 10);
        GameObject.Instantiate(BullectPrefab); 
      
    }
    
}
