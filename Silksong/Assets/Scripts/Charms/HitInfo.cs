using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HitInfo : MonoBehaviour
{
    public GameObject damageText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateText(DamagerBase damagerBase, DamageableBase damageableBase)
    {
        GameObject text = Instantiate(damageText, transform);
        text.transform.localPosition = Vector3.zero;;
        text.GetComponentInChildren<Text>().text = damagerBase.damage.ToString();
        text.transform.DOLocalMove(new Vector3(Random.Range(-1.0f,1.0f), 3.0f, 0.0f), 0.5f);
        Debug.Log(damagerBase.damage.ToString());
        Destroy(text, 1.0f);
    }
}
