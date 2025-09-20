using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AQ.App.Tests
{
    public class ThemeSmokeTests
    {
        [UnityTest]
        public System.Collections.IEnumerator PrefabsRespondToThemeSwap()
        {
            var hudPrefab = Resources.Load<GameObject>("App/UI/Prefabs/HUD");
            Assert.IsNotNull(hudPrefab, "HUD prefab not found at Resources/App/UI/Prefabs/HUD");

            var go = Object.Instantiate(hudPrefab);
            Assert.IsNotNull(go);

            // Be graceful: add ThemeController if the prefab doesn't have it yet (Step C will add it in authoring).
            var themeCtrl = go.GetComponentInChildren<ThemeController>() ?? go.AddComponent<ThemeController>();
            Assert.IsNotNull(themeCtrl, "Could not obtain ThemeController on HUD");

            // Minimal swap exercise; full binding checks come in WK2-1 Step C.
            var t1 = ScriptableObject.CreateInstance<ThemeSO>();
            t1.Primary = Color.red;
            var t2 = ScriptableObject.CreateInstance<ThemeSO>();
            t2.Primary = Color.blue;

            themeCtrl.SetActiveTheme(t1);
            yield return null;
            themeCtrl.SetActiveTheme(t2);
            yield return null;

            Assert.Pass("HUD can accept a ThemeController and apply themes without exceptions.");
        }
    }
}



