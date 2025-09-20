using UnityEngine;

namespace AQ.App.UnityAdapters
{
    public static class UnityPersistentPathProvider
    {
        public static string GetBasePath()
        {
            return Application.persistentDataPath;
        }
    }
}
