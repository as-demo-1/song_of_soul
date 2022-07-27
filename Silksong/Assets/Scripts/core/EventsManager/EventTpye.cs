using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EventType
{
    onTakeDamage,
    onMakeDamage,
    onDie,

    //Enemy Behavious
    onEnemyHitWall,
    onItemChange,
    onMoneyChange,
    onKeyChange,//

    // store
    onOpenStore,
    onCloseStore
}

public class EventDate
{
    /*������ݵĲ���*/
    public float floatValue;
    public bool boolValue;
    public int intValue;

    public EventDate(bool boolValue=true,float floatValue=0f,int intValue=0)
    {
        this.floatValue = floatValue;
        this.intValue = intValue;
        this.boolValue = boolValue;
    }

}
