using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using System;


[Serializable]
public class ShootSystem : MonoBehaviour
{
    public enum ShootMethod
    {
        ShootOnce = 0,
        ShootTogether = 1,
        ShootOneByOne = 2,
        ShootRandomly = 3,
        SpecialShoot = 4
    }
    public enum Pattern
    {
        Cycle = 0,
        Square = 1,
        Triangle = 2
    }
    public enum ShootDir
    {
        fromThisPos = 0,
        toTarget = 1,
        Random = 2,
        horizon = 3
    }
    [Serializable]
    public struct shootParam
    {
        public string shootModeName;
        public GameObject bullet;
        public int bulletNum;
        public ShootMethod shootMethod;
        public ShootDir shotDir;
        public GameObject target;
        public Transform createPos;
        public float bulletDelayTime;
        public float bulletSpeed;
        public float shootOffsetAngle;
    }



    public List<shootParam> shootModes;
    private Vector2 mainDir;
    void Start()
    {
        
    }

    public void Shoot(string shootMode)
    {
        foreach(var value in shootModes)
        {
            if(value.shootModeName.Equals(shootMode))
            {
                Fire(value);
                return;
            }
        }
        Debug.LogError("Shoot_mode_list could not find shoot mode:"+ shootMode);
    }
   
    private void Fire(shootParam Param)
    {
        if (Param.bullet == null)
            return;
        if (Param.target == null)
            Param.target = GameObject.FindGameObjectWithTag("Player");

        if (Param.createPos == null)
            Param.createPos = this.transform;

        switch (Param.shootMethod)
        {
            case ShootMethod.ShootOnce: ShootOnce(Param); break;
            case ShootMethod.ShootOneByOne: StartCoroutine(ShootOneByOne(Param)); break;
            case ShootMethod.ShootRandomly: StartCoroutine(ShootRandomly(Param)); break;
            case ShootMethod.ShootTogether: ShootTogether(Param); break;
        }
       
    }
    
    private Vector2 AddOffset(Vector2 a,float offsetAngle)
    {
        float o = (UnityEngine.Random.value-0.5f) * offsetAngle;
        Vector2 tem = new Vector2(Mathf.Cos(o), Mathf.Sin(o));
        return new Vector2(tem.x * a.x - tem.y * a.y, tem.x * a.y + tem.y * a.x).normalized;
    }

    private void CreateBullet(GameObject bullet, Vector2 CreatePos,Vector2 ShootVector)
    {
        GameObject tem = Instantiate<GameObject>(bullet, CreatePos, Quaternion.identity);
        tem.GetComponent<Rigidbody2D>().velocity = ShootVector;
    }
    private void ShootOnce(shootParam Param) 
    {
        switch (Param.shotDir)
        {
            case ShootDir.toTarget: mainDir = (Param.target.transform.position - Param.createPos.position).normalized; break;
            case ShootDir.fromThisPos: mainDir = (Param.createPos.position - transform.position).normalized; break;
            case ShootDir.Random: mainDir = UnityEngine.Random.insideUnitCircle; break;
            case ShootDir.horizon: mainDir = new Vector2(Param.target.transform.position.x - Param.createPos.transform.position.x, 0); break;
        }
        mainDir = AddOffset(mainDir, Param.shootOffsetAngle);
        CreateBullet(Param.bullet, Param.createPos.position, mainDir * Param.bulletSpeed);
    }
    private void ShootTogether(shootParam Param)
    {
        for (int i = 0; i < Param.bulletNum; i++)
            ShootOnce(Param);
    }
    private IEnumerator ShootOneByOne(shootParam Param)
    {
        for (int i = 0; i < Param.bulletNum; i++)
        {
            ShootOnce(Param);
            yield return new WaitForSeconds(Param.bulletDelayTime+0.2f);
        }
    }

    private IEnumerator ShootRandomly(shootParam Param)
    {
        for (int i = 0; i < UnityEngine.Random.Range(5, 10); i++)
        {
            ShootOnce(Param);
            yield return new WaitForSeconds(Param.bulletDelayTime + UnityEngine.Random.Range(0.8f, 1.2f));
        }
    }


    private void Update()
    {
        
    }
}
