using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reflector : DamageableBase
{
    bool ifRotate = false;
    GameObject newLight;
    GameObject moonLight;
    RaycastHit2D[] hit;
    public LayerMask Ground;
    [Tooltip("光线的射出速度（如果选择方案一可以删掉这个）")]
    public float lightSpeed = 1;
    [Tooltip("被攻击时旋转速度")]
    public float rotateSpeed = 1;
    [Tooltip("每次被攻击时旋转的度数")]
    public float angle=30f;
    [Tooltip("逆时针旋转的最大值，选取0到90")]
    public float maxAngle;
    [Tooltip("顺时针旋转的最大值,选取0到-90")]
    public float minAngle;
    #region 反射光线部分
    public void BeIrradiated(GameObject moonLight,Vector3 pos)
    {
        if (moonLight == null) return;
        float z0 = transform.rotation.eulerAngles.z > 180 ? transform.rotation.eulerAngles.z-180 : transform.rotation.eulerAngles.z;
        float ZRotate = 2*(90- (moonLight.transform.rotation.eulerAngles.z -z0));
        newLight.transform.position = pos;
        newLight.transform.rotation = moonLight.transform.rotation;
        newLight.transform.localScale = new Vector3(moonLight.transform.localScale.x, -moonLight.transform.localScale.y);
        if (Mathf.Abs(moonLight.transform.rotation.eulerAngles.z - z0) > 10)
        {
            newLight.transform.RotateAround(pos, new Vector3(0, 0, 1), ZRotate);            
            Vector3 target = ReflectRay(newLight);
            LightMove2(newLight, target);
        }
        else
        {
            newLight.transform.position = moonLight.transform.position;
        }
    }
    //这时随着每帧旋转及时的改变光线的位置和长度
    public void LightMove2(GameObject newLight, Vector3 target)
    {
        float length = (target - newLight.transform.position).magnitude;
        newLight.transform.position = target;
        newLight.transform.localScale = new Vector3(newLight.transform.localScale.x, length * 1.1f);
        //newLight.GetComponent<MoonLight>().Radial(newLight.transform.GetChild(1).transform.position, newLight.transform.GetChild(0).transform.position);
    }
    #region 光线不随着镜子旋转
    //public void BeIrradiated2(GameObject moonLight, Vector3 pos)
    //{
    //    float z0 = transform.rotation.eulerAngles.z > 180 ? transform.rotation.eulerAngles.z - 180 : transform.rotation.eulerAngles.z;
    //    float ZRotate = 2 * (90 - (moonLight.transform.rotation.eulerAngles.z - z0));
    //    newLight.transform.position = pos;
    //    newLight.transform.rotation = moonLight.transform.rotation;
    //    newLight.transform.localScale = new Vector3(moonLight.transform.localScale.x, -moonLight.transform.localScale.y/moonLight.transform.localScale.y);
    //    newLight.transform.RotateAround(pos, new Vector3(0, 0, 1), ZRotate);
    //    Vector3 target = ReflectRay(newLight);
    //    StartCoroutine(LightMove(newLight, target));
    //}
    ////用一个协程表示光线射出的过程
    //IEnumerator LightMove(GameObject newLight, Vector3 target)
    //{
    //    while ((newLight.transform.position - target).magnitude > 0.001)
    //    {
    //        newLight.transform.localScale = new Vector3(newLight.transform.localScale.x, newLight.transform.localScale.y + Time.deltaTime * lightSpeed * 1.15f);
    //        newLight.transform.position = Vector2.MoveTowards(newLight.transform.position, target, Time.deltaTime * lightSpeed);
    //        yield return null;
    //    }
    //}
    #endregion
    //光线照射到反射镜的点(反射点）
    public Vector3 LightHitPos(GameObject light)
    {
        if (light == null) return Vector3.zero;
        Vector3 direct = light.transform.GetChild(0).transform.position - light.transform.position;
        hit = Physics2D.RaycastAll(light.transform.position, direct,100);
        foreach(var a in hit)
        {
            if (a.collider.gameObject==gameObject)
            {
                return a.point;
            }
        }
        return Vector3.zero;
    }
    //反射光线照射到地板的点
    public Vector3 ReflectRay(GameObject newLight)
    {
        Vector3 direct = newLight.transform.GetChild(0).transform.position - newLight.transform.position;
        RaycastHit2D hit = Physics2D.Raycast(newLight.transform.position, direct, 100,Ground);
        if (hit.collider != null)
        {
            return newLight.transform.position+ ((Vector3)hit.point-newLight.transform.position)/2;
        }
        return newLight.transform.position + (newLight.transform.GetChild(0).transform.position - newLight.transform.position).normalized * 20;
    }
    #endregion
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<MoonLight>() && collision.gameObject != newLight&&moonLight!=collision.gameObject)
        {
            moonLight = collision.gameObject;
            newLight = Instantiate(moonLight);
            //newLight.GetComponent<MoonLight>().line=collision.GetComponent<LineRenderer>();
            Vector3 pos = LightHitPos(moonLight);
            BeIrradiated(moonLight, pos);
        }
    }
    //被攻击时旋转
    IEnumerator SelfRotate()
    {
        int dir = angle > 0 ? 1 : -1;
        ifRotate = true; Vector3 pos;
        float currentZ = transform.rotation.eulerAngles.z;
        while (Mathf.Abs(currentZ-transform.rotation.eulerAngles.z)  <= Mathf.Abs(angle))
        {
            transform.Rotate(new Vector3(0, 0, Time.deltaTime * rotateSpeed*dir));
            pos = LightHitPos(moonLight);
            BeIrradiated(moonLight, pos);
            yield return null;
        }
        pos = LightHitPos(moonLight);
        BeIrradiated(moonLight, pos);
        ifRotate = false;
    }
    public override void takeDamage(DamagerBase damager)
    {
        float rotation = transform.rotation.eulerAngles.z >180 ? transform.rotation.eulerAngles.z -360 : transform.rotation.eulerAngles.z;
        if (rotation >= maxAngle||rotation<=minAngle) angle = -angle;
        if (!ifRotate)
        {
            StartCoroutine(SelfRotate());
        }
    }
}
