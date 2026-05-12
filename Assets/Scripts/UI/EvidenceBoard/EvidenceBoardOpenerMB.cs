using UnityEngine;
using UnityEngine.EventSystems;

namespace AQ.App.UI.EvidenceBoard
{
    public class EvidenceBoardOpenerMB : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            EvidenceBoardScreen.Open();
        }
    }
}
