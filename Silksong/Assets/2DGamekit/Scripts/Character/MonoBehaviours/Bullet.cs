using UnityEngine;
using UnityEngine.Tilemaps;

namespace Gamekit2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Damager))]
    public class Bullet : MonoBehaviour
    {
        public bool destroyWhenOutOfView = true;
        public bool spriteOriginallyFacesLeft;

        [Tooltip("If -1 never auto destroy, otherwise bullet is return to pool when that time is reached")]
        public float timeBeforeAutodestruct = -1.0f;
    
        [HideInInspector]
        public BulletObject bulletPoolObject;
        [HideInInspector]
        public Camera mainCamera;

        protected SpriteRenderer m_SpriteRenderer;
        static readonly int VFX_HASH = VFXController.StringToHash("BulletImpact");

        const float k_OffScreenError = 0.01f;

        protected float m_Timer;

        private void OnEnable()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_Timer = 0.0f;
        }

        public void ReturnToPool ()
        {
            bulletPoolObject.ReturnToPool ();
        }

        void FixedUpdate ()
        {
            if (destroyWhenOutOfView)
            {
                Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);
                bool onScreen = screenPoint.z > 0 && screenPoint.x > -k_OffScreenError &&
                                screenPoint.x < 1 + k_OffScreenError && screenPoint.y > -k_OffScreenError &&
                                screenPoint.y < 1 + k_OffScreenError;
                if (!onScreen)
                    bulletPoolObject.ReturnToPool();
            }

            if (timeBeforeAutodestruct > 0)
            {
                m_Timer += Time.deltaTime;
                if (m_Timer > timeBeforeAutodestruct)
                {
                    bulletPoolObject.ReturnToPool();
                }
            }
        }

        public void OnHitDamageable(Damager origin, Damageable damageable)
        {
            FindSurface(origin.LastHit);
        }

        public void OnHitNonDamageable(Damager origin)
        {
            FindSurface(origin.LastHit);
        }

        protected void FindSurface(Collider2D collider)
        {
            Vector3 forward = spriteOriginallyFacesLeft ? Vector3.left : Vector3.right;
            if (m_SpriteRenderer.flipX) forward.x = -forward.x;

            TileBase surfaceHit = PhysicsHelper.FindTileForOverride(collider, transform.position, forward);

            VFXController.Instance.Trigger(VFX_HASH, transform.position, 0, m_SpriteRenderer.flipX, null, surfaceHit);
        }
    }
}
