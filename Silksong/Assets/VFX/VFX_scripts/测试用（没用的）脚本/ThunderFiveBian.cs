using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderFiveBian : MonoBehaviour
{
    //测试用的脚本，把闪电链的两个端点位置用两个gameobject的位置来控制，然后可以在inspector控制闪电链等级，没了
    ThunderChainController m_ThunderChainController;
    public GameObject enemyA;
    public GameObject enemyB;
    public int thunderLevel;
    // Start is called before the first frame update
    void Start()
    {
        m_ThunderChainController = this.transform.GetComponent<ThunderChainController>();
        thunderLevel = 1;
    }

    // Update is called once per frame
    void Update()
    {
        m_ThunderChainController.ThunderChainLevel = thunderLevel;
        m_ThunderChainController.startPos = enemyA.transform.position;
        m_ThunderChainController.endPos = enemyB.transform.position;
    }
}
