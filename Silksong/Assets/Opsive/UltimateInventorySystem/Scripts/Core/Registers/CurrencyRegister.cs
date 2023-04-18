/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.Registers
{
    using Opsive.UltimateInventorySystem.Exchange;

    /// <summary>
    /// The register for currencies.
    /// </summary>
    public class CurrencyRegister : InventoryObjectRegister<Currency>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="register">The register.</param>
        public CurrencyRegister(InventorySystemRegister register) : base(register)
        {
        }

        /// <summary>
        /// Delete an Currencies.
        /// </summary>
        /// <param name="currency">The currency.</param>
        public override void Delete(Currency currency)
        {
            while (currency.GetChildrenCount() != 0) {
                var child = currency.GetChildrenAt(0);
                child.SetParent(null);
                child.SetParent(currency.Parent);
            }

            currency.SetParent(null);

            Unregister(currency);
        }
    }
}