using UnityEngine;

namespace PixelCrushers.DialogueSystem.Demo
{

    /// <summary>
    /// This component implements a very simple third person shooter-style controller.
    /// The mouse rotates the character, the axes (WASD/arrow keys) move, and the Fire1
    /// button (left mouse button) fires.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    [RequireComponent(typeof(CharacterController))]
    public class SimpleController : MonoBehaviour
    {

        [Header("Animator")]

        [Tooltip("Float parameter defined in animator controller that controls forward/backward speed.")]
        public string forwardSpeedFloatParameter = "Speed";

        [Tooltip("Float parameter defined in animator controller that controls left/right side-step speed.")]
        public string lateralSpeedFloatParameter = "Strafe";

        [Tooltip("Bool parameter defined in animator controller that specifies whether to use two-hand weapon animation or one-hand.")]
        public string twoHandWeaponBoolParameter = "Rifle";

        [Tooltip("Trigger parameter defined in animator controller that makes the animator play an attack animation.")]
        public string attackTriggerParameter = "Fire";

        [Header("Movement & Camera")]

        [Tooltip("Speed at which player moves if animator doesn't use root motion.")]
        public float runSpeed = 5f;

        [Tooltip("Mouse look rotation sensitivity.")]
        public float mouseSensitivityX = 15f;
        public float mouseSensitivityY = 10f;

        [Tooltip("Maximum up/down angles for mouse look.")]
        public float mouseMinimumY = -60f;
        public float mouseMaximumY = 60f;

        [Header("Attack")]

        [Tooltip("Use two-hand weapon animation.")]
        public bool useTwoHandWeapon = false;

        [Tooltip("Attack animation checks for target hit at this time in animation.")]
        public float hitDelay = 0.3f;

        [Tooltip("Play this sound at Hit Delay time.")]
        public AudioClip attackSound;

        [Tooltip("Distance at which attack can hit target.")]
        public float hitDistance = 100f;

        [Tooltip("Check for targets on these layers.")]
        public LayerMask hitLayerMask = 1;

        [Tooltip("Send this message to targets that are hit.")]
        public string damageMessage = "TakeDamage";

        [Tooltip("Send with parameter with the Damage Message.")]
        public float weaponDamage = 100;

        [Header("Input")]

        public string horizontalAxis = "Horizontal";
        public string verticalAxis = "Vertical";
        public string mouseXAxis = "Mouse X";
        public string mouseYAxis = "Mouse Y";
        public string attackButton = "Fire1";

        private CharacterController m_controller = null;
        private SmoothCameraWithBumper m_smoothCamera = null;
        private Animator m_animator = null;
        private float m_cameraRotationY = 0;
        private Quaternion m_originalCameraRotation;
        private bool m_firing = false;

        void Awake()
        {
            m_controller = GetComponent<CharacterController>();
            m_smoothCamera = GetComponentInChildren<SmoothCameraWithBumper>();
            m_animator = GetComponent<Animator>();
        }

        void Start()
        {
            var camera = UnityEngine.Camera.main;
            m_originalCameraRotation = (camera != null) ? camera.transform.localRotation : Quaternion.identity;
        }

        void Update()
        {
            if (Time.timeScale <= 0) return;

#if USE_NEW_INPUT
            var mouseX = UnityEngine.InputSystem.Mouse.current.delta.x.ReadValue() * Time.deltaTime;
            var mouseY = UnityEngine.InputSystem.Mouse.current.delta.y.ReadValue() * Time.deltaTime;
#else
            var mouseX = InputDeviceManager.GetAxis(mouseXAxis); // Input Manager already multiplies mouse axes by Time.deltaTime.
            var mouseY = InputDeviceManager.GetAxis(mouseYAxis);
#endif

            // Mouse X rotation:
            transform.Rotate(0, mouseX * mouseSensitivityX, 0);

            // Mouse Y rotation:
            m_cameraRotationY += mouseY * mouseSensitivityY;
            m_cameraRotationY = ClampAngle(m_cameraRotationY, mouseMinimumY, mouseMaximumY);
            Quaternion yQuaternion = Quaternion.AngleAxis(m_cameraRotationY, -Vector3.right);
            if (m_smoothCamera != null)
            {
                // If we have a SmoothCameraWithBumper, leave camera adjustments to it:
                m_smoothCamera.adjustQuaternion = yQuaternion;
            }
            else
            {
                UnityEngine.Camera.main.transform.localRotation = m_originalCameraRotation * yQuaternion;
            }

            // Weapon:
            if (m_animator != null) m_animator.SetBool(twoHandWeaponBoolParameter, useTwoHandWeapon);

            // Firing:
            if (DialogueManager.GetInputButtonDown(attackButton) && !m_firing)
            {
                if (m_animator != null) m_animator.SetTrigger(attackTriggerParameter);
                m_firing = true;
                Invoke("OnFired", hitDelay);
            }

            // Movement:
            float centralSpeed = InputDeviceManager.GetAxis(verticalAxis);
            float lateralSpeed = InputDeviceManager.GetAxis(horizontalAxis);
            if ((Mathf.Abs(centralSpeed) > 0.1f) || (Mathf.Abs(lateralSpeed) > 0.1f))
            {
                SetSpeed(centralSpeed, lateralSpeed);
            }
            else
            {
                SetSpeed(0, 0);
            }

            // Move, including gravity:
            if (m_animator == null || !m_animator.applyRootMotion)
            {
                m_controller.Move(transform.rotation * ((Vector3.forward * centralSpeed * runSpeed * Time.deltaTime) + (Vector3.right * lateralSpeed * runSpeed * Time.deltaTime)) + Vector3.down * 20f * Time.deltaTime);
            }
        }

        /// <summary>
        /// When the character has fired, play the fire sound and check for hits.
        /// </summary>
        void OnFired()
        {
            m_firing = false;
            if (attackSound != null) AudioSource.PlayClipAtPoint(attackSound, transform.position);
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, hitDistance, hitLayerMask))
            {
                hit.collider.gameObject.BroadcastMessage(damageMessage, weaponDamage, SendMessageOptions.DontRequireReceiver);
            }
        }

        void SetSpeed(float forwardSpeed, float lateralSpeed)
        {
            if (m_animator != null)
            {
                if (!string.IsNullOrEmpty(forwardSpeedFloatParameter)) m_animator.SetFloat(forwardSpeedFloatParameter, forwardSpeed);
                if (!string.IsNullOrEmpty(lateralSpeedFloatParameter)) m_animator.SetFloat(lateralSpeedFloatParameter, lateralSpeed);
            }
        }

        /// <summary>
        /// When the character is involved in a conversation, stop moving and firing.
        /// </summary>
        void OnConversationStart(Transform actor)
        {
            SetSpeed(0, 0);
            CancelInvoke("OnFired");
            m_firing = false;
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f) angle += 360f;
            if (angle > 360f) angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }

    }

}
