using UnityEngine;

namespace Inventory
{
    public class CollectableItem : MonoBehaviour
    {
        [SerializeField] private ItemSO _currentItem = default;
        [SerializeField] private GameObject _itemGO = default;

        public ItemSO GetItem()
        {

            return _currentItem;

        }
        public void SetItem(ItemSO item)
        {
            _currentItem = item;

        }
    }
}