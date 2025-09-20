// Assembly: AQ.App
// Use this ONLY in new code during WK3.
// Existing Object.Destroy(...) call sites will be migrated in one hardening pass.

using UnityEngine;

namespace AQ.App
{
    public static class SafeUnity
    {
        public static void Destroy(Object obj)
        {
            if (obj == null) return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Claude’s guard: handle edit-mode + scene/prefab contexts consistently
                if (obj is GameObject go)
                {
                    // If it's a scene object or prefab instance, DestroyImmediate is correct in edit mode.
                    // (We avoid touching AssetDatabase blocks during sprint; hardening pass will review call-sites.)
                    Object.DestroyImmediate(go);
                }
                else
                {
                    Object.DestroyImmediate(obj);
                }
                return;
            }
#endif
            Object.Destroy(obj);
        }

        public static void Destroy(GameObject go) => Destroy((Object)go);
        public static void Destroy(Component c)   => Destroy(c ? c.gameObject : null);
    }
}
