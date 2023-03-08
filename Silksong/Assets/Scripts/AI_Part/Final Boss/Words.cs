using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Words : MonoBehaviour
{
    [SerializeField] private GameObject boss;
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private GameObject damager;
    [SerializeField] private GameObject fallPos;
    [SerializeField] private string[] fallChart;
    [SerializeField] private int currentFall;
    [SerializeField] private GameObject leftPos;
    [SerializeField] private GameObject rightPos;
    [SerializeField] private string[] horizonChart;
    [SerializeField] private int currentHorizon;
    [SerializeField] private GameObject emergePos;
    [SerializeField] private string[] emergeChart;
    [SerializeField] private int currentEmerge;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(FallGenerate("绝望"));
        //StartCoroutine(LeftGenerate("绝望"));
        //StartCoroutine(RightGenerate("绝望"));
        //string[] names = { "绝望", "绝望" };
        //StartCoroutine(EmergeGenerate(names));
        //GenerateChase("juew");
    }

    // Update is called once per frame
    void Update()
    {
    }

    GameObject GenerateWord(int idx)
    {
        string name = sprites[idx].name;
        GameObject word = new GameObject(name);
        word.layer = 8;
        word.tag = "word";
        word.AddComponent<SpriteRenderer>();
        word.AddComponent<Word>();

        SpriteRenderer spriteRenderer = word.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[idx];
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

    IEnumerator FallAttack()
    {
        for (int i = 0; i < fallChart[currentFall].Length; ++i)
        {
            StartCoroutine(FallGenerate(fallChart[currentFall][i]-'0'));
            yield return new WaitForSeconds(1f);
        }
    }

    public void HorizonAttack()
    {
        /*
        for (int i = 0; i < horizonChart[currentHorizon].Length; ++i)
        {
            //StartCoroutine(HorizonGenerate(Random.Range(0, sprites.Count)));
        }
        */
    }

    public void ChaseAttack()
    {
        for (int i = 0; i < 2; ++i)
        {
            //StartCoroutine(FallGenerate(Random.Range(0, sprites.Count)));
        }
    }

    public void EmergeAttack()
    {
        for (int i = 0; i < emergeChart[currentEmerge].Length; ++i)
        {
            //StartCoroutine(EmergeGenerate(Random.Range(0, sprites.Count)));
        }
    }
    IEnumerator FallGenerate(int idx)
    {
        GameObject word = GenerateWord(Random.Range(0, sprites.Count));
        word.transform.position = fallPos.transform.GetChild(idx).transform.position;
        yield return new WaitForSeconds(2f);
        SetDamager(word);
    }
    
    IEnumerator LeftGenerate(int idx)
    {
        GameObject word = GenerateWord(idx);
        word.transform.localPosition = leftPos.transform.localPosition;
        yield return new WaitForSeconds(1f);
        SetDamager(word);

        word.GetComponent<Rigidbody2D>().AddForce(new Vector3(1, 0, 0) * 15, ForceMode2D.Impulse);
    }
    
    IEnumerator RightGenerate(int idx)
    {
        GameObject word = GenerateWord(idx);
        word.transform.localPosition = rightPos.transform.localPosition;
        yield return new WaitForSeconds(1f);
        SetDamager(word);

        word.GetComponent<Rigidbody2D>().AddForce(new Vector3(-1, 0, 0) * 15, ForceMode2D.Impulse);
    }

    IEnumerator EmergeGenerate(int[] idxs)
    {
        for (int i = 0; i < idxs.Length; ++i)
        {
            GameObject word = GenerateWord(idxs[i]);
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

    void GenerateChase(int idx)
    {
        GameObject word = GenerateWord(idx);
        word.transform.localPosition = boss.transform.localPosition;
        SetDamager(word);
        word.AddComponent<ChasePlayer>();
    }

    public void Attack()
    {
        //int attack = Random.Range(0, 3);
        int attack = 0;
        if (attack == 0)
        {
            StartCoroutine(FallAttack());
        }
        else if (attack == 1)
        {
            HorizonAttack();
        }
        else if (attack == 2)
        {
            ChaseAttack();
        }
        else
        {
            EmergeAttack();
        }
    }
}
   