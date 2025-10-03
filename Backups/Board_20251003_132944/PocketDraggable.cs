// Assets/Scripts/UI/Board/PocketDraggable.cs
// TEMP STUB: Pocket feature is disabled for now.
using UnityEngine;
using UnityEngine.EventSystems;

namespace AQ.App.UI.Board
{
    [DisallowMultipleComponent]
    public sealed class PocketDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public void OnBeginDrag(PointerEventData e) { /* no pocket in this sprint */ }
        public void OnDrag(PointerEventData e)      { }
        public void OnEndDrag(PointerEventData e)   { }
    }
}
