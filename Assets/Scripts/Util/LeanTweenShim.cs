// This file is a minimal shim so the project can compile without LeanTween.
// If you later import LeanTween, simply delete this file.
// We only implement the bits used in your project: cancel(), scale(), and setEaseOutBack().

#if !LEANTWEEN   // If you define LEANTWEEN when the real package is present, this shim won't compile.

using UnityEngine;

public sealed class LTDescr
{
    // Fluent no-op to match LeanTween's chaining style.
    public LTDescr setEaseOutBack() { return this; }
}

public static class LeanTween
{
    /// <summary>
    /// No-op cancel. Kept for compatibility.
    /// </summary>
    public static void cancel(GameObject go) { /* no-op */ }

    /// <summary>
    /// "Tween" a RectTransform scale by immediately setting it (no animation).
    /// Returns an LTDescr so existing code that chains .setEaseOutBack() compiles.
    /// </summary>
    public static LTDescr scale(RectTransform t, Vector3 to, float time)
    {
        if (t != null) t.localScale = to;
        return new LTDescr();
    }

    /// <summary>
    /// Overload that accepts a GameObject (matches common LeanTween usage).
    /// </summary>
    public static LTDescr scale(GameObject go, Vector3 to, float time)
    {
        if (go != null)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt != null) rt.localScale = to;
        }
        return new LTDescr();
    }
}

#endif
