using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SMBAttackData : SMBEventTimeStamp
{
    //todo
    public float startTime;
    public float activeTime;
    public string damagerName;
    private GameObject damager;
    public override void EventActive()
    {
        //Debug.Log("event " + this.GetType());    
        if(damager==null)
        {
            damager = PlayerController.Instance.transform.Find(damagerName).gameObject;
            if (damager == null) 
                Debug.LogError("no this damagerName");
        }
        damager.transform.localScale = new Vector3(PlayerController.Instance.playerInfo.playerFacingRight ? 1 : -1, 1, 1);//damagerƒ¨»œ≥ØœÚ”“

        PlayerController.Instance.StartCoroutine(GameObjectActiveForSeconds(damager, startTime,activeTime));
      
    }

    IEnumerator GameObjectActiveForSeconds(GameObject gameObject, float startTime, float activeTime)
    {
        yield return new WaitForSeconds(startTime);
        gameObject.SetActive(true);
        yield return new WaitForSeconds(activeTime);
        gameObject.SetActive(false);
    }
}


