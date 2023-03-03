using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Words : MonoBehaviour
{
    public GameObject boss;
    public List<Sprite> sprites;
    //public GameObject word;
    public GameObject damager;
    public GameObject fallPos;
    public GameObject leftPos;
    public GameObject rightPos;
    public GameObject emergePos;
    
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(FallGenerate("绝望"));
        //StartCoroutine(LeftGenerate("绝望"));
        //StartCoroutine(RightGenerate("绝望"));
        //string[] names = { "绝望", "绝望" };
        //StartCoroutine(EmergeGenerate(names));
        GenerateChase("juew");
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

    void SetChase(GameObject word)
    {

    }

    void SetDamagerStatic(GameObject word)
    {
        word.AddComponent<PolygonCollider2D>();
        word.AddComponent<Rigidbody2D>();
        word.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
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

    IEnumerator EmergeGenerate(string[] names)
    {
        for (int i = 0; i < names.Length; ++i)
        {
            GameObject word = GenerateWord(names[i]);
            GameObject pos = emergePos.transform.GetChild(i).gameObject;
            word.transform.localPosition = pos.transform.localPosition;
            Color c = word.GetComponent<SpriteRenderer>().color;
            c.a = 0.5f;
            word.GetComponent<SpriteRenderer>().color = c;
            yield return new WaitForSeconds(1f);
            c.a = 1f;
            word.GetComponent<SpriteRenderer>().color = c;
            SetDamagerStatic(word);
        }
    }

    void GenerateChase(string name)
    {
        GameObject word = GenerateWord(name);
        word.transform.localPosition = boss.transform.localPosition;
        SetDamager(word);
        word.AddComponent<ChasePlayer>();
    }
}
   