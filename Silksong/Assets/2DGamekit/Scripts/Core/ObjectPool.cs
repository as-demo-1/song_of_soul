using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public abstract class ObjectPool<TPool, TObject, TInfo> : ObjectPool<TPool, TObject>
        where TPool : ObjectPool<TPool, TObject, TInfo>
        where TObject : PoolObject<TPool, TObject, TInfo>, new()
    {
        void Start()
        {
            for (int i = 0; i < initialPoolCount; i++)
            {
                TObject newPoolObject = CreateNewPoolObject();
                pool.Add(newPoolObject);
            }
        }

        public virtual TObject Pop(TInfo info)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i].inPool)
                {
                    pool[i].inPool = false;
                    pool[i].WakeUp(info);
                    return pool[i];
                }
            }

            TObject newPoolObject = CreateNewPoolObject();
            pool.Add(newPoolObject);
            newPoolObject.inPool = false;
            newPoolObject.WakeUp(info);
            return newPoolObject;
        }
    }

    public abstract class ObjectPool<TPool, TObject> : MonoBehaviour
        where TPool : ObjectPool<TPool, TObject>
        where TObject : PoolObject<TPool, TObject>, new()
    {
        public GameObject prefab;
        public int initialPoolCount = 10;
        [HideInInspector]
        public List<TObject> pool = new List<TObject>();

        void Start()
        {
            for (int i = 0; i < initialPoolCount; i++)
            {
                TObject newPoolObject = CreateNewPoolObject();
                pool.Add(newPoolObject);
            }
        }

        protected TObject CreateNewPoolObject()
        {
            TObject newPoolObject = new TObject();
            newPoolObject.instance = Instantiate(prefab);
            newPoolObject.instance.transform.SetParent(transform);
            newPoolObject.inPool = true;
            newPoolObject.SetReferences(this as TPool);
            newPoolObject.Sleep();
            return newPoolObject;
        }

        public virtual TObject Pop()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i].inPool)
                {
                    pool[i].inPool = false;
                    pool[i].WakeUp();
                    return pool[i];
                }
            }

            TObject newPoolObject = CreateNewPoolObject();
            pool.Add(newPoolObject);
            newPoolObject.inPool = false;
            newPoolObject.WakeUp();
            return newPoolObject;
        }

        public virtual void Push(TObject poolObject)
        {
            poolObject.inPool = true;
            poolObject.Sleep();
        }
    }

    [Serializable]
    public abstract class PoolObject<TPool, TObject, TInfo> : PoolObject<TPool, TObject>
        where TPool : ObjectPool<TPool, TObject, TInfo>
        where TObject : PoolObject<TPool, TObject, TInfo>, new()
    {
        public virtual void WakeUp(TInfo info)
        { }
    }

    [Serializable]
    public abstract class PoolObject<TPool, TObject>
        where TPool : ObjectPool<TPool, TObject>
        where TObject : PoolObject<TPool, TObject>, new()
    {
        public bool inPool;
        public GameObject instance;
        public TPool objectPool;

        public void SetReferences(TPool pool)
        {
            objectPool = pool;
            SetReferences();
        }

        protected virtual void SetReferences()
        { }

        public virtual void WakeUp()
        { }

        public virtual void Sleep()
        { }

        public virtual void ReturnToPool()
        {
            TObject thisObject = this as TObject;
            objectPool.Push(thisObject);
        }
    }
}