using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


[Serializable]
public class LightningChain : SoulSkill
{
    public float range = 1.0f;
    [Range(0,1)]
    public float extraSpeedPercent = 0;
    public float baseDamage = 5;
    [Range(0,1)]
    public float extraDamagePercent = 1;

    HpDamable preTarget;

    public float moveSpeedUp;
    private void Awake()
    {
        //_playerInfomation = GetComponentInParent<PlayerInfomation>();
        _playerCharacter = GetComponentInParent<PlayerCharacter>();
    }

    void Start()
    {
        base.Start();
        //_playerInfomation = GetComponentInParent<PlayerInfomation>();
        //RefreshAtk(_playerInfomation.atk);
    }

    private void OnEnable()
    {
        //_playerInfomation = GetComponentInParent<PlayerInfomation>();
        base.OnEnable();
        //SpeedUp(true);
    }

    private void OnDisable()
    {
        base.OnDisable();
        //SpeedUp(false);
    }

    private LightningChain eventVariant;
    public void TriggerAddElectricMarkEvent()
    {
        if (eventVariant is null)
        {
            eventVariant = Clone();
            eventVariant.gameObject.SetActive(false);
        }
        EventCenter<BattleEventType>.Instance.TiggerEvent(BattleEventType.LightningAddElectricMarkEvent, eventVariant);
    }
    
    public override bool AtkPerTarget(HpDamable target)
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

    public bool AddElectricMark(HpDamable target)
    {
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

    
    protected override bool IsAtkSuccess(HpDamable target)
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
        return (GetComponentInParent<Transform>().position - target.transform.position).magnitude <= _atkDistance;
    }

    private int Damage()
    {
        var damage = baseDamage * (1.0f + extraDamagePercent * ElectricMark.counter);
        Debug.Log("闪电链造成： " + damage);
        return (int)damage;
    }

    public Material lightningChainMat;
    public GameObject lightningChainPref;
    private float width = 0.1f;
    private Dictionary<int, GameObject> chains = new Dictionary<int, GameObject>();
    private GameObject chainsRoot;

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
}
