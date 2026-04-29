using System.Collections;
using TMPro;
using UnityEngine;

namespace Poker
{
    public class NPCDialogueController : MonoBehaviour
    {
        [Header("3D Dialogue Refs")]
        [SerializeField] private GameObject dialogueRoot;
        [SerializeField] private Transform bubbleVisual;
        [SerializeField] private TextMeshPro dialogueText;
        [SerializeField] private Camera targetCamera;

        [Header("Animation")]
        [SerializeField] private float appearDuration = 0.22f;
        [SerializeField] private float textStartDelay = 0.08f;
        [SerializeField] private float typeSpeed = 0.025f;
        [SerializeField] private float showDuration = 2.5f;
        [SerializeField] private float disappearDuration = 0.18f;

        [Header("Typing Sound")]
        [SerializeField] private AudioSource typingAudioSource;
        [SerializeField] private AudioClip[] typingClips;
        [SerializeField] private float typingVolume = 0.55f;
        [SerializeField] private float typingPitchMin = 0.92f;
        [SerializeField] private float typingPitchMax = 1.08f;
        [SerializeField] private int playSoundEveryLetters = 1;
        [SerializeField] private bool skipSpacesForSound = true;

        [Header("Motion")]
        [SerializeField] private float floatAmount = 0.025f;
        [SerializeField] private float floatSpeed = 1.6f;
        [SerializeField] private bool lookAtCamera = true;

        [Header("Debug")]
        [SerializeField] private bool ignoreChanceForTesting = false;

        private Coroutine showRoutine;

        private Vector3 rootBaseLocalPosition;
        private Vector3 bubbleBaseScale;
        private Vector3 textBaseScale;

        private void Awake()
        {
            if (dialogueRoot == null)
                dialogueRoot = gameObject;

            if (bubbleVisual == null)
                bubbleVisual = dialogueRoot.transform;

            if (dialogueText == null)
                dialogueText = GetComponentInChildren<TextMeshPro>(true);

            if (targetCamera == null)
                targetCamera = Camera.main;

            if (typingAudioSource == null)
                typingAudioSource = GetComponent<AudioSource>();

            rootBaseLocalPosition = dialogueRoot.transform.localPosition;

            if (bubbleVisual != null)
                bubbleBaseScale = bubbleVisual.localScale;

            if (dialogueText != null)
                textBaseScale = dialogueText.transform.localScale;

            HideInstant();
        }

        private void LateUpdate()
        {
            if (dialogueRoot == null)
                return;

            float y = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
            dialogueRoot.transform.localPosition = rootBaseLocalPosition + new Vector3(0f, y, 0f);

            if (lookAtCamera)
                LookAtCamera();
        }

        public void TryShow(NPCDialogueIntent intent, float chance)
        {
            if (intent == NPCDialogueIntent.None)
                return;

            if (!ignoreChanceForTesting && Random.value > chance)
                return;

            string line = GetLine(intent);

            if (string.IsNullOrWhiteSpace(line))
                return;

            Show(line);
        }

        private void Show(string text)
        {
            if (dialogueRoot == null || bubbleVisual == null || dialogueText == null)
                return;

            if (showRoutine != null)
                StopCoroutine(showRoutine);

            showRoutine = StartCoroutine(ShowRoutine(text));
        }

        private IEnumerator ShowRoutine(string text)
        {
            if (bubbleVisual != null)
                bubbleVisual.gameObject.SetActive(true);

            if (dialogueText != null)
                dialogueText.gameObject.SetActive(true);

            dialogueText.text = "";
            dialogueText.alpha = 0f;

            bubbleVisual.localScale = Vector3.zero;
            dialogueText.transform.localScale = textBaseScale * 0.85f;

            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / appearDuration;
                float k = Mathf.Clamp01(t);
                float eased = EaseOutBack(k);

                bubbleVisual.localScale = Vector3.LerpUnclamped(
                    Vector3.zero,
                    bubbleBaseScale,
                    eased
                );

                yield return null;
            }

            bubbleVisual.localScale = bubbleBaseScale;

            yield return new WaitForSeconds(textStartDelay);

            t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / 0.12f;
                float k = Mathf.Clamp01(t);

                dialogueText.alpha = k;
                dialogueText.transform.localScale = Vector3.Lerp(
                    textBaseScale * 0.85f,
                    textBaseScale,
                    EaseOutQuad(k)
                );

                yield return null;
            }

            yield return StartCoroutine(TypeText(text));

            yield return new WaitForSeconds(showDuration);

            t = 0f;

            Vector3 startBubbleScale = bubbleVisual.localScale;
            Vector3 startTextScale = dialogueText.transform.localScale;

            while (t < 1f)
            {
                t += Time.deltaTime / disappearDuration;
                float k = Mathf.Clamp01(t);

                dialogueText.alpha = 1f - k;

                dialogueText.transform.localScale = Vector3.Lerp(
                    startTextScale,
                    textBaseScale * 0.85f,
                    k
                );

                bubbleVisual.localScale = Vector3.Lerp(
                    startBubbleScale,
                    Vector3.zero,
                    EaseInQuad(k)
                );

                yield return null;
            }

            HideInstant();
            showRoutine = null;
        }

        private IEnumerator TypeText(string text)
        {
            dialogueText.text = "";

            int soundCounter = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char currentChar = text[i];

                dialogueText.text += currentChar;

                bool canPlaySound = true;

                if (skipSpacesForSound && char.IsWhiteSpace(currentChar))
                    canPlaySound = false;

                if (canPlaySound)
                {
                    soundCounter++;

                    if (soundCounter >= Mathf.Max(1, playSoundEveryLetters))
                    {
                        PlayTypingSound();
                        soundCounter = 0;
                    }
                }

                yield return new WaitForSeconds(typeSpeed);
            }
        }

        private void PlayTypingSound()
        {
            if (typingAudioSource == null)
                return;

            if (typingClips == null || typingClips.Length == 0)
                return;

            AudioClip clip = typingClips[Random.Range(0, typingClips.Length)];

            if (clip == null)
                return;

            typingAudioSource.pitch = Random.Range(typingPitchMin, typingPitchMax);
            typingAudioSource.PlayOneShot(clip, typingVolume);
        }

        private void LookAtCamera()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (targetCamera == null)
                return;

            Vector3 direction = dialogueRoot.transform.position - targetCamera.transform.position;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            dialogueRoot.transform.rotation = Quaternion.LookRotation(direction);
        }

        private void HideInstant()
        {
            if (dialogueText != null)
            {
                dialogueText.text = "";
                dialogueText.alpha = 0f;
                dialogueText.transform.localScale = textBaseScale * 0.85f;
                dialogueText.gameObject.SetActive(false);
            }

            if (bubbleVisual != null)
            {
                bubbleVisual.localScale = Vector3.zero;
                bubbleVisual.gameObject.SetActive(false);
            }
        }

        private string GetLine(NPCDialogueIntent intent)
        {
            switch (intent)
            {
                case NPCDialogueIntent.CalmComment:
                    return PickRandom(
                        "Ход.",
                        "Дальше.",
                        "Продолжай.",
                        "Не тяни.",
                        "Быстрее."
                    );

                case NPCDialogueIntent.NervousLie:
                    return PickRandom(
                        "Я справлюсь.",
                        "Ничего не происходит.",
                        "Это под контролем.",
                        "Я не проиграю.",
                        "Только не сейчас."
                    );

                case NPCDialogueIntent.ConfidentTaunt:
                    return PickRandom(
                        "Ты не выйдешь отсюда.",
                        "Ставь.",
                        "Давай дальше.",
                        "Не останавливайся.",
                        "Играй."
                    );

                case NPCDialogueIntent.SuspiciousQuestion:
                    return PickRandom(
                        "Тишина...",
                        "Слишком тихо.",
                        "Они близко.",
                        "Я чувствую это.",
                        "Они рядом."
                    );

                case NPCDialogueIntent.AggressiveThreat:
                    return PickRandom(
                        "Не заставляй меня ждать.",
                        "Ставь.",
                        "Делай ход.",
                        "Живее.",
                        "Давай.",
                        "Быстрее."
                    );

                case NPCDialogueIntent.KnifeRemark:
                    return PickRandom(
                        "Они смотрят через меня.",
                        "Я не помню своё имя.",
                        "Они сказали — играть.",
                        "Я не могу остановиться.",
                        "Если я проиграю — они придут."
                    );

                default:
                    return "";
            }
        }

        private string PickRandom(params string[] lines)
        {
            if (lines == null || lines.Length == 0)
                return "";

            return lines[Random.Range(0, lines.Length)];
        }

        private float EaseOutQuad(float x)
        {
            return 1f - (1f - x) * (1f - x);
        }

        private float EaseInQuad(float x)
        {
            return x * x;
        }

        private float EaseOutBack(float x)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }
    }
}