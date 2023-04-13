/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Input
{
    using Opsive.Shared.Game;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.SceneManagement;
    
    public class EventSystemManager : MonoBehaviour
    {
        private static EventSystemManager s_Instance;
        private static EventSystemManager Instance
        {
            get
            {
                if (!s_Initialized) {
                    s_Instance = new GameObject("EventSystemManager").AddComponent<EventSystemManager>();
                    s_Initialized = true;
                }
                return s_Instance;
            }
        }
        private static bool s_Initialized;

        /// <summary>
        /// Reset the static variables for domain reloading.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void DomainReset()
        {
            s_Initialized = false;
            s_Instance = null;
        }

        protected static Dictionary<GameObject, EventSystem> m_GameObjectToEventSystemMap = new Dictionary<GameObject, EventSystem>();

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_GameObjectToEventSystemMap = new Dictionary<GameObject, EventSystem>();
        }

        /// <summary>
        /// The object has been enabled.
        /// </summary>
        private void OnEnable()
        {
            // The object may have been enabled outside of the scene unloading.
            if (s_Instance == null) {
                s_Instance = this;
                s_Initialized = true;
                SceneManager.sceneUnloaded -= SceneUnloaded;
            }
        }

        /// <summary>
        /// Reset the initialized variable when the scene is no longer loaded.
        /// </summary>
        /// <param name="scene">The scene that was unloaded.</param>
        private void SceneUnloaded(Scene scene)
        {
            s_Initialized = false;
            s_Instance = null;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneUnloaded += SceneUnloaded;
        }

        /// <summary>
        /// Select a gameobject.
        /// </summary>
        /// <param name="localGameObject">The local gameobject to select.</param>
        public static void Select(GameObject localGameObject)
        {
            Instance.SelectInternal(localGameObject);
        }
        
        /// <summary>
        /// Select the gameobject.
        /// </summary>
        /// <param name="localGameObject">The game object to select.</param>
        protected virtual void SelectInternal(GameObject localGameObject)
        {
            if (localGameObject == null) {
                return;
            }

            var eventSystem = EventSystemManager.GetEvenSystemFor(localGameObject);
            if (eventSystem == null || eventSystem.alreadySelecting) {
                return;
            }
            
            eventSystem.SetSelectedGameObject(localGameObject);
        }

        /// <summary>
        /// Get the event system for that game object. It can be different if using local multiplayer.
        /// </summary>
        /// <param name="localGameObject">The game object to get the event system for.</param>
        /// <returns>The event system for that game object.</returns>
        public static EventSystem GetEvenSystemFor(GameObject localGameObject)
        {
            return Instance.GetEventSystemForInternal(localGameObject);
        }

        /// <summary>
        /// Get the event system for that game object. It can be different if using local multiplayer.
        /// </summary>
        /// <param name="localGameObject">The game object to get the event system for.</param>
        /// <returns>The event system for that game object.</returns>
        protected virtual EventSystem GetEventSystemForInternal(GameObject localGameObject)
        {
            if (localGameObject == null) {
                return EventSystem.current;
            }

            if (m_GameObjectToEventSystemMap.TryGetValue(localGameObject, out var eventSystem)) {
                if (eventSystem != null) {
                    return eventSystem;
                }
            }

            var root = localGameObject.transform.root;
            var identifier = root.gameObject.GetCachedComponent<EventSystemIdentifier>();

            eventSystem = identifier == null || identifier.EventSystem == null ? EventSystem.current : identifier.EventSystem;
        
            m_GameObjectToEventSystemMap[localGameObject] = eventSystem;
        
            return eventSystem;
        }
    }
}