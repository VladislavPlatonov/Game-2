using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Poker
{
    public class PokerGameOverUI : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PokerGame game;

        [Header("UI")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI winText;
        [SerializeField] private TextMeshProUGUI loseText;

        [Header("Button")]
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private string mainMenuSceneName = "SampleScene";

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.35f;
        [SerializeField] private float textPopDuration = 0.28f;
        [SerializeField] private float loseShakeDuration = 0.45f;
        [SerializeField] private float loseShakeAmount = 18f;

        [Header("Sounds")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip winSound;
        [SerializeField] private AudioClip loseSound;
        [SerializeField] private float soundVolume = 0.8f;

        private CanvasGroup canvasGroup;

        private Vector3 winBaseScale;
        private Vector3 loseBaseScale;

        private RectTransform loseRect;
        private Vector2 loseBasePos;

        private bool shown;

        private void Awake()
        {
            if (game == null)
                game = FindFirstObjectByType<PokerGame>();

            if (gameOverPanel != null)
            {
                canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();

                if (canvasGroup == null)
                    canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();

                gameOverPanel.SetActive(false);
            }

            if (winText != null)
                winBaseScale = winText.transform.localScale;

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            if (loseText != null)
            {
                loseBaseScale = loseText.transform.localScale;
                loseRect = loseText.GetComponent<RectTransform>();

                if (loseRect != null)
                    loseBasePos = loseRect.anchoredPosition;
            }

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        private void Update()
        {
            if (shown) return;

            if (game == null)
                game = FindFirstObjectByType<PokerGame>();

            if (game == null) return;

            if (game.GetCurrentState() == GameState.GameOver)
                Show(game.GetGameOverMessage());
        }

        private void Show(string message)
        {

            AudioManager.Instance.StopAll();
            shown = true;

            bool isWin = message.Contains("¬€ ¬€»√–¿À»");
            bool isLose = message.Contains("¬€ œ–Œ»√–¿À»");
            PlayGameOverSound(isWin, isLose);

            if (winText != null)
                winText.gameObject.SetActive(isWin);

            if (loseText != null)
                loseText.gameObject.SetActive(isLose);

            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();

            StopAllCoroutines();
            StartCoroutine(ShowRoutine(isLose));
        }

        private IEnumerator ShowRoutine(bool isLose)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (winText != null)
                winText.transform.localScale = Vector3.zero;

            if (loseText != null)
                loseText.transform.localScale = Vector3.zero;

            float t = 0f;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / fadeDuration;
                canvasGroup.alpha = t;
                yield return null;
            }

            if (isLose)
                yield return StartCoroutine(AnimateLose());
            else
                yield return StartCoroutine(AnimateWin());

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            Time.timeScale = 0f;
        }

        private IEnumerator AnimateWin()
        {
            float t = 0f;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / textPopDuration;
                winText.transform.localScale = winBaseScale * EaseOutBack(t);
                yield return null;
            }

            winText.transform.localScale = winBaseScale;
        }

        private IEnumerator AnimateLose()
        {
            float t = 0f;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / textPopDuration;
                loseText.transform.localScale = loseBaseScale * EaseOutBack(t);
                yield return null;
            }

            loseText.transform.localScale = loseBaseScale;

            float elapsed = 0f;

            while (elapsed < loseShakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float strength = Mathf.Lerp(loseShakeAmount, 0f, elapsed / loseShakeDuration);
                loseRect.anchoredPosition = loseBasePos + Random.insideUnitCircle * strength;

                yield return null;
            }

            loseRect.anchoredPosition = loseBasePos;
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private float EaseOutBack(float x)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }

        private void PlayGameOverSound(bool isWin, bool isLose)
        {
            if (audioSource == null)
                return;

            AudioClip clip = null;

            if (isWin)
                clip = winSound;
            else if (isLose)
                clip = loseSound;

            if (clip == null)
                return;

            audioSource.PlayOneShot(clip, soundVolume);
        }
    }
}