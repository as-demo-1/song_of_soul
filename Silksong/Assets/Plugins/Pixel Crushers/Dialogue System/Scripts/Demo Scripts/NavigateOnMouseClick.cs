// Based on: http://wiki.unity3d.com/index.php/Click_To_Move_C
// By: Vinicius Rezendrix
using UnityEngine;
using UnityEngine.AI;

namespace PixelCrushers.DialogueSystem.Demo
{

    /// <summary>
    /// Navigates to the place where the player mouse clicks.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavigateOnMouseClick : MonoBehaviour
    {
        public string animatorSpeedParameter = "Speed";

        public float stoppingDistance = 0.5f;

        public enum MouseButtonType { Left, Right, Middle };
        public MouseButtonType mouseButton = MouseButtonType.Left;

        public bool ignoreClicksOnUI = true;

        private Transform m_myTransform;
        private Animator m_animator;
        private NavMeshAgent m_navMeshAgent;

        void Awake()
        {
            m_myTransform = transform;
            m_animator = GetComponent<Animator>();
            m_navMeshAgent = GetComponent<NavMeshAgent>();
            if (m_navMeshAgent == null)
            {
                Debug.LogWarning("Dialogue System: NavigateOnMouseClick didn't find a NavMeshAgent on " + name + ". Disabling component.", this);
                enabled = false;
            }
#if USE_NEW_INPUT
            Debug.LogWarning("Dialogue System: NavigateOnMouseClick doesn't support the new input system.");
            enabled = false;
#endif
        }

        void Update()
        {
            // Set animator to reflect NavMeshAgent speed:
            if (!(m_animator == null || string.IsNullOrEmpty(animatorSpeedParameter)))
            {
                m_animator.SetFloat(animatorSpeedParameter, m_navMeshAgent.velocity.magnitude);
            }

            // Ingnore clicks on UI:
            if (ignoreClicksOnUI && UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // Moves the Player if the Left Mouse Button was clicked:
            if (Input.GetMouseButtonDown((int)mouseButton) && GUIUtility.hotControl == 0)
            {
                Plane playerPlane = new Plane(Vector3.up, m_myTransform.position);
                Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                float hitdist = 0.0f;

                if (playerPlane.Raycast(ray, out hitdist))
                {
                    m_navMeshAgent.SetDestination(ray.GetPoint(hitdist));
                }
            }

            // Moves the player if the mouse button is held down:
            else if (Input.GetMouseButton((int)mouseButton) && GUIUtility.hotControl == 0)
            {

                Plane playerPlane = new Plane(Vector3.up, m_myTransform.position);
                Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                float hitdist = 0.0f;

                if (playerPlane.Raycast(ray, out hitdist))
                {
                    m_navMeshAgent.SetDestination(ray.GetPoint(hitdist));
                }
            }
        }
    }
}
