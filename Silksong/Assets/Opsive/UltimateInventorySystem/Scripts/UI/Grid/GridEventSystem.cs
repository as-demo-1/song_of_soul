/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Grid
{
    using Opsive.UltimateInventorySystem.Input;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// The grid event system is used to listen to any event that occur on a grid.
    /// </summary>
    public class GridEventSystem : MonoBehaviour
    {
        public event Action<int> OnGridElementSelectedE;
        public event Action<int> OnGridElementClickE;
        public event Action<int> OnGridElementDeselectE;
        public event Action<int> OnGridElementCancelE;
        public event Action<int, PointerEventData> OnGridElementPointerDownE;
        public event Action<int, PointerEventData> OnGridElementPointerUpE;
        public event Action<int, PointerEventData> OnGridElementBeginDragE;
        public event Action<int, PointerEventData> OnGridElementEndDragE;
        public event Action<int, PointerEventData> OnGridElementDragE;
        public event Action<int, PointerEventData> OnGridElementDropE;
        public event Action OnUnavailableNavigationRight;
        public event Action OnUnavailableNavigationLeft;
        public event Action OnUnavailableNavigationUp;
        public event Action OnUnavailableNavigationDown;

        [Tooltip("The grid button prefab. Requires the CustomButton component.")]
        [SerializeField] protected GameObject m_ButtonPrefab;
        [Tooltip("Are the boxes in the grid draggable (Not compatible with all grids.).")]
        [SerializeField] protected bool m_Draggable = true;
        [Tooltip("Content Transform containing the grid elements.")]
        [SerializeField] protected Transform m_Content;
        [Tooltip("The grid layout group, (can be null).")]
        [SerializeField] protected GridLayoutGroup m_GridLayoutGroup;

        protected ActionButton[] m_Buttons;
        protected Vector2Int m_GridSize;

        protected int m_GridSizeCount;
        [System.NonSerialized] protected bool m_Initialized;

        public GameObject ButtonPrefab {
            get => m_ButtonPrefab;
            set => m_ButtonPrefab = value;
        }

        public Transform Content
        {
            get { return m_Content; }
            set => m_Content = value;
        }

        public GridLayoutGroup GridLayoutGroup
        {
            get { return m_GridLayoutGroup; }
            set => m_GridLayoutGroup = value;
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        private void Awake()
        {
            if (m_Content == null) { m_Content = transform; }

            if (m_GridLayoutGroup == null) { m_GridLayoutGroup = m_Content.GetComponent<GridLayoutGroup>(); }
        }

        /// <summary>
        /// Initialize the grid buttons and the event listeners.
        /// </summary>
        /// <param name="gridSize">The grid size.</param>
        public void Initialize(Vector2Int gridSize)
        {
            if (m_Initialized) { return; }

            if (m_Content == null) { m_Content = transform; }

            m_GridSize = gridSize;
            m_GridSizeCount = m_GridSize.x * m_GridSize.y;

            m_Buttons = new ActionButton[m_GridSizeCount];

            for (int i = 0; i < m_Buttons.Length; i++) {
                if (i >= m_Content.childCount) {
                    if (m_ButtonPrefab == null || m_ButtonPrefab.GetComponent<ActionButton>() == null) {
                        Debug.LogError("The Button Prefab is null or missing the ActionButton component, Please assign a prefab to the Button Prefab Field.", gameObject);
                        return;
                    }
                    m_Buttons[i] = Instantiate(m_ButtonPrefab, m_Content).GetComponent<ActionButton>();
                } else {
                    m_Buttons[i] = m_Content.GetChild(i).GetComponent<ActionButton>();
                }

                var localIndex = i;
                m_Buttons[i].OnSubmitE += () => ClickedButton(localIndex);
                m_Buttons[i].OnSelectE += () => SelectedButton(localIndex);
                m_Buttons[i].OnDeselectE += () => OnDeselectButton(localIndex);
                m_Buttons[i].OnCancelE += () => OnCancelButton(localIndex);
                m_Buttons[i].OnMoveE += (eventData, moved) => OnMove(eventData, moved, localIndex);
                if (m_Draggable) {
                    m_Buttons[i].OnPointerDownE += (eventData) => OnPointerDownButton(localIndex, eventData);
                    m_Buttons[i].OnPointerUpE += (eventData) => OnPointerUpButton(localIndex, eventData);
                    m_Buttons[i].OnBeginDragE += (eventData) => OnBeginDragButton(localIndex, eventData);
                    m_Buttons[i].OnEndDragE += (eventData) => OnEndDragButton(localIndex, eventData);
                    m_Buttons[i].OnDragE += (eventData) => OnDragButton(localIndex, eventData);
                    m_Buttons[i].OnDropE += (eventData) => OnDropButton(localIndex, eventData);
                }
            }

            for (int i = m_Buttons.Length; i < m_Content.childCount; i++) {
                m_Content.GetChild(i).gameObject.SetActive(false);
            }

            m_Initialized = true;
        }

        /// <summary>
        /// Spawn the buttons in the editor.
        /// </summary>
        /// <param name="gridSize">The grid size.</param>
        public void SpawnButtons(Vector2Int gridSize)
        {
            if (m_ButtonPrefab == null) {
                Debug.LogWarning("Cannot spawn buttons because the prefab is null", gameObject);
                return;
            }

            var buttonsCount = gridSize.x * gridSize.y;

            if (m_Content == null) { m_Content = transform; }

            for (int i = 0; i < buttonsCount; i++) {
                if (i >= m_Content.childCount) {
                    Instantiate(m_ButtonPrefab, m_Content).GetComponent<ActionButton>();
                } else {
                    var button = m_Content.GetChild(i).GetComponent<ActionButton>();
                    if (button != null) {
                        button.gameObject.SetActive(true);
                        continue;
                    }

                    Instantiate(m_ButtonPrefab, m_Content).GetComponent<ActionButton>();
                    m_Content.GetChild(i).SetAsLastSibling();
                }
            }

            for (int i = m_Content.childCount - 1; i >= buttonsCount; i--) {
                m_Content.GetChild(i).gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// A button was clicked.
        /// </summary>
        /// <param name="index">The button index.</param>
        private void ClickedButton(int index)
        {
            OnGridElementClickE?.Invoke(index);
        }

        /// <summary>
        /// A button was selected.
        /// </summary>
        /// <param name="index">The button index.</param>
        private void SelectedButton(int index)
        {
            OnGridElementSelectedE?.Invoke(index);
        }

        /// <summary>
        /// The button was deslected.
        /// </summary>
        /// <param name="index">The button index.</param>
        private void OnDeselectButton(int index)
        {
            OnGridElementDeselectE?.Invoke(index);
        }

        /// <summary>
        /// The button was canceled.
        /// </summary>
        /// <param name="index">The index of the button.</param>
        private void OnCancelButton(int index)
        {
            OnGridElementCancelE?.Invoke(index);
        }

        /// <summary>
        /// A move event when going from one selectable to another.
        /// </summary>
        /// <param name="eventData">The eventData.</param>
        /// <param name="moved">Did ite move successfully.</param>
        /// <param name="index">The button index.</param>
        private void OnMove(AxisEventData eventData, bool moved, int index)
        {
            switch (eventData.moveDir) {
                case MoveDirection.Left:
                    if (!moved) { OnUnavailableNavigationLeft?.Invoke(); }
                    break;
                case MoveDirection.Up:
                    if (!moved) { OnUnavailableNavigationUp?.Invoke(); }
                    break;
                case MoveDirection.Right:
                    if (!moved) { OnUnavailableNavigationRight?.Invoke(); }
                    break;
                case MoveDirection.Down:
                    if (!moved) { OnUnavailableNavigationDown?.Invoke(); }
                    break;
            }
        }

        /// <summary>
        /// On Pointer Down Button.
        /// </summary>
        /// <param name="index">The button index.</param>
        /// <param name="eventData">The event data.</param>
        protected virtual void OnPointerDownButton(int index, PointerEventData eventData)
        {
            OnGridElementPointerDownE?.Invoke(index, eventData);
        }

        /// <summary>
        /// On Pointer Up Button.
        /// </summary>
        /// <param name="index">The button index.</param>
        /// <param name="eventData">The event data.</param>
        protected virtual void OnPointerUpButton(int index, PointerEventData eventData)
        {
            OnGridElementPointerUpE?.Invoke(index, eventData);
        }

        /// <summary>
        /// On Begin Drag Button.
        /// </summary>
        /// <param name="index">The button index.</param>
        /// <param name="eventData">The event data.</param>
        protected virtual void OnBeginDragButton(int index, PointerEventData eventData)
        {
            OnGridElementBeginDragE?.Invoke(index, eventData);
        }

        /// <summary>
        /// On End Drag Button.
        /// </summary>
        /// <param name="index">The button index.</param>
        /// <param name="eventData">The event data.</param>
        protected virtual void OnEndDragButton(int index, PointerEventData eventData)
        {
            OnGridElementEndDragE?.Invoke(index, eventData);
        }

        /// <summary>
        /// On Drag Button.
        /// </summary>
        /// <param name="index">The button index.</param>
        /// <param name="eventData">The event data.</param>
        protected virtual void OnDragButton(int index, PointerEventData eventData)
        {
            OnGridElementDragE?.Invoke(index, eventData);
        }

        /// <summary>
        /// On Drop Button.
        /// </summary>
        /// <param name="index">The button index.</param>
        /// <param name="eventData">The event data.</param>
        protected virtual void OnDropButton(int index, PointerEventData eventData)
        {
            OnGridElementDropE?.Invoke(index, eventData);
        }

        /// <summary>
        /// Select the button.
        /// </summary>
        /// <param name="index">The button index.</param>
        public void SelectButton(int index)
        {
            if (index < 0 || index >= m_Buttons.Length) { return; }
            
            EventSystemManager.Select(m_Buttons[index].gameObject);
            //Button.Select does not call the OnSelect event. Therefore we need to make this call.
            SelectedButton(index);
        }

        /// <summary>
        /// Get the button.
        /// </summary>
        /// <param name="index">The button index.</param>
        /// <returns>The button.</returns>
        public ActionButton GetButton(int index)
        {
            return m_Buttons[index];
        }

        /// <summary>
        /// Select the first or last button in a row. 
        /// </summary>
        /// <param name="last">The last or first button.</param>
        public void SelectRowButton(bool last)
        {
            var selectedButton = GetSelectedButtonIndex();
            if (selectedButton == -1) { selectedButton = 0; }
            var row = selectedButton / m_GridSize.x;

            var buttonToSelect = last ?
                m_Buttons[(m_GridSize.x - 1) + row * m_GridSize.x]:
                m_Buttons[row * m_GridSize.x];
            
            EventSystemManager.Select(buttonToSelect.gameObject);
        }

        /// <summary>
        /// Select the first or last button in a column. 
        /// </summary>
        /// <param name="last">last or first.</param>
        public void SelectColumnButton(bool last)
        {
            var col = GetSelectedButtonIndex() / m_GridSize.y;

            var buttonToSelect = last ?
                m_Buttons[(m_GridSize.y - 1) + col * m_GridSize.y]:
                m_Buttons[col * m_GridSize.y];
            
            EventSystemManager.Select(buttonToSelect.gameObject);
        }

        /// <summary>
        /// Get the selected button index.
        /// </summary>
        /// <returns>The button index.</returns>
        public int GetSelectedButtonIndex()
        {
            var eventSystem = EventSystemManager.GetEvenSystemFor(gameObject);
            if (eventSystem == null)
                return -1;
            for (int i = 0; i < m_Buttons.Length; i++) {
                if (eventSystem.currentSelectedGameObject == m_Buttons[i].gameObject) { return i; }
            }

            return -1;
        }

        /// <summary>
        /// Get the selected Button.
        /// </summary>
        /// <returns>The selected button.</returns>
        public ActionButton GetSelectedButton()
        {
            var index = GetSelectedButtonIndex();
            if (index < 0 || index >= m_Buttons.Length) { return null; }

            return m_Buttons[index];
        }
    }
}
