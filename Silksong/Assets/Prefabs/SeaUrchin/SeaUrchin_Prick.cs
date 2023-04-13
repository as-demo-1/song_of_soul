using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaUrchin_Prick : Damable
{
    public Transform seaUrchin;
    public Vector2 dir;
    public Vector2 range;//¸¡¶¯·¶Î§
    public float maxMoveRange=1;
    public float maxMoveSpeed;
    public float RotateSpeed;
    public Rigidbody2D rb;
    float t;
    public void InitPrick(Transform urchin)
    {
        t = 0;
        seaUrchin = urchin;
        this.dir =(urchin.position-transform.position).normalized;
    }
    public float SinMove(float t)
    {
        float speed=0;
        speed=Mathf.Sin(t*maxMoveSpeed)*maxMoveRange;
        return speed;
    }
    private void FixedUpdate()
    {
        transform.up= (transform.position-seaUrchin.position   ).normalized;
        t += Time.fixedDeltaTime;
        rb.velocity = transform.up*SinMove(t);
        transform.RotateAround(seaUrchin.position, Vector3.forward, RotateSpeed * Time.fixedDeltaTime);
    }
}
