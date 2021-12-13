using UnityEngine;

namespace Inventory
{
    public class SceneItem : MonoBehaviour
    {
        [SerializeField] private ItemSO _currentItemSO = default;
        [SerializeField] private GameObject _itemGO = default;

        public ItemSO GetItem()
        {

            return _currentItemSO;

        }
        public void SetItem(ItemSO item)
        {
            _currentItemSO = item;

        }
    }
}