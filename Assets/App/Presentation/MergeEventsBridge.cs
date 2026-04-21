using UnityEngine;
using AQ.App.Items;
using AQ.App.UI.Board;
using AQ.SharedKernel.Events;

namespace AQ.App.Presentation
{
    /// <summary>
    /// Bridges MergeBoardController's static OnItemCreated event into the GlobalBus
    /// as an ItemCreatedOnBoard domain event. This keeps the board controller ignorant
    /// of the bus and the lead system ignorant of the board controller.
    /// </summary>
    public sealed class MergeEventsBridge : MonoBehaviour
    {
        [SerializeField] private ItemRegistry _registry;

        private void Awake()
        {
            if (!_registry) _registry = FindAnyObjectByType<ItemRegistry>();
        }

        private void OnEnable()
        {
            MergeBoardController.OnItemCreated += HandleItemCreated;
        }

        private void OnDisable()
        {
            MergeBoardController.OnItemCreated -= HandleItemCreated;
        }

        private void HandleItemCreated(string family, int tier)
        {
            var def = _registry != null ? _registry.Find(family, tier) : null;

            // Fall back to a synthetic id so events still flow even before all
            // ItemDefinitionSOs are authored. The checker will simply find no match.
            var itemId = def != null ? def.itemId : string.Empty;

            GlobalBus.Bus.Publish(new ItemCreatedOnBoard(itemId, family, tier));
        }
    }
}
