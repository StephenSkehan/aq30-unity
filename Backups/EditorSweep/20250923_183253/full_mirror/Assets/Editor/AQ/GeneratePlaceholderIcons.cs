#if UNITY_EDITOR
using UnityEditor;
namespace AQ.Editor {
  public static class GeneratePlaceholderIcons {
    [MenuItem("AQ/Tools/Generate Placeholder Icons")]
    public static void Run(){ EditorUtility.DisplayDialog("AQ", "Placeholder only.", "OK"); }
  }
}
#endif
