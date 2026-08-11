using AQ.App.UI.Dossiers;
using UnityEditor;
using UnityEngine;

namespace AQ.App.UI.Board.Editor
{
    /// <summary>QA seams for the Character Dossiers feature (play mode).</summary>
    public static class QADossiers
    {
        [MenuItem("AQ/QA/Dossiers/Open Ally Case File")]
        public static void OpenAlly() => Open("ally");

        [MenuItem("AQ/QA/Dossiers/Open Dot Case File")]
        public static void OpenDot() => Open("dot");

        [MenuItem("AQ/QA/Dossiers/Open Vera Case File")]
        public static void OpenVera() => Open("vera");

        private static void Open(string key)
        {
            if (!Application.isPlaying) { Debug.LogWarning("[QA] Enter play mode first."); return; }
            DossierPopup.Show(key);
        }

        [MenuItem("AQ/QA/Dossiers/Grant 1000 CC")]
        public static void GrantCC()
        {
            if (!Application.isPlaying) { Debug.LogWarning("[QA] Enter play mode first."); return; }
            var wallet = AQ.App.Economy.WalletLocator.Instance;
            if (wallet == null) { Debug.LogWarning("[QA] No wallet."); return; }
            wallet.Grant("qa.dossier", AQ.SharedKernel.Economy.Reward.Soft(1000));
            Debug.Log("[QA] +1000 CC. Balance: " + wallet.Get(AQ.SharedKernel.Economy.Currency.Soft));
        }

        [MenuItem("AQ/QA/Dossiers/Set Episode Closed Flag")]
        public static void SetCloseFlag()
        {
            AQ.App.DialogueFlags.Set("aq.lead.e1_close.seen");
            Debug.Log("[QA] aq.lead.e1_close.seen set — final facts unsealed.");
        }

        [MenuItem("AQ/QA/Dossiers/Reset Dossiers")]
        public static void Reset()
        {
            DossierService.ResetAll();
            Debug.Log("[QA] Dossier state cleared.");
        }
    }
}
