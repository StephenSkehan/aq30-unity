using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class WK3_2_VariantA_PlayProbe
{
    static string ReadText(Transform t)
    {
        if (!t) return null;
        var tmp = t.GetComponentsInChildren<Component>(true).FirstOrDefault(c => c && c.GetType().Name=="TextMeshProUGUI");
        if (tmp != null) return (string)tmp.GetType().GetProperty("text")?.GetValue(tmp);
        var ugui = t.GetComponentInChildren<Text>(true);
        return ugui ? ugui.text : null;
    }

    Transform FindDeep(Transform p, string n)
    {
        if (!p) return null;
        foreach (var t in p.GetComponentsInChildren<Transform>(true)) if (t.name == n) return t;
        return null;
    }

    [UnityTest]
    public IEnumerator VariantA_Shows_On_ResolutionOverlay()
    {
        // Load the demo scene you use for WK flows – adjust name if needed
        yield return SceneManager.LoadSceneAsync("WK2_BoardDemo", LoadSceneMode.Single);

        // 1) Re-apply Variant A via your existing menu static (if present) so we know which content is active
        var t = System.Type.GetType("AQ.EditorTools.Content.ContentVariant");
        var m = t?.GetMethod("ApplyVariantA_Menu", System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic);
        m?.Invoke(null, null);

        // 2) Find the scrub diamond button and click it to advance to the resolution overlay
        var miniRoot = GameObject.Find("Minigame_Scrub");
        Assert.IsNotNull(miniRoot, "Minigame_Scrub not found.");
        var diamond = FindDeep(miniRoot.transform, "Button");
        Assert.IsNotNull(diamond, "Scrub Button not found.");
        var btn = diamond.GetComponent<Button>();
        Assert.IsNotNull(btn, "Scrub Button component not found.");

        btn.onClick.Invoke(); // simulate tap
        yield return null;    // let CaseFlow advance a frame
        yield return new WaitForSeconds(0.1f);

        // 3) Read the overlay labels now that they're bound at runtime
        var root = GameObject.Find("ResolutionRoot");
        Assert.IsNotNull(root, "ResolutionRoot missing at runtime.");
        var panel = FindDeep(root.transform, "ResolutionPanel");
        Assert.IsNotNull(panel, "ResolutionPanel missing.");

        var title = ReadText(FindDeep(panel, "TitleText"));
        var body  = ReadText(FindDeep(panel, "BodyText"));
        var q0    = ReadText(FindDeep(panel, "Quest_0"));
        var q1    = ReadText(FindDeep(panel, "Quest_1"));
        var q2    = ReadText(FindDeep(panel, "Quest_2"));
        var rbtn  = FindDeep(panel, "ResolveButton");
        var btxt  = ReadText(rbtn) ?? ReadText(FindDeep(rbtn, "Text"));

        // 4) Snapshot for audit
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "_audit", "wk3_variants_runtime");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "variantA_runtime_snapshot.txt");
        File.WriteAllLines(path, new[]{
            "=== WK3-2 Variant A Runtime Snapshot ===",
            $"Title: {title}",
            $"Body : {body}",
            $"Quest_0: {q0}",
            $"Quest_1: {q1}",
            $"Quest_2: {q2}",
            $"Button : {btxt}",
        });
        Debug.Log($"WK3-2 (runtime) snapshot: {path}");

        // 5) Reasonable assertions: not placeholders for Title/Body, at least one quest present
        Assert.IsFalse(string.IsNullOrWhiteSpace(title) || title.Trim()=="TitleText", "Title not bound.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(body)  || body.Trim()=="BodyText",  "Body not bound.");
        Assert.IsTrue(new[]{q0,q1,q2}.Any(s => !string.IsNullOrWhiteSpace(s)), "No quest lines shown.");
    }
}
