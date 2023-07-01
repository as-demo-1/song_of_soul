using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterZone : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GetComponentInParent<BattleBlockManager>().TriggerBattle();
        }
        
    }
}
//这里是进入战斗场景下调取触发战斗，具体方法在manager里面