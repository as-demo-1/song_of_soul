using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


#if UNITY_EDITOR
[CustomEditor(typeof(DamagerBase), true)]
#endif

public class FrozenArea : DamagerBase
{
    [Tooltip("每隔多久受到一次伤害")]
    public int stayTime = 1;



    private float recordTime = 0;   
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !ignoreInvincibility) 
        {
            recordTime = recordTime + Time.deltaTime;
            if (recordTime >= stayTime)
            {
                var damageable = collision.gameObject.GetComponent<DamageableBase>();
                damageable.takeDamage(this);
                makeDamage(damageable);
                recordTime = 0;
            }
        } 
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        recordTime = 0;
    }

    protected override void makeDamage(DamageableBase target)
    {

    }
}
