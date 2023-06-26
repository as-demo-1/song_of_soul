/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.Shared.Editor.UIElements.Controls.Types;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.Utility;
    using System;
    using System.Collections.Generic;
    using Opsive.Shared.Utility;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements TypeControlBase for the Sprite ControlType.
    /// </summary>
    [ControlType(typeof(ItemShape))]
    public class ItemShapeControl : TypeControlBase
    {
        /// <summary>
        /// Does the control use a label?
        /// </summary>
        public override bool UseLabel { get { return true; } }

        /// <summary>
        /// Returns the control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="target">The object that should have its fields displayed.</param>
        /// <param name="field">The field responsible for the control (can be null).</param>
        /// <param name="arrayIndex">The index of the object within the array (-1 indicates no array).</param>
        /// <param name="type">The type of control being retrieved.</param>
        /// <param name="value">The value of the control.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        /// <param name="userdata">Optional data which can be used by the controls.</param>
        /// <returns>The created control.</returns>
        public override VisualElement GetControl(UnityEngine.Object unityObject, object target, System.Reflection.FieldInfo field, int arrayIndex, System.Type type, object value, System.Func<object, bool> onChangeEvent, object userData)
        {

            var itemShape = value as ItemShape;

            if (itemShape == null) {
                itemShape = new ItemShape();
            }

            if (itemShape.Size.x < 1 && itemShape.Size.y < 1) {
                itemShape.SetSize(new Vector2Int(1, 1));
            } else if (itemShape.Size.x < 1) {
                itemShape.SetSize(new Vector2Int(1, itemShape.Size.y));
            } else if (itemShape.Size.y < 1) {
                itemShape.SetSize(new Vector2Int(itemShape.Size.x, 1));
            }

            var container = new VisualElement();
            var booleanGrid = new BooleanGridField();
            booleanGrid.SetShape(itemShape);
            booleanGrid.SetEnabled(itemShape.UseCustomShape);

            // Show non-editable version.
            if (field == null) {
                container.Add(booleanGrid);
                return container;
            }

            // Vector2 Size
            var addRemoveColumns = new AddRemoveField("Columns");
            addRemoveColumns.OnAddRemove += (add) =>
            {
                var newCols = itemShape.Cols + (add ? 1 : -1);
                itemShape.SetSize(itemShape.Rows, newCols);
                itemShape.SetSize(new Vector2Int(itemShape.Cols, itemShape.Rows));
                booleanGrid.SetShape(itemShape);

                onChangeEvent(itemShape);
            };
            container.Add(addRemoveColumns);

            var addRemoveRows = new AddRemoveField("Rows");
            addRemoveRows.OnAddRemove += (add) =>
            {
                var newRows = itemShape.Rows + (add ? 1 : -1);
                itemShape.SetSize(newRows, itemShape.Cols);
                booleanGrid.SetShape(itemShape);

                onChangeEvent(itemShape);
            };
            container.Add(addRemoveRows);

            // Bool Use Custom Shape
            var toggle = new Toggle("Use a custom shape?");
            toggle.value = itemShape.UseCustomShape;
            container.Add(toggle);

            System.Action<object> onBoolBindingUpdateEvent = (object newValue) => toggle.SetValueWithoutNotify((newValue as ItemShape).UseCustomShape);
            toggle.RegisterCallback<AttachToPanelEvent>(c =>
            {
                BindingUpdater.AddBinding(field, arrayIndex, target, onBoolBindingUpdateEvent);
            });
            toggle.RegisterCallback<DetachFromPanelEvent>(c =>
            {
                BindingUpdater.RemoveBinding(onBoolBindingUpdateEvent);
            });

            toggle.RegisterValueChangedCallback(c =>
            {
                itemShape.SetUseCustomShape(c.newValue);
                booleanGrid.SetShape(itemShape);

                booleanGrid.SetEnabled(itemShape.UseCustomShape);

                if (!onChangeEvent(itemShape)) {
                    toggle.SetValueWithoutNotify(c.previousValue);
                }
                c.StopPropagation();
            });

            // Bool Array Custom Shape
            container.Add(new Label("Use right-click to set Anchor"));

            booleanGrid.OnToggleChange += (index, newValue) =>
            {
                itemShape.m_CustomShape[index] = newValue;
                onChangeEvent(itemShape);
            };

            container.Add(booleanGrid);

            return container;
        }

        public class BooleanGridField : VisualElement
        {
            public event Action<int, bool> OnToggleChange;

            protected List<Toggle> m_Toggles;
            protected ItemShape m_ItemShape;

            public BooleanGridField()
            {
                m_Toggles = new List<Toggle>();
            }

            public void SetShape(ItemShape itemShape)
            {
                Clear();

                m_ItemShape = itemShape;

                m_Toggles.EnsureSize<Toggle>(m_ItemShape.Size.x * m_ItemShape.Size.y + 1);
                for (int i = 0; i < m_Toggles.Count; i++) {
                    if (m_Toggles[i] == null) {
                        m_Toggles[i] = new Toggle();

                        m_Toggles[i].AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

                        var localI = i;
                        m_Toggles[i].RegisterValueChangedCallback(evt =>
                        {
                            if (evt.newValue == false && localI == itemShape.AnchorIndex) {
                                m_Toggles[localI].SetValueWithoutNotify(true);
                                return;
                            }

                            OnToggleChange?.Invoke(localI, evt.newValue);
                        });
                    }
                }

                for (int y = 0; y < m_ItemShape.Size.y; y++) {
                    var row = new VisualElement();
                    row.AddToClassList("horizontal-layout");
                    Add(row);

                    for (int x = 0; x < m_ItemShape.Size.x; x++) {
                        var toggle = GetToggle(x, y);
                        toggle.value = m_ItemShape.IsIndexOccupied(x, y);

                        if (x == m_ItemShape.Anchor.x && y == m_ItemShape.Anchor.y) {
                            toggle.AddToClassList(ControlTypeStyles.s_BooleanGridAnchor);
                        } else {
                            toggle.RemoveFromClassList(ControlTypeStyles.s_BooleanGridAnchor);
                        }

                        row.Add(toggle);
                    }
                }
            }

            public int GetToggleIndex(int x, int y)
            {
                return y * m_ItemShape.Size.x + x;
            }

            public int GetToggleIndex(Vector2Int v)
            {
                return v.y * m_ItemShape.Size.x + v.x;
            }

            public Toggle GetToggle(int x, int y)
            {
                return m_Toggles[GetToggleIndex(x, y)];
            }

            public Toggle GetToggle(Vector2Int v)
            {
                return m_Toggles[GetToggleIndex(v)];
            }

            /// <summary>
            /// Build the contextual menu when right-clicking a definition.
            /// </summary>
            /// <param name="evt">The event context.</param>
            void BuildContextualMenu(ContextualMenuPopulateEvent evt)
            {
                evt.menu.AppendAction("Set Anchor", SetAnchor, DropdownMenuAction.AlwaysEnabled, evt.target);
            }

            /// <summary>
            /// Duplicate the item definition using the right click.
            /// </summary>
            /// <param name="action">The drop down menu action.</param>
            void SetAnchor(DropdownMenuAction action)
            {
                var toggle = action.userData as Toggle;
                var index = m_Toggles.FindIndex(t => t == toggle);

                var newAnchor = new Vector2Int(index % m_ItemShape.Cols, index / m_ItemShape.Cols);

                var previousAnchor = m_ItemShape.Anchor;

                m_ItemShape.SetAnchor(newAnchor);

                if (previousAnchor != m_ItemShape.Anchor) {
                    GetToggle(previousAnchor).RemoveFromClassList(ControlTypeStyles.s_BooleanGridAnchor);
                    GetToggle(m_ItemShape.Anchor).AddToClassList(ControlTypeStyles.s_BooleanGridAnchor);
                }

                OnToggleChange?.Invoke(index, toggle.value);
            }
        }
    }

    public class AddRemoveField : VisualElement
    {
        public event Action<bool> OnAddRemove;

        protected Label m_Label;
        protected Button m_Add;
        protected Button m_Remove;

        public Label Label => m_Label;

        public AddRemoveField(string text)
        {
            AddToClassList(ControlTypeStyles.s_AddRemoveField);

            m_Label = new Label(text);
            Add(m_Label);

            m_Remove = new Button();
            m_Remove.AddToClassList(ControlTypeStyles.s_AddRemoveFieldButton);
            m_Remove.text = "-";
            m_Remove.clicked += () => OnAddRemove?.Invoke(false);
            Add(m_Remove);

            m_Add = new Button();
            m_Add.AddToClassList(ControlTypeStyles.s_AddRemoveFieldButton);
            m_Add.text = "+";
            m_Add.clicked += () => OnAddRemove?.Invoke(true);
            Add(m_Add);
        }
    }
}