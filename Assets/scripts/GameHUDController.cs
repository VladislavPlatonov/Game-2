using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Poker
{
    public class GameHUDController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button btnFold;
        [SerializeField] private Button btnCheck;
        [SerializeField] private Button btnCall;
        [SerializeField] private Button btnRaise;
        [SerializeField] private Button btnAllIn;

        [Header("Button Labels")]
        [SerializeField] private TextMeshProUGUI callButtonText;

        [Header("Raise UI")]
        [SerializeField] private GameObject raisePanel;
        [SerializeField] private Slider raiseSlider;
        [SerializeField] private TextMeshProUGUI raiseAmountText;
        [SerializeField] private Button raiseConfirmBtn;
        [SerializeField] private Button raiseCancelBtn;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI potText;
        [SerializeField] private TextMeshProUGUI stateText;

        private PokerGame game;

        private void Awake()
        {
            game = FindFirstObjectByType<PokerGame>();
        }

        private void OnEnable()
        {
            if (game != null)
            {
                game.OnPotUpdated += Refresh;
                game.OnGameStateChanged += Refresh;
                game.OnTurnChanged += OnTurnChanged;
            }
        }

        private void OnDisable()
        {
            if (game != null)
            {
                game.OnPotUpdated -= Refresh;
                game.OnGameStateChanged -= Refresh;
                game.OnTurnChanged -= OnTurnChanged;
            }
        }

        private void Start()
        {
            btnFold.onClick.AddListener(() => game.PlayerFold());
            btnCheck.onClick.AddListener(() => game.PlayerCheck());
            btnCall.onClick.AddListener(() => game.PlayerCall());
            btnAllIn.onClick.AddListener(() => game.PlayerAllIn());
            btnRaise.onClick.AddListener(OpenRaise);

            if (raisePanel != null) raisePanel.SetActive(false);

            if (raiseSlider != null)
            {
                raiseSlider.onValueChanged.AddListener(v =>
                {
                    if (raiseAmountText != null)
                        raiseAmountText.text = $"{Mathf.RoundToInt(v)}";
                });
            }

            if (raiseConfirmBtn != null) raiseConfirmBtn.onClick.AddListener(ConfirmRaise);
            if (raiseCancelBtn != null) raiseCancelBtn.onClick.AddListener(() => raisePanel.SetActive(false));

            Refresh();
        }

        private void OnTurnChanged(bool playerTurn)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (game == null) return;

            int pot = game.GetCurrentPot();
            int toCall = Mathf.Max(0, game.GetCurrentBet() - game.GetPlayerBetThisRound());
            int hp = game.GetPlayerHP();
            bool myTurn = game.IsWaitingForPlayerAction();

            if (potText != null)
                potText.text = $"ÁÀÍÊ: {pot} HP";

            if (stateText != null)
            {
                stateText.text = game.GetCurrentState() switch
                {
                    
                    GameState.Preflop => "ÏÐÅÔËÎÏ",
                    GameState.Flop => "ÔËÎÏ",
                    GameState.Turn => "Ò¨ÐÍ",
                    GameState.River => "ÐÈÂÅÐ",
                    GameState.Showdown => "ÂÑÊÐÛÒÈÅ",
                    GameState.HandOver => "ÊÎÍÅÖ ÐÀÇÄÀ×È",
                    GameState.GameOver => "ÊÎÍÅÖ",
                    _ => ""
                };
            }

            if (callButtonText != null)
            {
                callButtonText.text = toCall > 0 ? $"ÊÎËË ({toCall})" : "ÊÎËË";
            }

            btnFold.interactable = myTurn;
            btnCheck.interactable = myTurn && toCall == 0;
            btnCall.interactable = myTurn && toCall > 0 && hp > 0;
            btnRaise.interactable = myTurn && hp > toCall;
            btnAllIn.interactable = myTurn && hp > 0;
        }

        private void OpenRaise()
        {
            if (game == null || raisePanel == null || raiseSlider == null) return;

            int toCall = Mathf.Max(0, game.GetCurrentBet() - game.GetPlayerBetThisRound());
            int hp = game.GetPlayerHP();

            int minRaise = Mathf.Min(toCall + 1, hp);
            int maxRaise = hp;

            raiseSlider.minValue = minRaise;
            raiseSlider.maxValue = maxRaise;
            raiseSlider.value = Mathf.Clamp(toCall + 5, minRaise, maxRaise);

            if (raiseAmountText != null)
                raiseAmountText.text = $"{Mathf.RoundToInt(raiseSlider.value)}";

            raisePanel.SetActive(true);
        }

        private void ConfirmRaise()
        {
            if (game == null || raisePanel == null || raiseSlider == null) return;

            int raiseBy = Mathf.RoundToInt(raiseSlider.value);
            game.PlayerRaise(raiseBy);

            raisePanel.SetActive(false);
        }
    }
}