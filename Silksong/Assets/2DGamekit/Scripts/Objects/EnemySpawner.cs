using System.Collections;
using Gamekit2D;
using UnityEngine;

namespace Gamekit2D
{
    public class EnemySpawner : ObjectPool<EnemySpawner, Enemy, Vector2>, IDataPersister
    {
        public int totalEnemiesToBeSpawned;
        public int concurrentEnemiesToBeSpawned;
        public float spawnArea = 1.0f;
        public float spawnDelay;
        public float removalDelay;
        public DataSettings dataSettings;

        protected int m_TotalSpawnedEnemyCount;
        protected int m_CurrentSpawnedEnemyCount;
        protected Coroutine m_SpawnTimerCoroutine;
        protected WaitForSeconds m_SpawnWait;

        void OnEnable()
        {
            PersistentDataManager.RegisterPersister(this);
        }

        void OnDisable()
        {
            PersistentDataManager.UnregisterPersister(this);
        }

        void Start()
        {
            for (int i = 0; i < initialPoolCount; i++)
            {
                Enemy newEnemy = CreateNewPoolObject();
                pool.Add(newEnemy);
            }

            int spawnCount = Mathf.Min(totalEnemiesToBeSpawned - m_TotalSpawnedEnemyCount, concurrentEnemiesToBeSpawned);

            for (int i = 0; i < spawnCount; i++)
            {
                Pop(transform.position + transform.right * Random.Range(-spawnArea * 0.5f, spawnArea * 0.5f));
            }

            m_CurrentSpawnedEnemyCount = spawnCount;
            m_TotalSpawnedEnemyCount += concurrentEnemiesToBeSpawned;
            m_SpawnWait = new WaitForSeconds(spawnDelay);
        }

        public override void Push(Enemy poolObject)
        {
            poolObject.inPool = true;
            m_CurrentSpawnedEnemyCount--;
            poolObject.Sleep();
            StartSpawnTimer();
        }

        protected void StartSpawnTimer()
        {
            if (m_SpawnTimerCoroutine == null)
                m_SpawnTimerCoroutine = StartCoroutine(SpawnTimer());
        }

        protected IEnumerator SpawnTimer()
        {
            while (m_CurrentSpawnedEnemyCount < concurrentEnemiesToBeSpawned && m_TotalSpawnedEnemyCount < totalEnemiesToBeSpawned)
            {
                yield return m_SpawnWait;
                Pop(transform.position + transform.right * Random.Range(-spawnArea * 0.5f, spawnArea * 0.5f));
                m_CurrentSpawnedEnemyCount++;
                m_TotalSpawnedEnemyCount++;
            }

            m_SpawnTimerCoroutine = null;
        }

        public DataSettings GetDataSettings()
        {
            return dataSettings;
        }

        public void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType)
        {
            dataSettings.dataTag = dataTag;
            dataSettings.persistenceType = persistenceType;
        }

        public Data SaveData()
        {
            return new Data<int>(m_TotalSpawnedEnemyCount);
        }

        public void LoadData(Data data)
        {
            Data<int> enemyData = (Data<int>)data;
            m_TotalSpawnedEnemyCount = enemyData.value;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea, 0.4f, 0));
        }
    }
}


public class Enemy : PoolObject<EnemySpawner, Enemy, Vector2>
{
    public Damageable damageable;
    public EnemyBehaviour enemyBehaviour;

    protected WaitForSeconds m_RemoveWait;

    protected override void SetReferences()
    {
        damageable = instance.GetComponent<Damageable>();
        enemyBehaviour = instance.GetComponent<EnemyBehaviour>();

        damageable.OnDie.AddListener(ReturnToPoolEvent);

        m_RemoveWait = new WaitForSeconds(objectPool.removalDelay);
    }

    public override void WakeUp(Vector2 info)
    {
        enemyBehaviour.SetMoveVector(Vector2.zero);
        instance.transform.position = info;
        instance.SetActive(true);
        damageable.SetHealth(damageable.startingHealth);
        damageable.DisableInvulnerability();
        enemyBehaviour.contactDamager.EnableDamage();
        SceneLinkedSMB<EnemyBehaviour>.Initialise(enemyBehaviour.GetComponent<Animator>(), enemyBehaviour);
        enemyBehaviour.EndAttack();
    }

    public override void Sleep()
    {
        instance.SetActive(false);
        damageable.EnableInvulnerability();
    }

    protected void ReturnToPoolEvent(Damager dmgr, Damageable dmgbl)
    {
        objectPool.StartCoroutine(ReturnToPoolAfterDelay());
    }

    protected IEnumerator ReturnToPoolAfterDelay()
    {
        yield return m_RemoveWait;
        ReturnToPool();
    }
}
