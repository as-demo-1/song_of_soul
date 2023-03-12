using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Word : MonoBehaviour
{
    [SerializeField]
    private string type;
    private GameObject player;
    private bool isChasing = false;
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
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "word")
            Destroy(this.gameObject);
    }

    public void Chase()
    {
        this.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        isChasing = true;
    }
}
