using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
<<<<<<< HEAD
/// 发射酸水(火球)
=======
/// 发射酸水
>>>>>>> d279aa9a (update mapObjects)
/// </summary>作者：青瓜
public class AcidShooter : MonoBehaviour
{
    public GameObject flyAcid;
    public float shotHeight;
    public float shotCD;

    private float Time;//竖直上抛总时间的一半
    private float Velocity;//上抛初速度

    void Start()
    {
        float g = -Physics2D.gravity.y;//g=9.81   shotHeight=g*time^2/2   time*g=velocity
        Time = Mathf.Sqrt(2*shotHeight/g);
        Velocity = Time * g;

        if (shotCD > 0)
<<<<<<< HEAD
        {
            StartCoroutine(shotAcidLoop());
        }
           
=======
            StartCoroutine(shotAcidLoop());
>>>>>>> d279aa9a (update mapObjects)
        else Debug.Log("shot cd can not <=0");
    }

    private IEnumerator shotAcidLoop()
    {
        while(true)
        {
            yield return new WaitForSeconds(shotCD);//以shotcd为间隔不断发射
            shotAcid();
        }
    }

<<<<<<< HEAD
    private IEnumerator rotateAcid(Transform acid)
    {
        yield return new WaitForSeconds(Time);
        acid.rotation = Quaternion.Euler( new Vector3(0, 0, 0));
    }
=======
>>>>>>> d279aa9a (update mapObjects)
    private void shotAcid()
    {
        GameObject acid = Instantiate(flyAcid);
        acid.transform.position = transform.position;
        acid.GetComponent<Rigidbody2D>().velocity = new Vector2(0, Velocity);
        Destroy(acid, Time * 2);
<<<<<<< HEAD
        StartCoroutine(rotateAcid(acid.transform));
=======
>>>>>>> d279aa9a (update mapObjects)
    }
}
