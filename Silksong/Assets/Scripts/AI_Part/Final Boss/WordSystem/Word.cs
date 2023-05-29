using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Word : MonoBehaviour
{
    [SerializeField]
    private string type;
    private GameObject player;
    [SerializeField]
    private float lifeCycle = 5.0f;
    [SerializeField]
    private bool isChasing = false;
    [SerializeField]
    private float timer = 0f;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (isChasing)
        {
            Vector2 diff = (player.transform.position - this.transform.position).normalized;
            this.GetComponent<Rigidbody2D>().velocity = (this.GetComponent<Rigidbody2D>().velocity + diff / 2).normalized * 2;
        }

        if (timer >= 0)
            {
                timer += Time.deltaTime;
                if (timer > lifeCycle)
                {
                    Destroy(gameObject);
                }
            }
    }

    public void setLifeTime(float time)
    {
        this.timer += Time.deltaTime;
        this.lifeCycle = time;
    }

    public void Chase()
    {
        this.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        Destroy(this.transform.GetChild(0).GetComponent<Rigidbody2D>());
        isChasing = true;
    }
}
