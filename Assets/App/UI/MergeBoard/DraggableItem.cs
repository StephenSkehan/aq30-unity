using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AQ.App
{
    /// <summary>
    /// Minimal draggable item that notifies the MergeInputAdapter when dropped
    /// on another ItemView. Uses a simple RaycastAll to find the drop target.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DraggableItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [Tooltip("The adapter that will be notified when this item is dropped on a target.")]
        public MergeInputAdapter adapter;

        [Tooltip("This component should also live on the same GameObject as your item UI.")]
        public ItemView itemView;

        private Canvas _canvas;
        private RectTransform _rt;
        private CanvasGroup _group;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            _group = GetComponent<CanvasGroup>();
            if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_group != null) _group.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_rt == null) return;
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            if (_canvas == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvas.transform as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var localPos))
            {
                _rt.anchoredPosition = localPos;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_group != null) _group.blocksRaycasts = true;

            if (adapter == null || itemView == null || EventSystem.current == null)
                return;

            // Raycast to find a potential ItemView under the pointer
            var results = new List<RaycastResult>(16);
            EventSystem.current.RaycastAll(eventData, results);

            ItemView target = null;
            for (int i = 0; i < results.Count; i++)
            {
                var iv = results[i].gameObject.GetComponentInParent<ItemView>();
                if (iv != null && iv != itemView)
                {
                    target = iv;
                    break;
                }
            }

            if (target != null)
                adapter.HandleDrop(itemView, target);
        }
    }
}
