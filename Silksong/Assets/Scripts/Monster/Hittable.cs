using System;
using System.Collections;
using System.Collections.Generic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(MonsterFSM))]
public class Hittable : MonoBehaviour
{
    public int maxHP;
    public int currentHP;
    // face left is -1, face right is 1
    public int face = -1;
    public bool lockHP;
    private MonsterFSM _monsterFsm;
    
    public Hittable(int maxHp, int currentHp = Int32.MaxValue)
    {
        maxHP = maxHp;
        currentHP = Mathf.Min(maxHp, currentHp);
    }

    void BeHitAction(in Hitter hitter)
    {
        _monsterFsm.TransitionState(EMonsterState.Hurt);
        switch (hitter.m_actionName)
        {
            case "Player Normal Atk":
                TickNormalAtcEffect((PlayerNormalAtk)hitter);
                break;
            case "LightningChain Atk":
                break;
            default:
                break;
        }
    }

    void TickLightningChainEffect(LightningChain lightningChain)
    {
        //lightningChain
    }
    
    void TickNormalAtcEffect(PlayerNormalAtk playerNormalAtk)
    {
        playerNormalAtk.AtkPerTarget(this);
    }

    public void GetDamage(int atk)
    {
        if (_monsterFsm.monsterState.stateEnum != EMonsterState.Die)
        {
            currentHP -= atk;
        }

        if (currentHP <= 0)
        {
            
        }
    }

    public void GetRepel(float backDistance)
    {
        Vector3 position = GetComponentInParent<Transform>().position;
        position = new Vector3(position.x + face * backDistance, position.y, position.z);
    }
}
