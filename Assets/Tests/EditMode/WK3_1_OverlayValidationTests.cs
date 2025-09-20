using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class WK3_1_OverlayValidationTests
{
    static bool HasTMP(Component c) => c && c.GetType().Name == "TextMeshProUGUI";

    [Test]
    public void Overlay_Present_And_Configured()
    {
        var root  = GameObject.Find("ResolutionRoot");
        Assert.IsNotNull(root, "ResolutionRoot missing.");

        var panel = root.transform.Find("ResolutionPanel");
        Assert.IsNotNull(panel, "ResolutionPanel missing.");

        var title = panel.Find("TitleText");
        var body  = panel.Find("BodyText");
        var btnT  = panel.Find("ResolveButton");

        Assert.IsNotNull(title, "TitleText missing.");
        Assert.IsNotNull(body,  "BodyText missing.");
        Assert.IsNotNull(btnT,  "ResolveButton missing.");

        Assert.IsTrue(HasTMP(title.GetComponent<Component>()), "TitleText must use TMP (TextMeshProUGUI).");
        Assert.IsTrue(HasTMP(body .GetComponent<Component>()), "BodyText must use TMP (TextMeshProUGUI).");
        Assert.IsTrue(System.Linq.Enumerable.Any(btnT.GetComponentsInChildren<Component>(true), HasTMP),
                      "ResolveButton label must use TMP (TextMeshProUGUI).");

        var button = btnT.GetComponent<Button>();
        Assert.IsNotNull(button, "ResolveButton must have Button.");

        bool wired = false;
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentTarget(i) != null &&
                button.onClick.GetPersistentMethodName(i) == "OnResolve")
            { wired = true; break; }
        }
        Assert.IsTrue(wired, "ResolveButton must call OnResolve().");

        Assert.Less(btnT.position.y, body.position.y, "ResolveButton should be below BodyText.");
    }
}
