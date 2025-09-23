// Assets/Editor/AQ/Variants/OverlayPolish.cs
#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AQ.EditorTools.Variants
{
    /// <summary>
    /// Simple palette & text sizing polish for the Resolution overlay.
    /// Updated to use TMP_Text.textWrappingMode instead of obsolete enableWordWrapping.
    /// </summary>
    public static class OverlayPolish
    {
        [MenuItem("AQ/Variants/Polish/Apply Overlay Palette & Sizes")]
        public static void ApplyPalette()
        {
            int affected = 0;

            var roots = Resources.FindObjectsOfTypeAll<Transform>()
                .Where(t => t && t.name == "ResolutionRoot" && t.gameObject.scene.IsValid() && t.gameObject.scene.isLoaded);

            foreach (var root in roots)
            {
                var go = root.gameObject;

                // Background color (example slightly desaturated dark)
                var bgImg = go.GetComponent<Image>();
                if (bgImg)
                {
                    var c = bgImg.color;
                    c = new Color(0.10f, 0.10f, 0.12f, 0.92f);
                    bgImg.color = c;
                }

                // Title title
                var title = root.Find("ResolutionPanel/TitleText")?.GetComponent<TMP_Text>();
                if (title)
                {
                    title.fontSize = Mathf.Max(title.fontSize, 52f);
                    title.color = new Color(0.95f, 0.95f, 1f, 1f);
#if TMP_PRESENT
#endif
                    title.textWrappingMode = TextWrappingModes.NoWrap; // previously enableWordWrapping = false
                    title.alignment = TextAlignmentOptions.Center;
                }

                // Body
                var body = root.Find("ResolutionPanel/BodyText")?.GetComponent<TMP_Text>();
                if (body)
                {
                    body.fontSize = Mathf.Max(body.fontSize, 32f);
                    body.color = new Color(0.85f, 0.85f, 0.92f, 1f);
                    body.textWrappingMode = TextWrappingModes.Normal; // previously enableWordWrapping = true
                    body.alignment = TextAlignmentOptions.Center;
                }

                // Button text
                var btnText = root.Find("ResolutionPanel/ResolveButton/Text")?.GetComponent<TMP_Text>();
                if (btnText)
                {
                    btnText.fontSize = Mathf.Max(btnText.fontSize, 36f);
                    btnText.color = new Color(0.10f, 0.10f, 0.12f, 1f);
                    btnText.textWrappingMode = TextWrappingModes.NoWrap; // previously enableWordWrapping = false
                    btnText.alignment = TextAlignmentOptions.Center;
                }

                // Optional: Quest bullets if present
                for (int i = 0; i < 3; i++)
                {
                    var q = root.Find($"ResolutionPanel/Quest_{i}")?.GetComponent<TMP_Text>();
                    if (q)
                    {
                        q.fontSize = Mathf.Max(q.fontSize, 28f);
                        q.color = new Color(0.88f, 0.88f, 0.95f, 1f);
                        q.textWrappingMode = TextWrappingModes.Normal; // previously enableWordWrapping = true
                        q.alignment = TextAlignmentOptions.Left;
                    }
                }

                affected++;
                var scene = go.scene;
                if (scene.IsValid() && !Application.isPlaying)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }

            if (affected > 0 && !Application.isPlaying)
                AssetDatabase.SaveAssets();

            Debug.Log($"[Overlay/Polish] Applied palette & sizes to {affected} instance(s).");
        }
    }
}
#endif
