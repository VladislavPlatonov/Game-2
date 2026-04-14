using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        [Header("Right Info Panel")]
        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private TextMeshProUGUI needToCallText;
        [SerializeField] private TextMeshProUGUI actionLogText;

        [Header("Pot Animation")]
        [SerializeField] private int yellowThreshold = 20;
        [SerializeField] private int redThreshold = 50;
        [SerializeField] private Color lowPotColor = new Color(0.2f, 1f, 0.2f);
        [SerializeField] private Color midPotColor = new Color(1f, 0.9f, 0.15f);
        [SerializeField] private Color highPotColor = new Color(1f, 0.2f, 0.2f);
        [SerializeField] private float potPulseScale = 1.08f;
        [SerializeField] private float potPulseSpeed = 12f;
        [SerializeField] private float potShakeDuration = 0.35f;
        [SerializeField] private float yellowShakeAmount = 3f;
        [SerializeField] private float redShakeAmount = 7f;

        [Header("State Animation")]
        [SerializeField] private float stateFadeDuration = 0.12f;
        [SerializeField] private float stateScaleFrom = 0.85f;
        [SerializeField] private float stateScaleTo = 1.08f;

        [Header("Right Panel Animation")]
        [SerializeField] private float rightFadeDuration = 0.08f;
        [SerializeField] private float rightScaleFrom = 0.94f;
        [SerializeField] private float rightScaleTo = 1.04f;

        private PokerGame game;

        private int lastPot = -1;
        private string lastStateLabel = string.Empty;

        private string lastTurnLabel = string.Empty;
        private string lastNeedToCallLabel = string.Empty;
        private string lastActionLogLabel = string.Empty;

        private RectTransform potRect;
        private RectTransform stateRect;
        private RectTransform turnRect;
        private RectTransform needToCallRect;
        private RectTransform actionLogRect;

        private Vector3 potBaseScale;
        private Vector2 potBaseAnchoredPos;
        private Vector3 stateBaseScale;
        private Vector3 turnBaseScale;
        private Vector3 needToCallBaseScale;
        private Vector3 actionLogBaseScale;

        private Coroutine potAnimCoroutine;
        private Coroutine stateAnimCoroutine;
        private Coroutine turnAnimCoroutine;
        private Coroutine needToCallAnimCoroutine;
        private Coroutine actionLogAnimCoroutine;

        private void Awake()
        {
            game = FindFirstObjectByType<PokerGame>();

            if (potText != null)
            {
                potRect = potText.rectTransform;
                potBaseScale = potRect.localScale;
                potBaseAnchoredPos = potRect.anchoredPosition;
            }

            if (stateText != null)
            {
                stateRect = stateText.rectTransform;
                stateBaseScale = stateRect.localScale;
            }

            if (turnText != null)
            {
                turnRect = turnText.rectTransform;
                turnBaseScale = turnRect.localScale;
            }

            if (needToCallText != null)
            {
                needToCallRect = needToCallText.rectTransform;
                needToCallBaseScale = needToCallRect.localScale;
            }

            if (actionLogText != null)
            {
                actionLogRect = actionLogText.rectTransform;
                actionLogBaseScale = actionLogRect.localScale;
            }
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

            if (raisePanel != null)
                raisePanel.SetActive(false);

            if (raiseSlider != null)
            {
                raiseSlider.onValueChanged.AddListener(v =>
                {
                    if (raiseAmountText != null)
                        raiseAmountText.text = Mathf.RoundToInt(v).ToString();
                });
            }

            if (raiseConfirmBtn != null)
                raiseConfirmBtn.onClick.AddListener(ConfirmRaise);

            if (raiseCancelBtn != null)
                raiseCancelBtn.onClick.AddListener(() => raisePanel.SetActive(false));

            Refresh();
        }

        private void OnTurnChanged(bool playerTurn)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (game == null)
                return;

            int pot = game.GetCurrentPot();
            int toCall = Mathf.Max(0, game.GetCurrentBet() - game.GetPlayerBetThisRound());
            int hp = game.GetPlayerHP();
            bool myTurn = game.IsWaitingForPlayerAction();

            UpdatePot(pot);
            UpdateState();
            UpdateRightPanel(myTurn, toCall);

            if (callButtonText != null)
                callButtonText.text = toCall > 0 ? $" ŒÀÀ ({toCall})" : " ŒÀÀ";

            btnFold.interactable = myTurn;
            btnCheck.interactable = myTurn && toCall == 0;
            btnCall.interactable = myTurn && toCall > 0 && hp > 0;
            btnRaise.interactable = myTurn && hp > toCall;
            btnAllIn.interactable = myTurn && hp > 0;
        }

        private void UpdatePot(int pot)
        {
            if (potText == null)
                return;

            potText.text = $"¡¿Õ  - {pot} ’œ";

            Color targetColor = EvaluatePotColor(pot);
            potText.color = targetColor;

            if (pot != lastPot)
            {
                lastPot = pot;

                if (potAnimCoroutine != null)
                    StopCoroutine(potAnimCoroutine);

                potAnimCoroutine = StartCoroutine(AnimatePotChange(targetColor, EvaluateShakeAmount(pot)));
            }
        }

        private void UpdateState()
        {
            if (stateText == null)
                return;

            string newLabel = GetStateLabel(game.GetCurrentState());

            if (newLabel == lastStateLabel)
                return;

            lastStateLabel = newLabel;

            if (stateAnimCoroutine != null)
                StopCoroutine(stateAnimCoroutine);

            stateAnimCoroutine = StartCoroutine(AnimateStateChange(newLabel));
        }

        private void UpdateRightPanel(bool myTurn, int toCall)
        {
            string newTurnLabel = myTurn ? "“¬Œ… ’Œƒ" : "’Œƒ œ–Œ“»¬Õ» ¿";
            string newNeedToCallLabel = $"Õ”∆ÕŒ ”–¿¬Õﬂ“‹: {toCall} ’œ";

            string newActionLogLabel;
            if (!myTurn)
                newActionLogLabel = "Œ∆»ƒ¿Õ»≈ ’Œƒ¿ œ–Œ“»¬Õ» ¿";
            else if (toCall > 0)
                newActionLogLabel = "¬€¡≈–» ƒ≈…—“¬»≈";
            else
                newActionLogLabel = "ÃŒ∆ÕŒ ◊≈  »À» —“¿¬ ”";

            if (turnText != null && newTurnLabel != lastTurnLabel)
            {
                lastTurnLabel = newTurnLabel;

                if (turnAnimCoroutine != null)
                    StopCoroutine(turnAnimCoroutine);

                turnAnimCoroutine = StartCoroutine(AnimateRightTextChange(
                    turnText,
                    turnRect,
                    turnBaseScale,
                    newTurnLabel
                ));
            }

            if (needToCallText != null && newNeedToCallLabel != lastNeedToCallLabel)
            {
                lastNeedToCallLabel = newNeedToCallLabel;

                if (needToCallAnimCoroutine != null)
                    StopCoroutine(needToCallAnimCoroutine);

                needToCallAnimCoroutine = StartCoroutine(AnimateRightTextChange(
                    needToCallText,
                    needToCallRect,
                    needToCallBaseScale,
                    newNeedToCallLabel
                ));
            }

            if (actionLogText != null && newActionLogLabel != lastActionLogLabel)
            {
                lastActionLogLabel = newActionLogLabel;

                if (actionLogAnimCoroutine != null)
                    StopCoroutine(actionLogAnimCoroutine);

                actionLogAnimCoroutine = StartCoroutine(AnimateRightTextChange(
                    actionLogText,
                    actionLogRect,
                    actionLogBaseScale,
                    newActionLogLabel
                ));
            }
        }

        private Color EvaluatePotColor(int pot)
        {
            if (pot <= yellowThreshold)
            {
                float t = yellowThreshold <= 0 ? 1f : Mathf.Clamp01((float)pot / yellowThreshold);
                return Color.Lerp(lowPotColor, midPotColor, t);
            }

            if (pot <= redThreshold)
            {
                float range = Mathf.Max(1, redThreshold - yellowThreshold);
                float t = Mathf.Clamp01((pot - yellowThreshold) / range);
                return Color.Lerp(midPotColor, highPotColor, t);
            }

            return highPotColor;
        }

        private float EvaluateShakeAmount(int pot)
        {
            if (pot >= redThreshold)
                return redShakeAmount;

            if (pot >= yellowThreshold)
                return yellowShakeAmount;

            return 0f;
        }

        private IEnumerator AnimatePotChange(Color targetColor, float shakeAmount)
        {
            if (potRect == null)
                yield break;

            float duration = potShakeDuration;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float pulse = 1f + Mathf.Sin(t * Mathf.PI) * (potPulseScale - 1f);
                potRect.localScale = potBaseScale * pulse;

                if (shakeAmount > 0.01f)
                {
                    float wave = Mathf.Sin(t * Mathf.PI * potPulseSpeed);
                    potRect.anchoredPosition = potBaseAnchoredPos + new Vector2(wave * shakeAmount, 0f);
                }
                else
                {
                    potRect.anchoredPosition = potBaseAnchoredPos;
                }

                potText.color = targetColor;
                yield return null;
            }

            potRect.localScale = potBaseScale;
            potRect.anchoredPosition = potBaseAnchoredPos;
            potText.color = targetColor;
            potAnimCoroutine = null;
        }

        private IEnumerator AnimateStateChange(string newLabel)
        {
            if (stateRect == null)
            {
                stateText.text = newLabel;
                yield break;
            }

            Color baseColor = stateText.color;
            float half = Mathf.Max(0.01f, stateFadeDuration);

            float t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / half);

                stateText.alpha = 1f - k;
                stateRect.localScale = Vector3.Lerp(stateBaseScale, stateBaseScale * stateScaleFrom, k);
                yield return null;
            }

            stateText.text = newLabel;

            t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / half);

                stateText.alpha = k;
                stateRect.localScale = Vector3.Lerp(
                    stateBaseScale * stateScaleTo,
                    stateBaseScale,
                    k
                );
                yield return null;
            }

            stateText.alpha = 1f;
            stateText.color = baseColor;
            stateRect.localScale = stateBaseScale;
            stateAnimCoroutine = null;
        }

        private IEnumerator AnimateRightTextChange(
            TextMeshProUGUI text,
            RectTransform rect,
            Vector3 baseScale,
            string newLabel)
        {
            if (text == null)
                yield break;

            if (rect == null)
            {
                text.text = newLabel;
                yield break;
            }

            float half = Mathf.Max(0.01f, rightFadeDuration);

            float t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / half);

                text.alpha = 1f - k;
                rect.localScale = Vector3.Lerp(baseScale, baseScale * rightScaleFrom, k);
                yield return null;
            }

            text.text = newLabel;

            t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / half);

                text.alpha = k;
                rect.localScale = Vector3.Lerp(baseScale * rightScaleTo, baseScale, k);
                yield return null;
            }

            text.alpha = 1f;
            rect.localScale = baseScale;
        }

        private string GetStateLabel(GameState state)
        {
            return state switch
            {
                GameState.Preflop => "œ–≈‘ÀŒœ",
                GameState.Flop => "‘ÀŒœ",
                GameState.Turn => "“≈–Õ",
                GameState.River => "–»¬≈–",
                GameState.Showdown => "ÿŒ”ƒ¿”Õ",
                GameState.HandOver => "–¿«ƒ¿◊¿ «¿¬≈–ÿ≈Õ¿",
                GameState.GameOver => " ŒÕ≈÷ »√–€",
                _ => ""
            };
        }

        private void OpenRaise()
        {
            if (game == null || raisePanel == null || raiseSlider == null)
                return;

            int toCall = Mathf.Max(0, game.GetCurrentBet() - game.GetPlayerBetThisRound());
            int hp = game.GetPlayerHP();

            int minRaise = Mathf.Min(toCall + 1, hp);
            int maxRaise = hp;

            raiseSlider.minValue = minRaise;
            raiseSlider.maxValue = maxRaise;
            raiseSlider.value = Mathf.Clamp(toCall + 5, minRaise, maxRaise);

            if (raiseAmountText != null)
                raiseAmountText.text = Mathf.RoundToInt(raiseSlider.value).ToString();

            raisePanel.SetActive(true);
        }

        private void ConfirmRaise()
        {
            if (game == null || raisePanel == null || raiseSlider == null)
                return;

            int raiseBy = Mathf.RoundToInt(raiseSlider.value);
            game.PlayerRaise(raiseBy);

            raisePanel.SetActive(false);
        }
    }
}
