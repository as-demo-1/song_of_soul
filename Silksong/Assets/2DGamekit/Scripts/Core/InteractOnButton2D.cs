using UnityEngine;
using UnityEngine.Events;

namespace Gamekit2D
{
    public class InteractOnButton2D : InteractOnTrigger2D
    {
        public UnityEvent OnButtonPress;

        bool m_CanExecuteButtons;

        protected override void ExecuteOnEnter(Collider2D other)
        {
            m_CanExecuteButtons = true;
            OnEnter.Invoke ();
        }

        protected override void ExecuteOnExit(Collider2D other)
        {
            m_CanExecuteButtons = false;
            OnExit.Invoke ();
        }

        void Update()
        {
            if (m_CanExecuteButtons)
            {
                if (OnButtonPress.GetPersistentEventCount() > 0 && PlayerInput.Instance.Interact.Down)
                    OnButtonPress.Invoke();
            }
        }
    }
}