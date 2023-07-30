using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Random = UnityEngine.Random;

/// <summary>
/// 伤害跳字，用于测试buff效果
/// </summary>
public class HitInfo : MonoBehaviour
{
    public GameObject damageText;
    private HpDamable hpDamable;
    public void Awake()
    {
        hpDamable = GetComponent<HpDamable>();
        if (hpDamable!=null)
        {
            hpDamable.takeDamageEvent.AddListener(GenerateText);
        }
    }

    public void GenerateText(DamagerBase damagerBase, DamageableBase damageableBase)
    {
        GameObject text = Instantiate(damageText);
        text.transform.position = transform.position;
        text.GetComponentInChildren<Text>().text = damagerBase.damage.ToString();
        text.transform.DOMove(text.transform.position+new Vector3(Random.Range(-1.0f,1.0f), 1.5f+Random.Range(-1.0f,1.0f), 0.0f), 0.5f);
        //Debug.Log(damagerBase.damage.ToString());
        Destroy(text, 1.0f);
        
        // TODO:闪白特效
        // Sequence sequence = DOTween.Sequence();
        // sequence.AppendCallback(() =>
        // {
        //     GetComponent<SpriteRenderer>().material.SetFloat("_StrongTintFade", 1.0f);
        // });
        // sequence.AppendInterval(0.5f);
        // sequence.AppendCallback(() =>
        // {
        //     GetComponent<SpriteRenderer>().material.SetFloat("_StrongTintFade", 0.0f);
        // });
    }
}
