using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator animator;
    public bool destroy;
    void Start()
    {
        animator = this.GetComponent<Animator>();
        Invoke("Attack", 3f);
    }

    // Update is called once per frame
    void Update()
    {
        if (destroy)
        {
            Destroy(gameObject);
        }
    }
    
    void Attack()
    {
        animator.Play("attack");
    }
}
