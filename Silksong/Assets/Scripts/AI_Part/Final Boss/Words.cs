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
    [SerializeField] private int numOfChase;
    [SerializeField] private ArrayList chaseList;
    private int currentWord;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(FallGenerate("绝望"));
        //StartCoroutine(LeftGenerate("绝望"));
        //StartCoroutine(RightGenerate("绝望"));
        //string[] names = { "绝望", "绝望" };
        //StartCoroutine(EmergeGenerate(names));
        //GenerateChase("juew");
        chaseList = new ArrayList();
    }

    // Update is called once per frame
    void Update()
    {
    }

    GameObject GenerateWord()
    {
        int idx = Random.Range(0, sprites.Count);
        string name = sprites[idx].name;
        GameObject word = new GameObject(name);
        word.layer = 8;
        word.tag = "word";
        word.AddComponent<SpriteRenderer>();
        word.AddComponent<Word>();
        word.AddComponent<BulletCollision>();
        word.GetComponent<BulletCollision>().timeBeforeAutodestruct = 7;

        SpriteRenderer spriteRenderer = word.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[idx];
        spriteRenderer.sortingOrder = 1;
        word.transform.parent = this.transform;
        return word;
    }

    void SetDamager(GameObject word)
    {
        word.AddComponent<PolygonCollider2D>();
        word.AddComponent<Rigidbody2D>();
        GameObject d = Instantiate(damager, word.transform);
        Destroy(d.GetComponent<BoxCollider2D>());
        UnityEditorInternal.ComponentUtility.CopyComponent(word.GetComponent<PolygonCollider2D>());
        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(d);
        d.GetComponent<PolygonCollider2D>().isTrigger = true;
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
        for (int i = 0; i < horizonChart[currentHorizon].Length; ++i)
        {
            StartCoroutine(LeftGenerate(horizonChart[currentHorizon][i]-'0'));
            StartCoroutine(RightGenerate(horizonChart[currentHorizon][i]-'0'));
        }
    }

    IEnumerator ChaseAttack()
    {
        for (int i = 0; i < numOfChase; ++i)
        {
            ChaseGenerate();
            yield return new WaitForSeconds(2f);
        }
    }

    IEnumerator EmergeAttack()
    {
        for (int i = 0; i < emergeChart[currentEmerge].Length; ++i)
        {
            StartCoroutine(EmergeGenerate(emergeChart[currentEmerge][i]-'0'));
            yield return new WaitForSeconds(1f);
        }
        foreach(GameObject w in chaseList)
        {
            w.GetComponent<BulletCollision>().timeBeforeAutodestruct = 3;
        }
        chaseList.Clear();
    }
    IEnumerator FallGenerate(int idx)
    {
        GameObject word = GenerateWord();
        word.transform.position = fallPos.transform.GetChild(idx).transform.position;
        yield return new WaitForSeconds(2f);
        SetDamager(word);
    }
    
    IEnumerator LeftGenerate(int idx)
    {
        GameObject word = GenerateWord();
        word.transform.position = leftPos.transform.GetChild(idx).transform.position;
        Vector3 w = word.transform.position;
        Vector3 l = leftPos.transform.GetChild(idx).transform.position;
        Vector3 a = new Vector3();
        yield return new WaitForSeconds(1f);
        SetDamager(word);

        word.GetComponent<Rigidbody2D>().AddForce(new Vector3(1, 0, 0) * 15, ForceMode2D.Impulse);
    }
    
    IEnumerator RightGenerate(int idx)
    {
        GameObject word = GenerateWord();
        word.transform.position = rightPos.transform.GetChild(idx).transform.position;
        yield return new WaitForSeconds(1f);
        SetDamager(word);

        word.GetComponent<Rigidbody2D>().AddForce(new Vector3(-1, 0, 0) * 15, ForceMode2D.Impulse);
    }

    IEnumerator EmergeGenerate(int idx)
    {
        GameObject word = GenerateWord();
        word.GetComponent<BulletCollision>().timeBeforeAutodestruct = -1;
        chaseList.Add(word);
        GameObject pos = emergePos.transform.GetChild(idx).gameObject;
        word.transform.localPosition = pos.transform.localPosition;
        Color c = word.GetComponent<SpriteRenderer>().color;
        c.a = 0.2f;
        word.GetComponent<SpriteRenderer>().color = c;
        yield return new WaitForSeconds(1.5f);
        c.a = 1f;
        word.GetComponent<SpriteRenderer>().color = c;
        SetDamagerStatic(word);
    }

    void ChaseGenerate()
    {
        GameObject word = GenerateWord();
        word.transform.localPosition = boss.transform.localPosition;
        word.transform.localScale /= 2;
        word.AddComponent<HpDamable>();
        word.GetComponent<HpDamable>().MaxHp = 10;
        word.GetComponent<HpDamable>().playerAttackCanGainSoul = true;
        SetDamager(word);
        word.GetComponent<Word>().Chase();
    }

    public void Attack()
    {
        if (currentWord == 0)
        {
            StartCoroutine(FallAttack());
        }
        else if (currentWord == 1)
        {
            HorizonAttack();
        }
        else if (currentWord == 2)
        {
            StartCoroutine(ChaseAttack());
        }
        else
        {
            StartCoroutine(EmergeAttack());
        }
        currentWord = (currentWord + 1) % 4;
    }
}
   