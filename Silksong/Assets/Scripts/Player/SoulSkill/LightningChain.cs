using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using DG.Tweening;
using UnityEngine;
using UnityEngine.PlayerLoop;


[Serializable]
public class LightningChain : SoulSkill
{
    public float range = 1.0f;
    //[Range(0,1)]
    //public float extraSpeedPercent = 0;


    public int extraDamage = 1;

    //HpDamable preTarget;

    [Range(0, 20)] public float moveSpeedUp;

    //public Material lightningChainMat;

    //private float width = 0.1f;
    //private Dictionary<int, GameObject> chains = new Dictionary<int, GameObject>();
    [SerializeField]
    private List<ThunderChainController> chainList = new List<ThunderChainController>();

    [SerializeField] private ThunderChainController chainPrefab;
    [SerializeField] private Transform chainsRoot;

    [SerializeField]
    /// <summary>
    /// 标记的敌人列表
    /// </summary>
    private List<HpDamable> enemyList = new List<HpDamable>();

    [SerializeField]
    private List<GameObject> effectList = new List<GameObject>(); // lightning particle effect

    [SerializeField] private GameObject sparkEffectPrefab;

    [SerializeField] private GameObject explodeEffect;

    [SerializeField] private LightningTrigger lightningTrigger;

    [SerializeField] [Header("三档闪电链需要的数量")]
    private int[] lightningLevelNum = { 2, 4, 6 };

    private int lightningLevel;

    public override void Init(PlayerController playerController, PlayerCharacter playerCharacter)
    {
        base.Init(playerController, playerCharacter);
        lightningTrigger.GetComponent<CircleCollider2D>().radius = range;
        lightningTrigger.makeLightningEvent.AddListener(AddEnemy);
        this.SkillStart.AddListener(() => lightningTrigger.gameObject.SetActive(true));
        this.SkillEnd.AddListener(ExplodeLightning);
    }

    public void AddEnemy(HpDamable damageableBase)
    {
        if (enemyList.Contains(damageableBase))
        {
            return;
        }

        enemyList.Add(damageableBase);
        GameObject spark = Instantiate(sparkEffectPrefab, damageableBase.transform);
        spark.transform.localPosition = Vector3.zero;
        effectList.Add(spark);
        if (enemyList.Count > 1)
        {
            GameObject chain = Instantiate(chainPrefab.gameObject, chainsRoot);
            chainList.Add(chain.GetComponent<ThunderChainController>());
        }

        lightningLevel = 3;
        for (int i = 0; i < lightningLevelNum.Length - 1; i++)
        {
            if (enemyList.Count >= lightningLevelNum[i] &&
                enemyList.Count < lightningLevelNum[i + 1])
            {
                lightningLevel = i + 1;
                break;
            }
        }


        foreach (var chain in chainList)
        {
            chain.ThunderChainLevel = lightningLevel;
        }
    }

    public void ExplodeLightning()
    {
        lightningTrigger.gameObject.SetActive(false);
        
        damager.damage = baseDamage + enemyList.Count * extraDamage; // 伤害计算公式

        // 销毁锁链
        for (int i = 0; i < chainList.Count; i++)
        {
            Destroy(chainList[i].gameObject);
        }

        chainList.Clear();

        // 销毁标记
        for (int i = 0; i < effectList.Count; i++)
        {
            Destroy(effectList[i]);
        }

        effectList.Clear();

        foreach (var enemy in enemyList)
        {
            if (enemy != null)
            {
                GameObject explode = Instantiate(explodeEffect, enemy.transform);
                DOVirtual.DelayedCall(0.2f, () => enemy.takeDamage(damager));
                Destroy(explode, 2.0f);
            }
        }

        enemyList.Clear();
    }

    public void RemoveAllList()
    {
        // 切换关卡时调用
        chainList.Clear();
        effectList.Clear();
        enemyList.Clear();
    }


    private void Update()
    {
        //TriggerAddElectricMarkEvent();
        //UpdateTargetsLink();
        UpdateChain();
    }

    /// <summary>
    /// 更新闪电链的位置
    /// </summary>
    public void UpdateChain()
    {
        if (chainList.Count.Equals(0))
        {
            return;
        }

        if (!enemyList.Count.Equals(chainList.Count + 1))
        {
            Debug.LogError("锁链数量错误");
            return;
        }


        for (int i = 0; i < chainList.Count; i++)
        {
            chainList[i].startPos = enemyList[i].transform.position;
            chainList[i].endPos = enemyList[i + 1].transform.position;
        }
    }

    public void SpeedUp(bool needApply)
    {
        if (needApply) _playerCharacter.buffManager.AddBuff(BuffProperty.MOVE_SPEED, moveSpeedUp);
        else
        {
            _playerCharacter.buffManager.DecreaseBuff(BuffProperty.MOVE_SPEED, moveSpeedUp);
        }
    }
/*
    // private LightningChain eventVariant;
    // public void TriggerAddElectricMarkEvent()
    // {
    //     if (eventVariant is null)
    //     {
    //         eventVariant = Clone();
    //         //eventVariant.m_eventType = BattleEventType.LightningAddElectricMarkEvent;
    //
    //         eventVariant.gameObject.SetActive(false);
    //     }
    //     EventCenter<BattleEventType>.Instance.TiggerEvent(BattleEventType.LightningAddElectricMarkEvent, eventVariant);
    // }
    
    public bool AtkPerTarget(HpDamable target)
    {
        if (!IsAtkSuccess(target)||!target.HaveBuff(BuffType.ElectricMark)) return false;
        //target.GetDamage(Damage());
        target.RemoveBuff(BuffType.ElectricMark);
        if (chainsRoot != null)
        {
            Destroy(chainsRoot);
            chains = null;
        }
        return true;
    }

    /// <summary>
    /// 攻击目标，对目标添加闪电标记
    /// </summary>
    /// <param name="damager"></param>
    /// <param name="damageable"></param>
    public void AtkTarget(DamagerBase damager, DamageableBase damageable)
    {
        AddElectricMark((HpDamable)damageable);
    }

    public bool AddElectricMark(HpDamable target)
    {
        Debug.Log("添加闪电标志");
        if (!IsAddElectricMarkSuccess(target)||!target.CanGetBuff(BuffType.ElectricMark)) return false;
        target.GetBuff(BuffType.ElectricMark);
        return true;
    }

    public void UpdateTargetsLink()
    {
        if(ElectricMark.targets.Count < 2) return;
        bool needInitFirstTarget = true;
        int index = 0;
        foreach (var target in ElectricMark.targets)
        {
            if (needInitFirstTarget)
            {
                preTarget = target.Value;
                needInitFirstTarget = false;
            }
            else
            {
                LightningChainUpdate(preTarget.transform.position,
                    target.Value.transform.position, index);
                index++;
                preTarget = target.Value;
            }
        }
    }


    
    

    protected bool IsAtkSuccess(HpDamable target)
    {
        return true;
    }

    // TODO:实现挂载闪电标记的逻辑
    private bool IsAddElectricMarkSuccess(HpDamable target)
    {
        if (target == null)
        {
            return false;
        }
        return (GetComponentInParent<Transform>().position - target.transform.position).magnitude <= range;
    }

    private int Damage()
    {
        var damage = baseDamage * (1.0f + extraDamagePercent * ElectricMark.counter);
        Debug.Log("闪电链造成： " + damage);
        return (int)damage;
    }



    private void LightningChainUpdate(Vector3 start, Vector3 end, int chainIndex)
    {
        if (chains == null) chains = new Dictionary<int, GameObject>();
        if (chainsRoot == null)
        {
            chainsRoot = new GameObject();
            chainsRoot.name = "Chains Root";
        }

        float len = Vector3.Distance(start, end);
        float theta = Mathf.Atan2(end.y-start.y, end.x-start.x)/(Mathf.PI)*180;
        //float theta = Vector3.Angle(start, end);

        if (chains.ContainsKey(chainIndex))
        {
            GameObject chainGo = chains[chainIndex];
            chainGo.transform.localPosition = start;
            chainGo.transform.localRotation = Quaternion.Euler(theta, -90.0f, 0.0f);
            chainGo.transform.localScale = new Vector3(len, chainGo.transform.localScale.y, chainGo.transform.localScale.z);
        }
        else
        {
            GameObject lightningChainGo = Instantiate(lightningChainPref, chainsRoot.transform);
            //lightningChainGo.transform.localRotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);
            chains.Add(chainIndex, lightningChainGo);
        }

    }
    private void DrawLightningChain(Vector3 start, Vector3 end, int chainIndex)
    {
        if(chains == null) chains = new Dictionary<int, GameObject>();
        if (chainsRoot == null)
        {
            chainsRoot = new GameObject();
            chainsRoot.name = "Chains Root";
        }
        if (lightningChainMat is null)
        {
            Debug.LogError("no lightning Chain Material");
            return;
        }

        Vector3 offset = new Vector3(0, width * 0.5f, 0);
        Vector3[] verts = new Vector3[4];
        verts[0] = start + offset;
        verts[1] = start - offset;
        verts[2] = end + offset;
        verts[3] = end - offset;
        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 3;
        triangles[4] = 2;
        triangles[5] = 1;

        Mesh lightningChainMash = new Mesh();
        lightningChainMash.vertices = verts;
        lightningChainMash.triangles = triangles;

        if (chains.ContainsKey(chainIndex))
        {
            GameObject chainGo = chains[chainIndex];
            chainGo.GetComponent<MeshFilter>().mesh = lightningChainMash;
        }
        else
        {
            GameObject lightningChainGo = new GameObject();
            lightningChainGo.AddComponent<MeshFilter>().mesh = lightningChainMash;
            lightningChainGo.AddComponent<MeshRenderer>().sharedMaterial = lightningChainMat;
            lightningChainGo.transform.SetParent(chainsRoot.transform);
            chains.Add(chainIndex, lightningChainGo);
        }
    }

    private LightningChain Clone()
    {
        return (LightningChain)MemberwiseClone();
        return GameObject.Instantiate(this);
    }
    
    public void ChangeState(bool _option)
    {
        stateParticle.gameObject.SetActive(_option);
    }
    */
}