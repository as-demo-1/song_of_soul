using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordPart : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 impulse;
    private Rigidbody2D rb;
    private float slow = 2000.0f;
    private GameObject damager;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 velocity = rb.velocity;
        Vector3 direction = velocity.normalized;
        if (Mathf.Abs(velocity.y) < 0.1f)
        {
            rb.AddForce(direction * (-1) * slow * Time.fixedDeltaTime, ForceMode2D.Force);
        }
    }

    private void Awake()
    {
        SetDamager();
        Impulse();
    }

    void SetDamager()
    {
        damager = GameObject.Find("Boss").transform.GetChild(0).gameObject;
        GameObject d = Instantiate(damager, this.gameObject.transform);
        Destroy(d.GetComponent<BoxCollider2D>());
        //UnityEditorInternal.ComponentUtility.CopyComponent(this.gameObject.GetComponent<PolygonCollider2D>());
        //UnityEditorInternal.ComponentUtility.PasteComponentAsNew(d);
        d.GetComponent<PolygonCollider2D>().isTrigger = true;
    }


    void Impulse()
    {
        rb = this.GetComponent<Rigidbody2D>();
        rb.AddForce(impulse, ForceMode2D.Impulse); ;
    }
}
