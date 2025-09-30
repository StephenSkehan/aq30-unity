#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

public static class AuditMenuConflicts
{
    // Project audit: only our menu paths (default "AQ/")
    [MenuItem("AQ/Dev/Audit: MenuItem Conflicts (Project)")]
    public static void RunProjectOnly()
    {
        RunInternal(prefixFilter: "AQ/");
    }

    // Everything: includes Unity's own menus (noisy, but here if you need it)
    [MenuItem("AQ/Dev/Audit: MenuItem Conflicts (All)")]
    public static void RunAll()
    {
        RunInternal(prefixFilter: null);
    }

    static void RunInternal(string prefixFilter)
    {
        var methods = UnityEditor.TypeCache.GetMethodsWithAttribute<MenuItem>();
        // Collect all MenuItem attributes for each method.
        var all = new List<(string path, bool validate, MethodInfo method)>();

        foreach (var m in methods)
        {
            var attrs = m.GetCustomAttributes(typeof(MenuItem), false).Cast<MenuItem>();
            foreach (var attr in attrs)
            {
                if (!string.IsNullOrEmpty(prefixFilter) && !attr.menuItem.StartsWith(prefixFilter))
                    continue;
                all.Add((attr.menuItem, attr.validate, m));
            }
        }

        // Group by path
        var groups = all.GroupBy(x => x.path);

        int issues = 0;

        foreach (var g in groups.OrderBy(g => g.Key))
        {
            var executes  = g.Where(x => x.validate == false).Select(x => x.method).ToList();
            var validates = g.Where(x => x.validate == true ).Select(x => x.method).ToList();

            // "OK" patterns we tolerate:
            //  - 1 execute + 0 or 1 validate
            // Anything else is suspicious (multiple executes or multiple validates).
            bool ok =
                (executes.Count == 1 && (validates.Count == 0 || validates.Count == 1)) ||
                (executes.Count == 0 && validates.Count == 1); // rare, but some menus only validate-enable an existing one

            if (!ok)
            {
                issues++;
                Debug.LogWarning($"[Menu Conflict] \"{g.Key}\"  executes={executes.Count}, validates={validates.Count}\n" +
                                 Dump("Executes", executes) + "\n" +
                                 Dump("Validates", validates));
            }
        }

        if (issues == 0)
        {
            Debug.Log(prefixFilter == null
                ? "[Menu Audit] No actionable conflicts (All menus)."
                : "[Menu Audit] No actionable conflicts under prefix.");
        }
    }

    static string Dump(string label, IEnumerable<MethodInfo> methods)
    {
        var list = methods.ToList();
        if (list.Count == 0) return $"  {label}: —";
        return $"  {label}:\n" + string.Join("\n", list.Select(m => $"    - {m.DeclaringType.FullName}.{m.Name}"));
    }
}
#endif
