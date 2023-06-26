/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using System;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// A warning pop up.
    /// </summary>
    public class WarningPopupWindow : PopupWindow
    {
        private static WarningPopupWindow s_Instance;

        /// <summary>
        /// A instance of the warning pop up.
        /// </summary>
        private static WarningPopupWindow Instance {
            get {
                if (s_Instance == null) {
                    s_Instance = CreateInstance<WarningPopupWindow>();
                    s_Instance.hideFlags = HideFlags.HideAndDontSave;
                }
                return s_Instance;
            }
        }

        /// <summary>
        /// Open the warning pop up.
        /// </summary>
        /// <param name="rect">The position.</param>
        /// <param name="size">The size.</param>
        /// <param name="message">The warning message.</param>
        /// <param name="onCancel">The cancel action.</param>
        /// <param name="onSubmit">The submit action.</param>
        /// <returns>The warning pop up window.</returns>
        public static WarningPopupWindow OpenWindow(Rect rect, Vector2 size, string message, Action onCancel, Action onSubmit)
        {
            var window = Instance;

            var content = new VisualElement();
            content.AddToClassList(InventoryManagerStyles.WarningPopupWindow);

            var messageView = new Label(message);
            messageView.AddToClassList(InventoryManagerStyles.WarningPopupWindow_Message);
            content.Add(messageView);

            var buttonContainer = new VisualElement();
            buttonContainer.AddToClassList(InventoryManagerStyles.WarningPopupWindow_ButtonContainer);
            var cancelButton = new Button();
            cancelButton.text = "Cancel";
            cancelButton.clickable.clicked += () =>
            {
                onCancel?.Invoke();
                window.Close();
            };
            buttonContainer.Add(cancelButton);

            var submitButton = new Button();
            submitButton.text = "Yes";
            submitButton.clickable.clicked += () =>
            {
                onSubmit?.Invoke();
                window.Close();
            };
            buttonContainer.Add(submitButton);
            content.Add(buttonContainer);

            window.m_Content = content;
            window.ShowAsDropDown(rect, size);
            window.position = rect;

            size = new Vector2(size.x, window.Content.resolvedStyle.height);
            rect.size = size;
            window.ShowAsDropDown(rect, size);

            window.Refresh();
            return window;
        }
    }
}