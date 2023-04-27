/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes
{
    using Opsive.Shared.Editor.UIElements.Controls;
    using UnityEngine;

    /// <summary>
    /// Implements TypeControlBase for the GameObject ControlType.
    /// </summary>
    [ControlType(typeof(GameObject))]
    public class GameObjectControl : UnityObjectControlWithPreview
    {
    }
}