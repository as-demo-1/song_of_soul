using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleMonster_FSMManager :EnemyFSMManager
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer==LayerMask.NameToLayer("Gear"))
        {
            this.collision = collision;
            EventsManager.Instance.Invoke(this.gameObject, EventType.onEnemyHitWall);
        }

    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Gear"))
        {
            this.collision = collision;
            EventsManager.Instance.Invoke(this.gameObject, EventType.onEnemyHitWall);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerWeapon"))
        {
            this.triggerCollider = collision;
            EventsManager.Instance.Invoke(this.gameObject, EventType.onTakeDamager);

            Debug.Log("Collision Hitted");
        }
    }
}
