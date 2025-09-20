using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class DialogueSmokeTests_Modern
{
    // Keep both expected Resources paths so tests don't depend on one layout.
    static readonly string[] CandidatePaths = new[] {
        "App/UI/Narrative/Prefabs/DialoguePanel",
        "App/UI/Prefabs/DialoguePanel",
    };

    static GameObject LoadPanel(){
        foreach (var p in CandidatePaths){
            var go = Resources.Load<GameObject>(p);
            if (go != null) return go;
        }
        return null;
    }

    [UnityTest]
    public IEnumerator DialoguePanel_prefab_can_be_resolved_and_instantiated(){
        var prefab = LoadPanel();
        Assert.IsNotNull(prefab, "DialoguePanel prefab not found in any expected Resources path.");

        var root = new GameObject("TEST_ROOT");
        GameObject instance = null;
        try {
            instance = Object.Instantiate(prefab, root.transform);
            // allow any Awake/Start to run (and catch missing deps quickly)
            yield return null;
            // basic UI sanity: expect a RectTransform somewhere
            Assert.IsNotNull(instance.GetComponentInChildren<RectTransform>(true),
                "DialoguePanel instance should include a RectTransform.");
        }
        finally {
            if (root != null) Object.DestroyImmediate(root);
        }
    }

    [UnityTest]
    public IEnumerator DialoguePanel_has_basic_UI_shape(){
        var prefab = LoadPanel();
        Assert.IsNotNull(prefab, "DialoguePanel prefab missing.");

        // Check for any obvious text UI component on the prefab definition
        var hasUGUIText = prefab.GetComponentInChildren<UnityEngine.UI.Text>(true) != null;
        var hasTMPText = false;
#if TMP_PRESENT
        hasTMPText = prefab.GetComponentInChildren<TMPro.TMP_Text>(true) != null;
#endif
        Assert.IsTrue(hasUGUIText || hasTMPText, "Expected some UI text element on DialoguePanel.");
        yield return null;
    }
}

