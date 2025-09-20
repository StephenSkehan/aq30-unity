// Assets/App/Utils/DestroyUtil.cs
using UnityEngine;

public static class DestroyUtil
{
    public static void SafeDestroy(Object obj)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) Object.DestroyImmediate(obj);
        else Object.Destroy(obj);
#else
        Object.Destroy(obj);
#endif
    }
}
