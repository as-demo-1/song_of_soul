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
    public enum CreatePos
    {
        thisPos = 0,
        PlayerPos = 1,
        RandomPos = 2
    }
    public enum ShootDir
    {
        fromShootPos = 0,
        toTarget = 1,
        Random = 2
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
        public CreatePos createPos;
        public float bulletSpeed;
        public float shootOffsetAngle;
    }



    public List<shootParam> shootModes;
    private bool isShoot;
    private Vector2 mainDir;
    private Vector2 createPosition;
    void Start()
    {
        
    }

    public void Shoot(string shootMode)
    {
        foreach(var value in shootModes)
        {
            if(value.shootModeName==shootMode)
            {
                Fire(value);
                return;
            }
        }
        Debug.LogError(transform.gameObject.name + " Shoot_mode_list could not find shoot mode");
    }
   
    private void Fire(shootParam Param)
    {
        if (Param.bullet == null)
            return;

        

        switch (Param.shootMethod)
        {
            case ShootMethod.ShootOnce: ShootOnce(Param); break;
            case ShootMethod.ShootOneByOne: StartCoroutine(ShootOneByOne(Param)); break;
            case ShootMethod.ShootRandomly: ShootRandomly(Param); break;
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
        GameObject tem = Instantiate<GameObject>(bullet, CreatePos, Quaternion.identity, this.transform);
        tem.GetComponent<Rigidbody2D>().velocity = ShootVector;
    }
    private void ShootOnce(shootParam Param)
    {
        switch (Param.createPos)
        {
            case CreatePos.PlayerPos: createPosition = Param.target.transform.position; break;
            case CreatePos.RandomPos: createPosition = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(1, 10) + (Vector2)transform.position; break;
            case CreatePos.thisPos: createPosition = transform.position; break;
        }

        if (Param.createPos != CreatePos.PlayerPos)
        {
            switch (Param.shotDir)
            {
                case ShootDir.toTarget: mainDir = ((Vector2)Param.target.transform.position - createPosition).normalized; break;
                case ShootDir.fromShootPos: mainDir = (createPosition - (Vector2)transform.position).normalized; break;
                case ShootDir.Random: mainDir = UnityEngine.Random.insideUnitCircle; break;
            }
        }
        else
            mainDir = UnityEngine.Random.insideUnitCircle;

        mainDir = AddOffset(mainDir, Param.shootOffsetAngle);

        CreateBullet(Param.bullet, createPosition, mainDir * Param.bulletSpeed);
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
            yield return new WaitForSeconds(0.2f);
        }
    }


    private void ShootRandomly(shootParam Param)
    {

    }

    private void Update()
    {
        
    }
}
