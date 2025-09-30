using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class BoardTileView : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Button button;
    [SerializeField] private Image bg;
    [SerializeField] private Image itemImage;
    [SerializeField] private Image highlight;
    [SerializeField] private TMP_Text countLabel;

    public Button Button => button;

    public void Bind(Sprite sprite, int count = -1)
    {
        SetSprite(sprite);
        SetCount(count);
        SetHighlight(false);
    }

    public void SetSprite(Sprite sprite)
    {
        if (itemImage == null) return;
        itemImage.sprite = sprite;
        itemImage.enabled = sprite != null;
        if (itemImage) itemImage.preserveAspect = true;
    }

    public void SetHighlight(bool on)
    {
        if (highlight != null) highlight.enabled = on;
    }

    public void SetCount(int count)
    {
        if (!countLabel) return;
        if (count < 0)
        {
            countLabel.gameObject.SetActive(false);
        }
        else
        {
            countLabel.gameObject.SetActive(true);
            countLabel.text = count.ToString();
        }
    }

    public void Clear()
    {
        SetSprite(null);
        SetCount(-1);
        SetHighlight(false);
    }
}
