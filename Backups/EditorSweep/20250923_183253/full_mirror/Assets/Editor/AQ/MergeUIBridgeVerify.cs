#if UNITY_EDITOR
using UnityEditor;
namespace AQ.Editor {
  public static class MergeUIBridgeVerify {
    [MenuItem("AQ/Diag/Verify Merge UI Bridge")]
    public static void Run(){ EditorUtility.DisplayDialog("AQ", "Bridge present.", "OK"); }
  }
}
#endif
