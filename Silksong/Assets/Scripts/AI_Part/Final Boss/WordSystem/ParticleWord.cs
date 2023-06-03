using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleWord : MonoBehaviour
{
    public string word;
    public float destroyAfterGroundCollision;
    public float destroyAfterPlayerCollision;
    private GameObject child;
    private bool isChasing = false;
    private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        if (word == "random" || word == "")
        {
            child = this.transform.GetChild(Random.Range(0, this.transform.childCount - 1)).gameObject;
        } else
        {
            child = this.transform.Find(word).gameObject;
        }
        child.SetActive(true);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject sub_child = child.transform.GetChild(1).gameObject;
        sub_child.SetActive(true);
        isChasing = false;
        if (collision.gameObject.tag == "Player")
        {
            Destroy(gameObject, destroyAfterPlayerCollision);
        } else
        {
            Destroy(gameObject, destroyAfterGroundCollision);
        }
    }

    public void Chase()
    {
        //this.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        //Destroy(this.transform.GetChild(0).GetComponent<Rigidbody2D>());
        isChasing = true;
    }
}
