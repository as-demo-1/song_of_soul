using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Gamekit2D
{
    [RequireComponent(typeof(Collider2D))]
    public class TransitionPoint : MonoBehaviour
    {
        public enum TransitionType
        {
            DifferentZone, DifferentNonGameplayScene, SameScene,
        }


        public enum TransitionWhen
        {
            ExternalCall, InteractPressed, OnTriggerEnter,
        }

    
        [Tooltip("This is the gameobject that will transition.  For example, the player.")]
        public GameObject transitioningGameObject;
        [Tooltip("Whether the transition will be within this scene, to a different zone or a non-gameplay scene.")]
        public TransitionType transitionType;
        [SceneName]
        public string newSceneName;
        [Tooltip("The tag of the SceneTransitionDestination script in the scene being transitioned to.")]
        public SceneTransitionDestination.DestinationTag transitionDestinationTag;
        [Tooltip("The destination in this scene that the transitioning gameobject will be teleported.")]
        public TransitionPoint destinationTransform;
        [Tooltip("What should trigger the transition to start.")]
        public TransitionWhen transitionWhen;
        [Tooltip("The player will lose control when the transition happens but should the axis and button values reset to the default when control is lost.")]
        public bool resetInputValuesOnTransition = true;
        [Tooltip("Is this transition only possible with specific items in the inventory?")]
        public bool requiresInventoryCheck;
        [Tooltip("The inventory to be checked.")]
        public InventoryController inventoryController;
        [Tooltip("The required items.")]
        public InventoryController.InventoryChecker inventoryCheck;
    
        bool m_TransitioningGameObjectPresent;

        void Start ()
        {
            if (transitionWhen == TransitionWhen.ExternalCall)
                m_TransitioningGameObjectPresent = true;
        }

        void OnTriggerEnter2D (Collider2D other)
        {
            if (other.gameObject == transitioningGameObject)
            {
                m_TransitioningGameObjectPresent = true;

                if (ScreenFader.IsFading || SceneController.Transitioning)
                    return;

                if (transitionWhen == TransitionWhen.OnTriggerEnter)
                    TransitionInternal ();
            }
        }

        void OnTriggerExit2D (Collider2D other)
        {
            if (other.gameObject == transitioningGameObject)
            {
                m_TransitioningGameObjectPresent = false;
            }
        }

        void Update ()
        {
            if (ScreenFader.IsFading || SceneController.Transitioning)
                return;

            if(!m_TransitioningGameObjectPresent)
                return;

            if (transitionWhen == TransitionWhen.InteractPressed)
            {
                if (PlayerInput.Instance.Interact.Down)
                {
                    TransitionInternal ();
                }
            }
        }

        protected void TransitionInternal ()
        {
            if (requiresInventoryCheck)
            {
                if(!inventoryCheck.CheckInventory (inventoryController))
                    return;
            }
        
            if (transitionType == TransitionType.SameScene)
            {
                GameObjectTeleporter.Teleport (transitioningGameObject, destinationTransform.transform);
            }
            else
            {
                SceneController.TransitionToScene (this);
            }
        }

        public void Transition ()
        {
            if(!m_TransitioningGameObjectPresent)
                return;

            if(transitionWhen == TransitionWhen.ExternalCall)
                TransitionInternal ();
        }
    }
}