using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongPlatform : MonoBehaviour
{
    public float disappearTime;
    public bool ifDisappear;
    
    private void OnEnable()
    {
        if(ifDisappear)StartCoroutine(Disappear());
    }
    IEnumerator Disappear()
    {       
        float times = 0;
        while (times < disappearTime)
        {
            times += Time.deltaTime;
            yield return null;
        }
        GetComponent<Animator>().Play("disappear");
        GetComponent<Collider2D>().enabled = false;
    }
    public void SetActiveFalse() => gameObject.SetActive(false);
    public void StandAble() => GetComponent<Collider2D>().enabled = true;
}
