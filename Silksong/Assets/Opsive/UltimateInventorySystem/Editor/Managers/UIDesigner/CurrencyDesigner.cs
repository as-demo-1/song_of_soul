/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.UltimateInventorySystem.UI.Currency;
    using Opsive.UltimateInventorySystem.UI.Monitors;
    using System;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    public enum CurrencyCreatorOptions
    {
        MultiCurrencyView,
        MultiCurrencyViewSmall,
        CurrencyOwnerMonitor,
        CurrencyView
    }

    public class CurrencyDesigner : UIDesignerCreateEditTabContent<
        MultiCurrencyView,
        CurrencyDesignerCreator,
        CurrencyDesignerEditor>
    {
        public override string Title => "Currency";
        public override string Description => "Create and edit a currency UIs.";
    }

    public class CurrencyDesignerCreator : UIDesignerCreator<MultiCurrencyView>
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/currency/";

        private EnumField m_CreatorOptions;
        protected Label m_CreatorOptionDescription;

        protected override void CreateOptionsContent(VisualElement container)
        {
            m_CreatorOptions = new EnumField(CurrencyCreatorOptions.MultiCurrencyView);
            m_CreatorOptions.RegisterValueChangedCallback(evt => { SelectedOption((CurrencyCreatorOptions)evt.newValue); });
            container.Add(m_CreatorOptions);

            m_CreatorOptionDescription = new Label("Nothing selected.");

            SelectedOption((CurrencyCreatorOptions)m_CreatorOptions.value);
        }

        private void SelectedOption(CurrencyCreatorOptions creatorOption)
        {
            switch (creatorOption) {
                case CurrencyCreatorOptions.MultiCurrencyView:
                    m_CreatorOptionDescription.text =
                        "The standard Multi Currency View is used to show an amount of multiple currencies.";
                    break;
                case CurrencyCreatorOptions.MultiCurrencyViewSmall:
                    m_CreatorOptionDescription.text =
                        "The small Multi Currency View is used to show an amount of multiple currencies.";
                    break;
                case CurrencyCreatorOptions.CurrencyOwnerMonitor:
                    m_CreatorOptionDescription.text =
                        "The Currency Owner Monitor listens to a Currency Owner to update the Multi Currency View when updated.";
                    break;
                case CurrencyCreatorOptions.CurrencyView:
                    m_CreatorOptionDescription.text =
                        "The standard Currency View is used to display a single Currency Amount.";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override MultiCurrencyView BuildInternal()
        {
            var rectParent = m_ParentTransform.value as RectTransform;
            var creatorOption = (CurrencyCreatorOptions)m_CreatorOptions.value;

            return CreateMultiCurrencyView(rectParent, creatorOption);
        }

        public MultiCurrencyView CreateMultiCurrencyView(RectTransform rectParent, CurrencyCreatorOptions creatorOption)
        {
            MultiCurrencyView multiCurrencyView = null;

            switch (creatorOption) {
                case CurrencyCreatorOptions.MultiCurrencyView:
                    multiCurrencyView =
                        UIDesignerManager.InstantiateSchemaPrefab<MultiCurrencyView>(UIDesignerSchema.MultiCurrencyView,
                            rectParent);
                    break;
                case CurrencyCreatorOptions.MultiCurrencyViewSmall:
                    multiCurrencyView =
                        UIDesignerManager.InstantiateSchemaPrefab<MultiCurrencyView>(UIDesignerSchema.MultiCurrencyViewSmall,
                            rectParent);
                    break;
                case CurrencyCreatorOptions.CurrencyOwnerMonitor:
                    var currencyMonitor = UIDesignerManager.InstantiateSchemaPrefab<CurrencyOwnerMonitor>(UIDesignerSchema.CurrencyOwnerMonitor,
                        rectParent);
                    multiCurrencyView = currencyMonitor.GetComponent<MultiCurrencyView>();
                    break;
                case CurrencyCreatorOptions.CurrencyView:
                    var currencyView = UIDesignerManager.InstantiateSchemaPrefab<CurrencyView>(UIDesignerSchema.CurrencyView,
                        rectParent);
                    ComponentSelection.Select(currencyView);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return multiCurrencyView;
        }
    }

    public class CurrencyDesignerEditor : UIDesignerEditor<MultiCurrencyView>
    {
        protected override bool RequireDisplayPanel => false;

        protected CurrencyOwnerMonitor m_CurrencyOwnerMonitor;

        protected VisualElement m_CurrencyOwnerMonitorContainer;
        protected CreateSelectDeleteContainer m_CreateSelectDeleteCurrencyMonitor;

        public CurrencyDesignerEditor()
        {
            m_CurrencyOwnerMonitorContainer = new VisualElement();
            m_CurrencyOwnerMonitorContainer.Add(new SubTitleLabel("Currency Owner Monitor"));

            m_CreateSelectDeleteCurrencyMonitor = new CreateSelectDeleteContainer("Currency Monitor",
                AddRemoveCurrencyMonitor,
                AddRemoveCurrencyMonitor,
                () => m_CurrencyOwnerMonitor);
            m_CreateSelectDeleteCurrencyMonitor.Refresh();
            m_CurrencyOwnerMonitorContainer.Add(m_CreateSelectDeleteCurrencyMonitor);
        }

        private void AddRemoveCurrencyMonitor()
        {
            if (m_CurrencyOwnerMonitor == null) {
                var monitor = m_Target.gameObject.AddComponent<CurrencyOwnerMonitor>();
            } else {
                RemoveComponent(m_CurrencyOwnerMonitor);
            }

            Refresh();
        }

        protected override void NewValidTargetAssigned()
        {
            base.NewValidTargetAssigned();

            m_TargetOptionsContainer.Add(m_CurrencyOwnerMonitorContainer);

            m_CurrencyOwnerMonitor = m_Target.GetComponent<CurrencyOwnerMonitor>();

            m_CreateSelectDeleteCurrencyMonitor.Refresh();
        }
    }
}