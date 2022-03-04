using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonLightCast : MonoBehaviour
{
    public LineRenderer line;
    public float g;
    public LayerMask Ground;
    GameObject reflectorLight;
    GameObject reflector;
    RaycastHit2D[] hit2Ds;
    PlayerController playerController;
    Vector3 direct;
    // Start is called before the first frame update
    void Start()
    {
        hit2Ds = new RaycastHit2D[10];
        line=GetComponent<LineRenderer>();
        Physics2D.queriesStartInColliders = false;
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    void FixedUpdate()
    {
        Radial(transform.position,transform.GetChild(0).position);
        MoonLightEffect();
    }
    public Vector3 Radial(Vector2 shootPos, Vector2 target)
    {
        line.SetPosition(0,transform.position);
        Vector3 direct = target - shootPos;
        RaycastHit2D hit = Physics2D.Raycast(shootPos, direct, 100, Ground);
        if (hit.collider != null)
        {
            line.SetPosition(1, hit.point);
            return hit.point;
        }
        else
        {
            Vector2 pos1 = (Vector3)shootPos + direct * 20;
            line.SetPosition(1, pos1);
            return pos1;
        }
    }
    public void RotateLight(GameObject light,GameObject reflector,Vector2 pos)
    {
        //可以把reflector.transform.right当作法线
        light.transform.rotation = light.transform.rotation;
        float z0 = reflector.transform.rotation.eulerAngles.z > 180 ? reflector.transform.rotation.eulerAngles.z - 180 : reflector.transform.rotation.eulerAngles.z;
        float ZRotate = 2 * (90 - (transform.rotation.eulerAngles.z - z0));
        light.transform.RotateAround(pos, new Vector3(0, 0, 1), ZRotate);
    }
    public void MoonLightEffect()
    {
        Physics2D.RaycastNonAlloc(transform.position, transform.GetChild(0).transform.position-transform.position, hit2Ds, 100);
        foreach (var hit in hit2Ds)
        {
            if (!hit) continue;
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                playerController = hit.collider.GetComponent<PlayerController>();
                if (playerController && playerController.gravityLock == false)
                {
                    playerController.RB.gravityScale = g;
                }
            }
            else if (hit.collider.GetComponent<Reflector>()&&hit.collider.gameObject!=reflector)
            {
                if (reflectorLight == null)
                {
                    reflectorLight = Instantiate(gameObject);
                    reflectorLight.GetComponent<MoonLightCast>().reflector = hit.collider.gameObject;
                    reflectorLight.transform.position = hit.point;
                    reflectorLight.transform.localScale = new Vector3(reflectorLight.transform.localScale.x, -reflectorLight.transform.localScale.y, reflectorLight.transform.localScale.z);                    
                    RotateLight(reflectorLight, hit.collider.gameObject, hit.point);
                }
                
            }
        }
        //if (playerController && playerController.gravityLock == false)
        //{
        //    playerController.RB.gravityScale = playerController.playerInfo.normalGravityScale;
        //    playerController = null;
        //}
    }
}
