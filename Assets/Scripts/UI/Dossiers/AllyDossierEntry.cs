using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Dossiers
{
    /// <summary>
    /// Makes Ally's HUD bust (Img_Player, previously tap-dead) open her own
    /// case file. Runtime-installed so the baked HUD stays untouched — no
    /// scene edit, no rebake (the HUD bake rule). The HUD canvas is scene-
    /// authored with a working GraphicRaycaster, so a plain Button is safe.
    /// </summary>
    public static class AllyDossierEntry
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            var go = GameObject.Find("Img_Player");
            if (go == null || go.GetComponent<Button>() != null) return;

            var img = go.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;
            var portrait = img != null ? img.sprite : null;

            var btn        = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => DossierPopup.Show("ally", portrait));
        }
    }
}
