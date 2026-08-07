#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools
{
    /// <summary>QA drivers for Mo's Back Room (the CaseCash shop).</summary>
    public static class QAMoShop
    {
        [MenuItem("AQ/Dev/QA Open Mo Shop (Play Mode)")]
        public static void Open()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[QAMoShop] Enter Play Mode first.");
                return;
            }
            if (!AQ.App.UI.Shop.MoShopService.Unlocked)
                Debug.LogWarning("[QAMoShop] Shop locked (needs aq.lead.e1_pod1.seen + MoShop flag) — popup will refuse.");
            AQ.App.UI.Shop.MoShopPopup.Show();
        }

        [MenuItem("AQ/Dev/QA Mo Shop Unlock Flag")]
        public static void Unlock()
        {
            AQ.App.DialogueFlags.Set("aq.lead.e1_pod1.seen");
            Debug.Log("[QAMoShop] e1_pod1 seen-flag set — shop unlock condition met.");
        }

        [MenuItem("AQ/Dev/QA Mo Shop Restock")]
        public static void Restock()
        {
            AQ.App.UI.Shop.MoShopService.ResetStock();
            Debug.Log("[QAMoShop] Stock cleared — next open regenerates today's offers.");
        }

        [MenuItem("AQ/Dev/QA Grant Specials (one of each)")]
        public static void GrantSpecials()
        {
            foreach (AQ.App.UI.Specials.SpecialId id in
                     System.Enum.GetValues(typeof(AQ.App.UI.Specials.SpecialId)))
                AQ.App.UI.Specials.SpecialItemsService.Grant(id);
            AQ.App.UI.Specials.SpecialItemsService.GrantCassette("App/Audio/Cassettes/cassette_dot_goodnight");
            Debug.Log("[QAMoShop] One of each special + the first cassette granted.");
        }
    }
}
#endif
