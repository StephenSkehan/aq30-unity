using UnityEngine;

namespace AQ.App.Items
{
    /// <summary>
    /// Canonical definition of a single merge-board item type.
    /// One asset per named item (e.g. "forensic_laptop", "recorder_charged").
    /// Lead requirements reference this SO directly for type-safe, rename-proof wiring.
    /// </summary>
    [CreateAssetMenu(fileName = "Item_", menuName = "AQ/Item Definition", order = 20)]
    public class ItemDefinitionSO : ScriptableObject
    {
        [Tooltip("Stable string ID used to match this item to lead requirements and save data. " +
                 "Must be unique across all items. Never rename after content is authored.")]
        public string itemId;

        [Tooltip("Family key that MergeBoardController stamps on tiles spawned from a generator " +
                 "belonging to this item type (e.g. 'forensic_tools', 'stakeout_fuel').")]
        public string family;

        [Tooltip("0-based tier index — matches board PayloadData.tier (T1 item = tier 0).")]
        [Min(0)] public int tier;

        public Sprite icon;
        public string displayName;
    }
}
