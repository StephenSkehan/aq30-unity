#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace AQ.Editor.UI
{
    public static class LeadCardStyleFixer
    {
        private const string PrefabPath = "Assets/UI/Prefabs/LeadCard.prefab";

        [MenuItem("AQ/UI/Lead Cards/Apply Readable Text Colors + Actor Alpha")]
        public static void Apply()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            if (!root) { Debug.LogError($"[LeadCardStyleFixer] Missing {PrefabPath}"); return; }

            var title    = FindTMP(root, "Text_Title", "Title");
            var objective= FindTMP(root, "Text_Objective", "Objective", "Body");
            var id       = FindTMP(root, "Text_LeadId", "LeadId", "ID");
            var actorImg = FindImage(root, "ActorAnchor");

            if (title)     title.color     = new Color32(28, 32, 38, 255);  // near-black
            if (objective) objective.color = new Color32(60, 67, 75, 255);  // dark gray
            if (id)        id.color        = new Color32(110,122,135,255);  // mid gray

            if (actorImg)  actorImg.color  = new Color(actorImg.color.r, actorImg.color.g, actorImg.color.b, 1f);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            Debug.Log("[LeadCardStyleFixer] Colors updated and actor alpha set to 1.");
        }

        private static TMP_Text FindTMP(GameObject root, params string[] keys)
        {
            var all = root.GetComponentsInChildren<TMP_Text>(true);
            return all.FirstOrDefault(t => keys.Any(k => t.name.ToLowerInvariant().Contains(k.ToLowerInvariant())));
        }
        private static Image FindImage(GameObject root, params string[] keys)
        {
            var all = root.GetComponentsInChildren<Image>(true);
            return all.FirstOrDefault(i => keys.Any(k => i.name.ToLowerInvariant().Contains(k.ToLowerInvariant())));
        }
    }
}
#endif
