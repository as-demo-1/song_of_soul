using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_GrapplingGun : MonoBehaviour
{
    [Header("Scripts Ref:")]
    public Tutorial_GrappleRope grappleRope;//这个是对应的绳子
    public Transform firePoint;
    public float takeBackTime;
    public Vector2 baseDir;
    public LayerMask ground;
    [HideInInspector] public Vector2 grapplePoint;//抓到的点
    [HideInInspector] public Vector2 grappleDistanceVector;//射击的方向

    public void Grapple()
    {
        //firePoint.GetComponent<Rigidbody>().velocity = grappleDistanceVector*10;
        Debug.Log("Grapple");
    }
    private void Start()
    {
        RaycastHit2D hit = Physics2D.Raycast(firePoint.transform.position, baseDir, 100, ground);
        Shoot(hit.point);
        grappleRope.m_lineRenderer.SetPosition(1, hit.point);
    }
    public void Shoot(Vector2 grapplePoint)
    {
        this.grapplePoint = grapplePoint;
        grappleDistanceVector=(grapplePoint-(Vector2)firePoint.transform.position).normalized;
        grappleRope.gameObject.SetActive(true);
    }
    private void Update()
    {
        if(!grappleRope.enabled)
            grappleRope.m_lineRenderer.SetPosition(0, firePoint.transform.position);
    }
    public void Catch()
    {
        grappleRope.gameObject.SetActive(false);
    }
    public void TakeBack_Rope()
    {
        StartCoroutine(TakeBackRope());
    }
    
    IEnumerator TakeBackRope()
    {
        while (Vector2.Distance(grapplePoint, firePoint.transform.position)>0.1f)
        {
            grapplePoint = Vector2.MoveTowards(grapplePoint, firePoint.transform.position,takeBackTime*Time.deltaTime);
            yield return null;
        }
        Catch();
    }
}
