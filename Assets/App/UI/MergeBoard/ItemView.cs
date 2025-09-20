using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// Minimal item view with a label and a selectable state.
    /// No TMPro/Text dependency to keep tests prefab-free.
    /// </summary>
    public class ItemView : MonoBehaviour
    {
        private string _labelText = string.Empty;
        private bool _selected;

        public string LabelText => _labelText;

        public void SetLabel(string label)
        {
            _labelText = label ?? string.Empty;
            // Hook: update visual text component here if you add one later.
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            // Hook: visual highlight toggle here if/when needed.
        }
    }
}