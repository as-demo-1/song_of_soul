/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.SaveSystem
{
    using System.Collections.Generic;
    using Opsive.Shared.Utility;
    using UnityEngine;

    /// <summary>
    /// This component is used to save a Game object state of enabled/disabled components, plus position, rotation, scale.
    /// And can do the same for all children
    /// </summary>
    public class GameObjectSaver : SaverBase
    {
        [System.Serializable]
        public struct EnabledComponent
        {
            public string Key;
            public bool Enabled;
        }
        /// <summary>
        /// The currency owner save data.
        /// </summary>
        [System.Serializable]
        public struct GameObjectSaveData
        {
            public int Depth;
            public int SiblingIndex;
            public Vector3 Position;
            public Vector3 Rotation;
            public Vector3 Scale;
            public bool Active;
            public bool Destroyed;
            public EnabledComponent[] EnabledComponents;
        }
        
        /// <summary>
        /// The currency owner save data.
        /// </summary>
        [System.Serializable]
        public struct OriginGameObjectSaveData
        {
            public GameObjectSaveData[] GameObjectSaveDatas;
        }

        [SerializeField] protected GameObject m_TargetObject;
        [SerializeField] protected bool m_SavePosition;
        [SerializeField] protected bool m_SaveRotation;
        [SerializeField] protected bool m_SaveScale;
        [SerializeField] protected bool m_SaveActiveState;
        [SerializeField] protected bool m_SaveDestroyedState;
        [SerializeField] protected bool m_SaveComponentEnabledState;

        /// <summary>
        /// Get target game object on awake.
        /// </summary>
        protected override void Awake()
        {
            if (m_TargetObject == null) { m_TargetObject = gameObject; }
            base.Awake();
        }

        /// <summary>
        /// Serialize the save data.
        /// </summary>
        /// <returns>The serialized data.</returns>
        public override Serialization SerializeSaveData()
        {

            var saveDataList = new List<GameObjectSaveData>();

            AddSaveDataForGameObject(saveDataList, m_TargetObject, false, 0, 0);

            var saveData = new OriginGameObjectSaveData()
            {
                GameObjectSaveDatas = saveDataList.ToArray()
            };

            return Serialization.Serialize(saveData);
        }

        /// <summary>
        /// Add save data for the gameobject.
        /// </summary>
        /// <param name="saveData">The list of gameobject save data</param>
        /// <param name="obj">The game object to save.</param>
        /// <param name="saveChildrenRecursively">Save the children recursively?</param>
        /// <param name="depth">The currency child depth.</param>
        /// <param name="siblingIndex">The sibling index.</param>
        public virtual void AddSaveDataForGameObject(List<GameObjectSaveData> saveData, GameObject obj, bool saveChildrenRecursively, int depth, int siblingIndex)
        {
            if (obj == null) {
                saveData.Add(new GameObjectSaveData()
                {
                    Destroyed = true,
                });
                
                return;
            }

            GetEnabledComponentStates(obj);

            var gameObjectTransform = obj.transform;
            saveData.Add(new GameObjectSaveData() {
                Depth = depth,
                SiblingIndex = siblingIndex,
                Position = gameObjectTransform.position,
                Rotation = gameObjectTransform.rotation.eulerAngles,
                Scale =  gameObjectTransform.localScale,
                Active = obj.activeSelf,
                Destroyed = false,
                EnabledComponents = m_SaveComponentEnabledState? GetEnabledComponentStates(obj) : null,
            });

            if (saveChildrenRecursively == false) {
                return;
            }

            for (int i = 0; i < gameObjectTransform.childCount; i++) {
                var childGameObject = gameObjectTransform.GetChild(i).gameObject;
                AddSaveDataForGameObject(saveData, childGameObject, true, depth+1, i);
            }
        }

        /// <summary>
        /// Get an array of enabled components from the game object.
        /// </summary>
        /// <param name="obj">The game object.</param>
        /// <returns>The array of enabled components.</returns>
        protected virtual EnabledComponent[] GetEnabledComponentStates(GameObject obj)
        {
            var componenents = obj.GetComponents<Behaviour>();
            var enabledComponents = new EnabledComponent[componenents.Length];

            for (int i = 0; i < componenents.Length; i++) {
                enabledComponents[i] = new EnabledComponent()
                {
                    Key = componenents[i].name,
                    Enabled = componenents[i].enabled,
                };
            }

            return enabledComponents;
        }

        /// <summary>
        /// Deserialize and load the save data.
        /// </summary>
        /// <param name="serializedSaveData"></param>
        public override void DeserializeAndLoadSaveData(Serialization serializedSaveData)
        {
            var savedData = serializedSaveData.DeserializeFields(MemberVisibility.All) as OriginGameObjectSaveData?;

            if (savedData.HasValue == false) {
                return;
            }

            var gameObjectSaveDatas = savedData.Value.GameObjectSaveDatas;

            if (gameObjectSaveDatas == null || gameObjectSaveDatas.Length == 0) {
                return;
            }
            
            if (m_SaveDestroyedState && gameObjectSaveDatas[0].Destroyed) {
                DestroyImmediate(m_TargetObject);
                return;
            }

            LoadSaveDataOnGameObject(m_TargetObject, gameObjectSaveDatas[0]);
        }

        /// <summary>
        /// Load the save data for that game object.
        /// </summary>
        /// <param name="obj">The game object.</param>
        /// <param name="savedData">The save data.</param>
        private void LoadSaveDataOnGameObject(GameObject obj, GameObjectSaveData savedData)
        {

            var objTransform = obj.transform;

            if (m_SaveActiveState) {
                obj.SetActive(savedData.Active);
            }

            if (m_SaveComponentEnabledState && savedData.EnabledComponents != null && savedData.EnabledComponents.Length != 0) {
                //TODO use the Component Key to load the correct one.
                var componenents = obj.GetComponents<Behaviour>();
                for (int i = 0; i < componenents.Length; i++) {
                    if(savedData.EnabledComponents.Length <= i){ break; }

                    componenents[i].enabled = savedData.EnabledComponents[i].Enabled;
                }
            }
            
            if (m_SavePosition) {
                objTransform.position = savedData.Position;
            }

            if (m_SaveRotation) {
                objTransform.rotation = Quaternion.Euler(savedData.Rotation);
            }

            if (m_SaveScale) {
                objTransform.rotation = Quaternion.Euler(savedData.Scale);
            }
        }
    }
}