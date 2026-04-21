using UnityEngine;

namespace AQ.App.Items
{
    /// <summary>
    /// Scene-level singleton catalog of all ItemDefinitionSOs.
    /// Assign every item definition in the Inspector. Looked up by the
    /// MergeEventsBridge to resolve (family, tier) → itemId for lead matching.
    /// </summary>
    public sealed class ItemRegistry : MonoBehaviour
    {
        [SerializeField] private ItemDefinitionSO[] _items = System.Array.Empty<ItemDefinitionSO>();

        public static ItemRegistry Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Find by board identity (family + tier). Returns null if not registered.</summary>
        public ItemDefinitionSO Find(string family, int tier)
        {
            if (string.IsNullOrEmpty(family)) return null;
            foreach (var item in _items)
            {
                if (item != null && item.family == family && item.tier == tier)
                    return item;
            }
            return null;
        }

        /// <summary>Find by stable itemId string. Returns null if not registered.</summary>
        public ItemDefinitionSO FindById(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            foreach (var item in _items)
            {
                if (item != null && item.itemId == itemId)
                    return item;
            }
            return null;
        }
    }
}
