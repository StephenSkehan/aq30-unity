using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using AQ.App.UI;

namespace AQ.EditorTools
{
    /// <summary>
    /// Top bar touch-ups: the player portrait becomes Ally (replacing the
    /// placeholder photo) and the Settings button gets the theme treatment
    /// (interim — swaps to the generated gear icon when the art kit lands).
    /// </summary>
    public static class RestyleHudNoir
    {
        [MenuItem("AQ/Setup/Restyle HUD (Noir)")]
        public static void Restyle()
        {
            int changes = 0;

            var player = GameObject.Find("Img_Player");
            var ally = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Characters/Ally/char_ally_neutral_f01.png");
            if (player != null && ally != null)
            {
                var img = player.GetComponent<Image>();
                if (img != null)
                {
                    Undo.RecordObject(img, "HUD restyle");
                    img.sprite = ally;
                    img.preserveAspect = true;
                    EditorUtility.SetDirty(img);
                    changes++;
                }
            }
            else Debug.LogWarning($"[HUD] player image or Ally sprite missing (player={player != null}, ally={ally != null})");

            var settings = GameObject.Find("But_Settings");
            var rounded  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/App/UI/aq_rounded.png");
            if (settings != null && rounded != null)
            {
                var img = settings.GetComponent<Image>();
                if (img != null)
                {
                    Undo.RecordObject(img, "HUD restyle");
                    img.sprite = rounded;
                    img.type   = Image.Type.Sliced;
                    img.pixelsPerUnitMultiplier = 2f;
                    img.color  = AQTheme.Panel;
                    EditorUtility.SetDirty(img);
                    changes++;
                }
                var label = settings.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    var display = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/App/UI/Fonts/Staatliches SDF.asset");
                    Undo.RecordObject(label, "HUD restyle");
                    if (display != null) label.font = display;
                    label.text     = "MENU";
                    label.fontSize = 22f;
                    label.color    = AQTheme.Paper;
                    label.alignment = TextAlignmentOptions.Center;
                    EditorUtility.SetDirty(label);
                    changes++;
                }
            }

            if (changes > 0)
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log($"[HUD] restyle applied ({changes} changes).");
        }
    }
}
