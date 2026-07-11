using System.Linq;
using UnityEditor;
using UnityEngine;
using AQ.App.UI;

namespace AQ.EditorTools
{
    public static class FlightFXSetup
    {
        [MenuItem("AQ/Setup/Flight FX (sprites + premium HUD label)")]
        public static void Setup()
        {
            CopySprite("Assets/Art/Icons/Currency/cash/currency_cash_usd_t02_notes_double.png",
                       "Assets/Resources/App/UI/Icons/flight_cash.png");
            CopySprite("Assets/Art/Icons/Currency/platinum/currency_platinum_t01_ingot_single.png",
                       "Assets/Resources/App/UI/Icons/flight_ingot.png");
            CreatePremiumLabel();
            AssetDatabase.SaveAssets();
            Debug.Log("[FlightFX] setup complete.");
        }

        static void CopySprite(string src, string dst)
        {
            if (AssetDatabase.LoadAssetAtPath<Sprite>(dst) != null) return;
            if (!AssetDatabase.CopyAsset(src, dst))
                Debug.LogError($"[FlightFX] copy failed: {src} -> {dst}");
        }

        /// <summary>
        /// The ingot capsule in the HUD art has no value label (why it always
        /// looked blank). Clone the cash label, mirror its offset from the
        /// energy label, swap the binder for PremiumHudTMP.
        /// </summary>
        static void CreatePremiumLabel()
        {
            if (GameObject.Find("Txt_Premium") != null) { Debug.Log("[FlightFX] Txt_Premium exists"); return; }

            var cash   = GameObject.Find("Txt_Soft_Currency");
            var energy = GameObject.Find("Txt_Value");
            if (cash == null || energy == null) { Debug.LogError("[FlightFX] HUD labels not found"); return; }

            var clone = Object.Instantiate(cash, cash.transform.parent);
            clone.name = "Txt_Premium";
            Undo.RegisterCreatedObjectUndo(clone, "Premium HUD label");

            // capsules are evenly spaced: ingot = cash + (cash - energy)
            var dx = cash.transform.position.x - energy.transform.position.x;
            clone.transform.position = cash.transform.position + new Vector3(dx, 0f, 0f);

            // swap SoftCurrencyHudTMP -> PremiumHudTMP (both live in
            // Assembly-CSharp, which this assembly cannot reference directly)
            foreach (var mb in clone.GetComponents<MonoBehaviour>().ToArray())
                if (mb != null && mb.GetType().Name == "SoftCurrencyHudTMP")
                    Object.DestroyImmediate(mb);

            var premiumType = System.AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("AQ.App.UI.HUD.PremiumHudTMP"))
                .FirstOrDefault(t => t != null);
            if (premiumType != null) clone.AddComponent(premiumType);
            else Debug.LogError("[FlightFX] PremiumHudTMP type not found");

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("[FlightFX] Txt_Premium created.");
        }

        [MenuItem("AQ/Dev/Test Flight FX")]
        public static void TestFlight()
        {
            if (!Application.isPlaying) { Debug.LogWarning("[FlightFX] enter play mode first."); return; }
            FlightFX.FlyReward("soft", 100);
            FlightFX.FlyReward("energy", 40);
            FlightFX.FlyReward("premium", 60);
        }
    }
}
