using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingingEffect : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    public float disapearTime = 3;
    float disapearSpeed;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        disapearSpeed = spriteRenderer.color.a / disapearTime;
        StartCoroutine(Disapear());
    }
    IEnumerator Disapear()
    {
        float r = spriteRenderer.color.r;
        float g = spriteRenderer.color.g;
        float b = spriteRenderer.color.b;
        while (spriteRenderer.color.a > 0)
        {
            spriteRenderer.color=new Color(r,g,b, spriteRenderer.color.a-disapearSpeed*Time.deltaTime);
            yield return null;
        }
        //gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
