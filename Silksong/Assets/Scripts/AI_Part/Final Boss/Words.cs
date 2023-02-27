using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Words : MonoBehaviour
{
    public List<Sprite> sprites;
    //public GameObject word;
    public GameObject damager;
    public GameObject fallPos;
    public GameObject leftPos;
    public GameObject rightPos;
    
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(FallGenerate("¾øÍû"));
        StartCoroutine(LeftGenerate("¾øÍû"));
        StartCoroutine(RightGenerate("¾øÍû"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    GameObject GenerateWord(string name)
    {
        GameObject word = new GameObject(name);
        word.layer = 8;
        word.tag = "word";
        word.AddComponent<SpriteRenderer>();
        word.AddComponent<Word>();

        SpriteRenderer spriteRenderer = word.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[0];
        word.transform.parent = this.transform;
        return word;
    }

    void SetDamager(GameObject word)
    {
        word.AddComponent<PolygonCollider2D>();
        word.AddComponent<Rigidbody2D>();
        GameObject d = Instantiate(damager, word.transform);
        UnityEditorInternal.ComponentUtility.CopyComponent(word.GetComponent<PolygonCollider2D>());
        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(d);
        d.GetComponent<PolygonCollider2D>().isTrigger = true;
    }



    IEnumerator FallGenerate(string name)
    {
        GameObject word = GenerateWord(name);
        word.transform.localPosition = fallPos.transform.localPosition;
        yield return new WaitForSeconds(2f);
        SetDamager(word);
    }
    
    IEnumerator LeftGenerate(string name)
    {
        GameObject word = GenerateWord(name);
        word.transform.localPosition = leftPos.transform.localPosition;
        yield return new WaitForSeconds(1f);
        SetDamager(word);

        word.GetComponent<Rigidbody2D>().AddForce(new Vector3(1, 0, 0) * 15, ForceMode2D.Impulse);
    }
    
    IEnumerator RightGenerate(string name)
    {
        GameObject word = GenerateWord(name);
        word.transform.localPosition = rightPos.transform.localPosition;
        yield return new WaitForSeconds(1f);
        SetDamager(word);

        word.GetComponent<Rigidbody2D>().AddForce(new Vector3(-1, 0, 0) * 15, ForceMode2D.Impulse);
    }
    
}
   