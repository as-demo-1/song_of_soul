using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Break : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject breaks;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("word") == false)
        {
            //this.transform.parent.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            this.gameObject.SetActive(false);
            breaks.SetActive(true);
            breaks.transform.position = this.transform.position;
            breaks.transform.rotation = this.transform.rotation;
        }
    }
}
