using UnityEngine;
using UnityEngine.EventSystems;

namespace Poker
{
    public class UIButtonSound : MonoBehaviour, IPointerEnterHandler
    {
        [Header("Custom Sounds")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip hoverSound;

        // 🔊 КЛИК
        public void PlayClick()
        {
            if (AudioManager.Instance == null) return;

            AudioManager.Instance.PlayButtonClick();
        }

        // 🔊 НАВЕДЕНИЕ
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (AudioManager.Instance == null) return;

            AudioManager.Instance.PlayButtonHover();
        }
    }
}
