using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBirdController : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D rbd;
    public GameObject Player;
    private Vector3 bossloc;
    private Vector3 playerloc;
    private int cur_loc;
    private int Health = 100;
    void Start()
    {
        rbd = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        bossloc = rbd.position;
        playerloc = Player.transform.position;
        CaculateLoc(bossloc,playerloc);
    }

    void CaculateLoc(Vector3 Boss, Vector3 Player)
    {
        if (Player.x > Boss.x)
        {
            transform.localEulerAngles = Vector3.zero;
        }
        //Debug.Log("player at right");
        else
        {
            transform.localEulerAngles = new Vector3(0, 180, 0);
            // Debug.Log("player at left");
        }
    }

    public void DoDamege(int Damage)
    {
        Health -= Damage;
        //TODO::判断HP进入BOSS不同阶段
    }
    
    
}
