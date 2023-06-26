using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Words : MonoBehaviour
{
    [SerializeField] private GameObject boss;
    public GameObject bulletSender;
    [SerializeField] private List<Sprite> sprites;
    public GameObject wordPrefab;
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
    [SerializeField] private List<GameObject> chaseList;
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
        chaseList = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void SetDamager(GameObject word)
    {
        word = word.transform.GetChild(0).gameObject;
        word.AddComponent<BoxCollider2D>();
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
        Destroy(d.GetComponent<BoxCollider2D>());
        UnityEditorInternal.ComponentUtility.CopyComponent(word.GetComponent<PolygonCollider2D>());
        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(d);
        d.GetComponent<PolygonCollider2D>().isTrigger = true;
    }

    IEnumerator FallAttack()
    {
        for (int i = 0; i < fallChart[currentFall].Length; ++i)
        {
            GameObject word = Instantiate(wordPrefab);
            word.transform.position = fallPos.transform.GetChild(fallChart[currentFall][i] - '0').transform.position;
            yield return new WaitForSeconds(0.5f);
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
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator EmergeAttack()
    {
        for (int i = 0; i < emergeChart[currentEmerge].Length; ++i)
        {
            StartCoroutine(EmergeGenerate(emergeChart[currentEmerge][i]-'0'));
            yield return new WaitForSeconds(1f);
        }
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        foreach (GameObject w in chaseList)
        {
            //float angle = Vector3.Angle(player.transform.position, this.transform.position);
            //w.transform.GetChild(0).GetComponent<BulletSender>().ChangeRotation(angle);
            //w.GetComponent<Word>().setLifeTime(3.0f);
        }
        chaseList.Clear();
    }
    
    IEnumerator LeftGenerate(int idx)
    {
        GameObject word = Instantiate(wordPrefab);
        word.transform.GetChild(0).gameObject.layer = 0;
        word.transform.position = leftPos.transform.GetChild(idx).transform.position;
        yield return new WaitForSeconds(1f);
        SetDamager(word);

        word.transform.GetChild(0).GetComponent<Rigidbody2D>().AddForce(new Vector3(1, 0, 0) * 13, ForceMode2D.Impulse);
    }
    
    IEnumerator RightGenerate(int idx)
    {
        GameObject word = Instantiate(wordPrefab);
        word.transform.GetChild(0).gameObject.layer = 0;
        word.transform.position = rightPos.transform.GetChild(idx).transform.position;
        yield return new WaitForSeconds(1f);
        SetDamager(word);

        word.transform.GetChild(0).GetComponent<Rigidbody2D>().AddForce(new Vector3(-1, 0, 0) * 13, ForceMode2D.Impulse);
    }

    IEnumerator EmergeGenerate(int idx)
    {
        GameObject word = Instantiate(wordPrefab);
        //word.GetComponent<BulletCollision>().timeBeforeAutodestruct = -1;
        word.transform.localScale /= 2;
        //Instantiate(bulletSender, word.transform);
        chaseList.Add(word);
        GameObject pos = emergePos.transform.GetChild(idx).gameObject;
        word.transform.position = pos.transform.position;
        word = word.transform.GetChild(0).gameObject;
        Destroy(word.GetComponent<Break>());
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
        GameObject word = Instantiate(wordPrefab);
        word.transform.position = boss.transform.position;
        //word.transform.localScale /= 3;
        //word.AddComponent<HpDamable>();
        //word.GetComponent<HpDamable>().MaxHp = 10;
        //word.GetComponent<HpDamable>().playerAttackCanGainSoul = true;
        word.GetComponent<ParticleWord>().Chase();
    }

    public void Attack()
    {
        //StartCoroutine(FallAttack());
        StartCoroutine(ChaseAttack());
        /*
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
        */
    }
}
   