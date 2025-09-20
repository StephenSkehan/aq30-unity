using UnityEngine;
using UnityEngine.SceneManagement;

/// Tiny scene jump helper for development.
public class QuickNavButton : MonoBehaviour
{
    [Tooltip("Exact scene name to load (must be in Build Settings).")]
    public string sceneName = "Case_Board_Portrait";

    [Tooltip("If true, loads additively; otherwise loads Single (replaces current scene).")]
    public bool additive = false;

#if UNITY_EDITOR
    // Optional: drag a SceneAsset in the Inspector (editor-only) to avoid typos.
    [SerializeField] private UnityEditor.SceneAsset sceneAsset;
    private void OnValidate()
    {
        if (sceneAsset != null) sceneName = sceneAsset.name;
    }
#endif

    public void GoNow()
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[QuickNavButton] Scene name is empty.");
            return;
        }

        var mode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
        SceneManager.LoadScene(sceneName, mode);
    }
}
