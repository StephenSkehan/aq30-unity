#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    /// <summary>
    /// Assigns sprites from Assets/Art/UI/Leads/Actors (Actor1.png, Actor2.png, ...)
    /// to ActorAnchor/Image on each card under Content_Leads and sets alpha = 1.
    /// Read-only for structure; only touches the Image.sprite and Image.color.
    /// Logs per-card results for clarity.
    /// </summary>
    public static class AssignActorSpritesForDemo
    {
        private const string ActorsFolder = "Assets/Art/UI/Leads/Actors";

        [MenuItem("AQ/UI/Leads/Assign Actor Sprites (Demo)")]
        public static void Run()
        {
            var content = GameObject.Find("Content_Leads")?.transform;
            if (!content)
            {
                Debug.LogWarning("[AQ Actors] Content_Leads not found.");
                return;
            }

            // Load actor sprites
            var guids = AssetDatabase.FindAssets("Actor t:Sprite", new[] { ActorsFolder });
            var sprites = new List<Sprite>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (s != null && s.name.StartsWith("Actor"))
                    sprites.Add(s);
            }
            sprites = sprites.OrderBy(s => s.name).ToList();

            if (sprites.Count == 0)
            {
                Debug.LogWarning($"[AQ Actors] No sprites named 'Actor*' found under {ActorsFolder}.");
                return;
            }
            Debug.Log($"[AQ Actors] Found {sprites.Count} sprites: {string.Join(", ", sprites.Select(s => s.name))}");

            int idx = 0, assigned = 0, skipped = 0;
            foreach (Transform card in content)
            {
                // Be tolerant about structure: look for "ActorAnchor" anywhere under the card
                var anchor = card.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "ActorAnchor");
                if (anchor == null)
                {
                    Debug.LogWarning($"[AQ Actors] Card '{card.name}': no ActorAnchor found.");
                    skipped++;
                    continue;
                }

                // Prefer exact child "Image"; else use first Image under anchor
                Image img = anchor.Find("Image")?.GetComponent<Image>() ?? anchor.GetComponentInChildren<Image>(true);
                if (img == null)
                {
                    Debug.LogWarning($"[AQ Actors] Card '{card.name}': no Image under ActorAnchor.");
                    skipped++;
                    continue;
                }

                var sprite = sprites[idx % sprites.Count];
                img.sprite = sprite;
                var c = img.color; c.a = 1f; img.color = c;
                assigned++; idx++;

                Debug.Log($"[AQ Actors] Card '{card.name}': set sprite='{sprite.name}' α=1.");
            }

            Debug.Log($"[AQ Actors] Assigned {assigned} sprites, skipped {skipped}.");
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
#endif
