using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalDanmuController : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject Prefabs;
    private int l = 1;
    void Start()
    {
       
    }

    public void Fire()
    {
         var boss = GameObject.FindGameObjectWithTag("Boss");
         var player = GameObject.FindGameObjectWithTag("Player");

         if (boss.transform.position.x > player.transform.position.x)
         {
             for (int i = 0; i < 6; i++)
             {
                 l = -1;
                 StartCoroutine(FireF(i));
               
             }
         }
         else
         {
             for (int i = 0; i < 6; i++)
             {
                 l = 1;
                 StartCoroutine(FireF(i));
             }
            
         }
        
    }
    // Update is called once per frame
    void Update()
    {
        
        
        
    }

    IEnumerator FireF(int i)
    {
        yield return  new WaitForSeconds(0.2f);
        var temp = GameObject.Instantiate(Prefabs);
        var RBD = temp.GetComponent<Rigidbody2D>();
        RBD.AddForce(new Vector2(l * 200 * i, 0));
    }
}
