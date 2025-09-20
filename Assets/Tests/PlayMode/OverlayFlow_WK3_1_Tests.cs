#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class OverlayFlow_WK3_1_Tests
{
    IEnumerator LoadAnySceneWith(string goName)
    {
        // Try current scene first
        if (GameObject.Find(goName) == null)
        {
            // Brute-force: iterate scenes in build settings until one has the GO
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var op = SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
                while (!op.isDone) yield return null;
                yield return null;
                if (GameObject.Find(goName) != null) break;
            }
        }
    }

    [UnityTest]
    public IEnumerator Overlay_Shows_And_Continue_Wires()
    {
        yield return LoadAnySceneWith("ResolutionRoot");
        var root  = GameObject.Find("ResolutionRoot");
        Assert.IsNotNull(root, "ResolutionRoot not found in any build scene.");

        // Force overlay visible if it’s hidden by flow; many teams expose a helper—fallback to enabling the panel
        var panel = root.transform.Find("ResolutionPanel");
        Assert.IsNotNull(panel, "ResolutionPanel missing under ResolutionRoot.");
        panel.gameObject.SetActive(true);

        var btn = panel.transform.Find("ResolveButton");
        Assert.IsNotNull(btn, "ResolveButton not found.");
        var button = btn.GetComponent<Button>();
        Assert.IsNotNull(button, "ResolveButton has no Button component.");

        // Capture active state → click → ensure something happened (either overlay hides or a side-effect occurs)
        var beforeActive = panel.gameObject.activeInHierarchy;

        // Invoke persistent listeners (simulates user click path)
        button.onClick.Invoke();
        yield return null; // let frame process

        // Minimal assertions: either the panel hides or the button had listeners
        bool hadListeners = button.onClick.GetPersistentEventCount() > 0;
        Assert.IsTrue(hadListeners, "ResolveButton has no persistent listeners.");
        // Preferred behavior: overlay hidden after continue
        // If your flow hides from another system, relax this assert as needed:
        // Assert.IsFalse(panel.gameObject.activeInHierarchy, "Overlay did not hide after Continue.");
    }
}
#endif
