using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public BulletObject bullet;
    public GameObject PrefabToPool;
    //public int MaxPoolSize = 10;
    private Stack<BulletBehavior> pool;

    public BulletPool(BulletObject b)
    {
        pool = new Stack<BulletBehavior>();//MaxPoolSize);
        bullet = b;
        PrefabToPool = bullet.prefab;
    }

    public BulletBehavior GetObjectFromPool()
    {
        if (pool.Count <= 0)
        {
            pool.Push(CreateItem());
        }
        var obj = pool.Pop();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void ReturnObjectToPool(BulletBehavior objectToDeactivate)
    {
        if (objectToDeactivate != null)
        {
            objectToDeactivate.gameObject.SetActive(false);
            pool.Push(objectToDeactivate);
        }
    }

    private BulletBehavior CreateItem()
    {
        var bh = GameObject.Instantiate(bullet.prefab).AddComponent<BulletBehavior>();
        InitBullet(bh);
        bh.pool = this;
        return bh;
    }

    private void InitBullet(BulletBehavior bh)
    {
        bh.linearVelocity = bullet.linearVelocity;
        bh.angularAcceleration = bullet.angularAcceleration;
        bh.angularVelocity = bullet.angularVelocity;
        bh.lifeCycle = bullet.lifeCycle;
        bh.maxVelocity = bullet.maxVelocity;
    }

    public void OnGetItem(BulletBehavior bh)
    {
        InitBullet(bh);
        bh.gameObject.SetActive(true);
    }
}
