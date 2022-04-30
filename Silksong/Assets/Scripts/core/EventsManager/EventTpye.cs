using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EventType
{
<<<<<<< HEAD
    onTakeDamage,
    onMakeDamage,
    onDie,

    //Enemy Behavious
    onEnemyHitWall,

    onMoneyChange,
=======
    onTakeDamager,
    onMakeDamager,
    onDie
>>>>>>> 30f6fd9d (damage test)
}

public class EventDate
{
    /*允许传递的参数*/
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
