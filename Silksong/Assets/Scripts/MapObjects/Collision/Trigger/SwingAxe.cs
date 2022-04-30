using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingAxe : Trigger2DBase
{
    [SerializeField]
    private AnimationCurve curve;
    [SerializeField]
    private float SwingDeg = 120;//需要斧头摆动的角度，如需要两边各60°则输入120 以此类推
    [SerializeField]
    private float SwingSpeed = 7;//摆动速度
    [SerializeField]
    public float AxeDamage { get; } = 10;//斧头伤害
    private Quaternion quaternion = new Quaternion();//存放摆动起始角度
    private Quaternion endpointDeg = new Quaternion();//存放摆动结束角度
    private Transform axeTF;
    private bool isOver = false;



    // Start is called before the first frame update
    void Start()
    {
        axeTF = this.GetComponent<Transform>();
        SwingDeg = SwingDeg / 2;
        quaternion.eulerAngles = new Vector3(0, 0, -SwingDeg);
        endpointDeg.eulerAngles = new Vector3(0, 0, SwingDeg);
    }

    // Update is called once per frame
    void Update()
    {
        AxeSwing();
    }


    /// <summary>
    /// 斧头来回摆动方法
    /// </summary>
    private void AxeSwing()
    {
        if (!isOver)
        {
            axeTF.transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, curve.Evaluate(SwingSpeed * Time.deltaTime));
            if (Mathf.Abs(quaternion.eulerAngles.z - transform.eulerAngles.z) < 0.2)
            {
                isOver = true;
                transform.rotation = quaternion;
            }
        }
        else
        {
            axeTF.transform.rotation = Quaternion.Lerp(transform.rotation, endpointDeg, curve.Evaluate(SwingSpeed * Time.deltaTime));
            if (transform.rotation.z > 0)
            {
                if (Mathf.Abs(endpointDeg.eulerAngles.z - transform.eulerAngles.z) < 0.2)
                {
                    isOver = false;
                    transform.rotation = endpointDeg;
                }
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (canWork && targetLayer.Contains(collision.gameObject))
        {
            print(AxeDamage);
        }
    }
}
