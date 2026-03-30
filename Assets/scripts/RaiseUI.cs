// RaiseUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Poker
{
    public class RaiseUI : MonoBehaviour
    {
        [SerializeField] private PokerGame game;

        [Header("UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI amountText;

        private void Awake()
        {
            if (game == null) game = FindFirstObjectByType<PokerGame>();
            if (panel != null) panel.SetActive(false);
        }

        private void UpdateText(float v)
        {
            if (amountText != null) amountText.text = $"{Mathf.RoundToInt(v)}";
        }

        // OnClick: кнопка "Повысить" (открыть)
        public void Open()
        {
            if (game == null || panel == null || slider == null) return;

            int toCall = game.GetCurrentBet() - game.GetPlayerBetThisRound();
            int hp = game.GetPlayerHP();

            int min = Mathf.Clamp(toCall + 1, 1, hp);
            int max = Mathf.Max(min, hp);

            slider.minValue = min;
            slider.maxValue = max;
            slider.value = Mathf.Clamp(min + 2, min, max);

            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(UpdateText);

            UpdateText(slider.value);

            panel.SetActive(true);
        }

        // OnClick: подтвердить рейз
        public void Confirm()
        {
            if (game == null || panel == null || slider == null) return;

            int amount = Mathf.RoundToInt(slider.value);
            game.PlayerRaise(amount);

            panel.SetActive(false);
        }

        // OnClick: отмена
        public void Cancel()
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}