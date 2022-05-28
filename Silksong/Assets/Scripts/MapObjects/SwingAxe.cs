using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingAxe : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve curve;
    [SerializeField]
    private float SwingDeg = 120;//需要斧头摆动的角度，如需要两边各60°则输入120 以此类推
    [SerializeField]
    private float SwingSpeed = 1;//摆动速度
    [SerializeField]
    private int AxeDamage = 1;

    private Quaternion quaternion = new Quaternion();//存放摆动起始角度
    private Quaternion endpointDeg = new Quaternion();//存放摆动结束角度
    private bool isOver = false;


    private void Awake()
    {

        if (GetComponent<TwoTargetDamager>() == null)
        {
            this.gameObject.AddComponent<TwoTargetDamager>();

        }
        this.gameObject.GetComponent<TwoTargetDamager>().damage = AxeDamage;
    }

    // Start is called before the first frame update
    void Start()
    {
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
            this.gameObject.transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, curve.Evaluate(SwingSpeed * Time.deltaTime));
            if (Mathf.Abs(quaternion.eulerAngles.z - transform.eulerAngles.z) < 0.2)
            {
                isOver = true;
                transform.rotation = quaternion;
            }
        }
        else
        {
            this.gameObject.transform.rotation = Quaternion.Lerp(transform.rotation, endpointDeg, curve.Evaluate(SwingSpeed * Time.deltaTime));
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
}
