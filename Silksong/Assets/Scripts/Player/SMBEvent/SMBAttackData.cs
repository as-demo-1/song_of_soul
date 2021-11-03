using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SMBAttackData : SMBEventTimeStamp
{
    //todo
    public AttackData attackData;
    public override void EventActive()
    {
        GameObject gameObject = new GameObject();
        PlayerController.Instance.StartCoroutine(GameObjectActiveForSeconds(gameObject, attackData.activeForSeconds));
        //gameObject.transform.SetParent(PlayerController.Instance.transform, false);
        ContactObject contactObject = gameObject.AddComponent<ContactObject>();
        contactObject.faction = ContactObject.EContactFaction.Player;
        contactObject.targetFactions = attackData.contactFactions;

        DamagerComponent damagerComponent = gameObject.AddComponent<DamagerComponent>();
        damagerComponent.sendDamageNum = attackData.damageNum;
        contactObject.AddContactComponent(damagerComponent);

        BoxCollider2D boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
        boxCollider2D.isTrigger = true;
        boxCollider2D.offset = attackData.offset + (Vector2)PlayerController.Instance.transform.position;
        boxCollider2D.size = attackData.size;
    }

    IEnumerator GameObjectActiveForSeconds(GameObject gameObject, float i)
    {
        yield return new WaitForSeconds(i);
        GameObject.Destroy(gameObject);
    }
}

[System.Serializable]
public class AttackData
{
    public float damageNum;
    public List<ContactObject.EContactFaction> contactFactions;
    public Vector2 offset;
    public Vector2 size;
    public float activeForSeconds;
}
