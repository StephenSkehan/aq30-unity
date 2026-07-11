using UnityEditor;
using UnityEngine;
using AQ.App;

namespace AQ.EditorTools
{
    /// <summary>
    /// Boots a single dialogue node using a newly imported portrait sprite so
    /// the panel rendering can be verified without playing to the lead that
    /// uses it. Edit the two consts when checking a different character.
    /// </summary>
    public static class TestPortraitDialogue
    {
        const string SpritePath = "Assets/Art/Characters/Del/char_del_neutral_f01.png";
        const string Speaker    = "Del Cruz";

        [MenuItem("AQ/Dev/Test Portrait Dialogue (Play Mode)")]
        public static void Run()
        {
            if (!Application.isPlaying) { Debug.LogWarning("[PortraitTest] Enter Play Mode first."); return; }

            var runner = Object.FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (runner == null || sprite == null)
            {
                Debug.LogError($"[PortraitTest] runner={runner != null} sprite={sprite != null} ({SpritePath})");
                return;
            }

            var g = ScriptableObject.CreateInstance<CaseGraph>();
            g.startId = "P1";
            g.nodes = new[]
            {
                new CaseGraph.Node
                {
                    id = "P1", speaker = Speaker,
                    line = "Portrait render test — verify the face, crop and name plate, then tap to close.",
                    portrait = sprite
                }
            };

            if (runner.Panel != null) runner.Panel.gameObject.SetActive(true);
            runner.BootWithGraph(g);
            Debug.Log("[PortraitTest] booted with " + SpritePath);
        }
    }
}
