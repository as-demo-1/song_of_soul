using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterInformation : MonoBehaviour
{
    public bool isFaceToPlayer = true;

    public Vector3 GetTargetPos()
    {
        return GameObject.Find("Player").transform.position;
    }
}
