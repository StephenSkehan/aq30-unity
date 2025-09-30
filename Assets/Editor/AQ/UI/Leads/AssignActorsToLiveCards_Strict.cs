#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    /// <summary>
    /// Assigns Actor* sprites from Assets/Art/UI/Leads/Actors to the live cards under Content_Leads.
    /// Robust: if ActorAnchor has no Image child, it creates one. Only touches the live scene.
    /// </summary>
    public static class AssignActorsToLiveCards_Strict
    {
        private const string ActorsFolder = "Assets/Art/UI/Leads/Actors";

        [MenuItem("AQ/UI/Leads/Assign Live -> Actor Portraits (Strict)")]
        public static void Run()
        {
            var content = GameObject.Find("Content_Leads")?.transform;
            if (!content) { Debug.LogWarning("[AQ ActorsLive] Content_Leads not found."); return; }

            var guids = AssetDatabase.FindAssets("Actor t:Sprite", new[] { ActorsFolder });
            var sprites = guids.Select(g => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g)))
                               .Where(s => s != null).OrderBy(s => s.name).ToArray();
            if (sprites.Length == 0) { Debug.LogWarning("[AQ ActorsLive] No Actor* sprites found."); return; }

            int idx = 0, assigned = 0, createdImages = 0;
            foreach (Transform card in content)
            {
                var anchor = card.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "ActorAnchor");
                if (!anchor) continue;

                // Prefer child named "Image"; if missing, add one.
                var imgTf = anchor.Find("Image");
                if (!imgTf)
                {
                    var go = new GameObject("Image", typeof(RectTransform), typeof(Image));
                    var rt = go.GetComponent<RectTransform>();
                    rt.SetParent(anchor, false);
                    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(96, 96);
                    imgTf = rt;
                    createdImages++;
                }

                var img = imgTf.GetComponent<Image>();
                var sprite = sprites[idx % sprites.Length];
                img.sprite = sprite;
                var c = img.color; c.a = 1f; img.color = c;
                img.enabled = true; img.raycastTarget = false;
                assigned++; idx++;
            }

            Debug.Log($"[AQ ActorsLive] Assigned {assigned} portraits (added Image components: {createdImages}).");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
#endif
