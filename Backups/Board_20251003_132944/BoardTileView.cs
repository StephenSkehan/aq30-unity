using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Board
{
    [RequireComponent(typeof(Image))]
    public class BoardTileView : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Visual References")]
        [SerializeField] Image IconImage;
        [SerializeField] Image BackgroundImage;
        [SerializeField] Color SelectedColor = Color.yellow;
        [SerializeField] Color NormalColor = Color.white;
        [SerializeField] Color GeneratorColor = Color.cyan;
        #endregion

        #region Private Fields
        bool _isGenerator;
        bool _isSelected;
        int _iconIndex = -1;
        int _tier;
        Sprite _currentSprite;
        MergeBoardController _controller;
        #endregion

        #region Public Properties
        public bool IsGenerator => _isGenerator;
        public bool IsEmpty => _currentSprite == null && !_isGenerator;
        public int IconIndex => _iconIndex;
        public int Tier => _tier;
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            if (!IconImage) IconImage = GetComponent<Image>();
            if (!BackgroundImage) BackgroundImage = GetComponent<Image>();
            
            _controller = GetComponentInParent<MergeBoardController>();
            if (_controller == null)
            {
                Debug.LogWarning($"[BoardTileView] {name} could not find MergeBoardController in parent hierarchy");
            }
        }

        void Start()
        {
            UpdateVisuals();
        }

        void OnMouseDown()
        {
            if (_controller != null)
            {
                _controller.OnTileClicked(this);
            }
            else
            {
                Debug.LogWarning($"[BoardTileView] {name} has no controller reference, cannot handle click");
            }
        }
        #endregion

        #region Public Methods
        public void SetIsGenerator(bool isGenerator)
        {
            _isGenerator = isGenerator;
            UpdateVisuals();
            Debug.Log($"[BoardTileView] {name} SetIsGenerator({isGenerator})");
        }

        public void SetSprite(Sprite sprite, int iconIndex, int tier)
        {
            _currentSprite = sprite;
            _iconIndex = iconIndex;
            _tier = tier;
            _isGenerator = false;
            UpdateVisuals();
        }

        public void Clear()
        {
            _currentSprite = null;
            _iconIndex = -1;
            _tier = 0;
            _isGenerator = false;
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
                {
                    IconImage.color = GeneratorColor;
                }
                else
                {
                    IconImage.color = Color.white;
                }
            }

            if (BackgroundImage)
            {
                if (_isSelected)
                {
                    BackgroundImage.color = SelectedColor;
                }
                else if (_isGenerator)
                {
                    BackgroundImage.color = GeneratorColor;
                }
                else
                {
                    BackgroundImage.color = NormalColor;
                }
            }
        }
        #endregion
    }
}