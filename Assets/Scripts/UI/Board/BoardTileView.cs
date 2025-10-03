using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AQ.App.UI.Board
{
    [RequireComponent(typeof(Image))]
    public sealed class BoardTileView : MonoBehaviour, 
        IPointerDownHandler,
        IBeginDragHandler, 
        IDragHandler, 
        IEndDragHandler,
        IPointerClickHandler  // ADD THIS
    {
        #region Inspector Fields
        [Header("Visual References")]
        [SerializeField] Image IconImage;
        [SerializeField] Image BackgroundImage;
        [SerializeField] Color SelectedColor = Color.yellow;
        [SerializeField] Color NormalColor = Color.white;
        [SerializeField] Color GeneratorColor = Color.cyan;
        
        [Header("Drag Setup")]
        [SerializeField] RectTransform DragLayer;
        #endregion

        #region Private Fields
        bool _isGenerator;
        bool _isSelected;
        int _iconIndex = -1;
        int _tier;
        Sprite _currentSprite;
        MergeBoardController _controller;
        
        // Drag state
        RectTransform _iconRT;
        RectTransform _homeParent;
        Vector2 _savedAnchorMin, _savedAnchorMax, _savedPivot, _savedSize;
        int _savedSiblingIndex;
        bool _isDragging;  // CRITICAL: prevents click after drag
        
        Canvas _rootCanvas;
        CanvasGroup _iconCG;  // CanvasGroup on the Icon child
        #endregion

        #region Public Properties
        public bool IsGenerator => _isGenerator;
        public bool IsEmpty => _currentSprite == null && !_isGenerator;
        public int IconIndex => _iconIndex;
        public int Tier => _tier;
        public int Row { get; private set; }
        public int Col { get; private set; }
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            // Find icon image as CHILD
            if (!IconImage)
            {
                Transform iconChild = transform.Find("Icon") ?? transform.Find("Item") ?? transform.Find("icon");
                if (iconChild)
                {
                    IconImage = iconChild.GetComponent<Image>();
                }
                else
                {
                    var images = GetComponentsInChildren<Image>();
                    if (images.Length > 1)
                        IconImage = images[1];
                }
            }

            if (IconImage)
            {
                _iconRT = IconImage.rectTransform;
                _homeParent = (RectTransform)_iconRT.parent;
                
                // Get or add CanvasGroup to the ICON (not the root)
                _iconCG = IconImage.GetComponent<CanvasGroup>();
                if (!_iconCG)
                    _iconCG = IconImage.gameObject.AddComponent<CanvasGroup>();
                _iconCG.alpha = 1f;
            }

            // Background is the Image on root
            if (!BackgroundImage)
            {
                BackgroundImage = GetComponent<Image>();
            }
            
            // Find Canvas_Board
            _rootCanvas = GetComponentInParent<Canvas>();
            
            // Find controller
            _controller = GetComponentInParent<MergeBoardController>();
            
            // Find DragLayer
            if (!DragLayer)
            {
                var canvasBoard = GameObject.Find("Canvas_Board");
                if (canvasBoard)
                {
                    var dragLayerObj = canvasBoard.transform.Find("DragLayer");
                    if (dragLayerObj)
                        DragLayer = dragLayerObj.GetComponent<RectTransform>();
                }
            }

            // Parse row/col from name (e.g., "slot_02_05")
            ParseRowColFromName();
        }

        void ParseRowColFromName()
        {
            string n = name;
            if (n.StartsWith("slot_"))
            {
                var parts = n.Split('_');
                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[1], out int r))
                        Row = r;
                    if (int.TryParse(parts[2], out int c))
                        Col = c;
                }
            }
        }
        #endregion

        #region Event Handlers (FIXED PATTERN)
        
        public void OnPointerDown(PointerEventData eventData)
        {
            // ONLY cache pointer data - DO NOT call any game play logic here
            _isDragging = false;  // Reset per interaction
            // Could cache eventData.position if needed, but don't trigger selection
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsEmpty || IsGenerator || !_controller) return;
            if (!IconImage || !DragLayer) return;

            _isDragging = true;

            // 1) Clear any prior selection (CRITICAL FIX)
            _controller.ClearSelection();
            EventSystem.current?.SetSelectedGameObject(null);

            // 2) Cache layout + parent
            _savedSiblingIndex = _iconRT.GetSiblingIndex();
            _savedAnchorMin = _iconRT.anchorMin;
            _savedAnchorMax = _iconRT.anchorMax;
            _savedPivot = _iconRT.pivot;
            _savedSize = _iconRT.sizeDelta;

            // 3) Move to DragLayer (worldPositionStays: false is CRITICAL)
            _iconRT.SetParent(DragLayer, worldPositionStays: false);
            _iconRT.anchorMin = _iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            _iconRT.pivot = new Vector2(0.5f, 0.5f);
            _iconRT.localScale = Vector3.one;
            _iconRT.sizeDelta = _savedSize;
            _iconRT.SetAsLastSibling();

            // 4) Disable raycasts on icon (CRITICAL for drop detection)
            _iconCG.blocksRaycasts = false;
            IconImage.raycastTarget = false;

            // 5) Snap to pointer immediately
            FollowPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            FollowPointer(eventData);
        }

        void FollowPointer(PointerEventData eventData)
        {
            if (!DragLayer || !_iconRT) return;

            // ScreenSpace-Overlay => camera is null
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                DragLayer,
                eventData.position,
                _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _rootCanvas.worldCamera,
                out Vector2 localPoint
            );
            
            _iconRT.anchoredPosition = localPoint;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            // Restore parent + layout
            _iconRT.SetParent(_homeParent, worldPositionStays: false);
            _iconRT.SetSiblingIndex(_savedSiblingIndex);
            _iconRT.anchorMin = _savedAnchorMin;
            _iconRT.anchorMax = _savedAnchorMax;
            _iconRT.pivot = _savedPivot;
            _iconRT.sizeDelta = _savedSize;
            _iconRT.localScale = Vector3.one;

            // Re-enable raycasts
            _iconCG.blocksRaycasts = true;
            IconImage.raycastTarget = true;

            // Find target tile under pointer
            var targetGO = eventData.pointerCurrentRaycast.gameObject;
            var targetTile = targetGO ? targetGO.GetComponentInParent<BoardTileView>() : null;

            // Notify controller
            if (_controller)
            {
                _controller.EndDrag((Row, Col), targetTile != null ? ((int, int)?)(targetTile.Row, targetTile.Col) : null);
            }

            // Clear any latched selection (CRITICAL FIX)
            _controller.ClearSelection();
            EventSystem.current?.SetSelectedGameObject(null);

            _isDragging = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // CRITICAL: Suppress click that follows a drag
            if (_isDragging) return;
            
            // Now safe to trigger selection/merge
            if (_controller != null)
            {
                _controller.OnTileClicked(this);
            }
        }
        
        #endregion

        #region Public Methods
        public void SetIsGenerator(bool isGenerator)
        {
            _isGenerator = isGenerator;
            UpdateVisuals();
        }

        public void SetSprite(Sprite sprite, int iconIndex, int tier)
        {
            _currentSprite = sprite;
            _iconIndex = iconIndex;
            _tier = tier;
            _isGenerator = false;
            
            if (IconImage)
            {
                IconImage.sprite = sprite;
                IconImage.enabled = sprite != null;
            }
            
            UpdateVisuals();
        }

        public void Clear()
        {
            _currentSprite = null;
            _iconIndex = -1;
            _tier = 0;
            _isGenerator = false;
            
            if (IconImage)
            {
                IconImage.sprite = null;
                IconImage.enabled = false;
            }
            
            UpdateVisuals();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateVisuals();
        }
        #endregion

        #region Visual Updates
        void UpdateVisuals()
        {
            if (IconImage)
            {
                IconImage.sprite = _currentSprite;
                IconImage.enabled = _currentSprite != null || _isGenerator;
                
                if (_isGenerator && _currentSprite == null)
                    IconImage.color = GeneratorColor;
                else
                    IconImage.color = Color.white;
            }

            if (BackgroundImage)
            {
                if (_isSelected)
                    BackgroundImage.color = SelectedColor;
                else if (_isGenerator)
                    BackgroundImage.color = GeneratorColor;
                else
                    BackgroundImage.color = NormalColor;
            }
        }
        #endregion
    }
}