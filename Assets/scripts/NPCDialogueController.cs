using System.Collections;
using TMPro;
using UnityEngine;

namespace Poker
{
    public class NPCDialogueController : MonoBehaviour
    {
        // =========================================================
        // 🔗 ССЫЛКИ (назначаются в инспекторе)
        // =========================================================
        [Header("3D Dialogue Refs")]
        [SerializeField] private GameObject dialogueRoot;
        [SerializeField] private Transform bubbleVisual;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Camera targetCamera;

        // =========================================================
        // ⏱ НАСТРОЙКИ АНИМАЦИИ
        // =========================================================
        [Header("Animation")]
        [SerializeField] private float appearDuration = 0.3f;
        [SerializeField] private float textStartDelay = 0.05f;
        [SerializeField] private float typeSpeed = 0.025f;
        [SerializeField] private float showDuration = 2.5f;
        [SerializeField] private float disappearDuration = 0.35f;

        [Header("Typing Sound")]
        [SerializeField] private AudioSource typingAudioSource;
        [SerializeField] private AudioClip[] typingClips;

        [SerializeField] private float typingVolume = 0.6f;
        [SerializeField] private float pitchMin = 0.9f;
        [SerializeField] private float pitchMax = 1.1f;

        [SerializeField] private int playSoundEveryLetters = 1; // 1 = каждая буква
        [SerializeField] private bool skipSpaces = true;


        // =========================================================
        // 🎈 ПЛАВАЮЩИЙ ЭФФЕКТ
        // =========================================================
        [Header("Motion")]
        [SerializeField] private float floatAmount = 0.025f;
        [SerializeField] private float floatSpeed = 1.6f;
        [SerializeField] private bool lookAtCamera = true;

        // =========================================================
        // 🔒 КОНТРОЛЬ СОСТОЯНИЯ (ВАЖНО!)
        // =========================================================
        private Coroutine showRoutine;

        private bool isShowing = false;   // уже идёт диалог
        private bool isStarting = false;  // ❗ диалог только запускается (очень важно)

        private Vector3 rootBaseLocalPosition;
        private Vector3 bubbleBaseScale;

        // =========================================================
        // 🔧 ИНИЦИАЛИЗАЦИЯ
        // =========================================================
        private void Awake()
        {
            if (dialogueRoot == null)
                dialogueRoot = gameObject;

            if (bubbleVisual == null)
                bubbleVisual = dialogueRoot.transform;

            if (targetCamera == null)
                targetCamera = Camera.main;

            rootBaseLocalPosition = dialogueRoot.transform.localPosition;

            if (bubbleVisual != null)
                bubbleBaseScale = bubbleVisual.localScale;

            if (dialogueText == null)
                Debug.LogError("❌ DialogueText НЕ назначен!");
            else
                Debug.Log("✅ DialogueText: " + dialogueText.name);

            HideInstant();
        }

        // =========================================================
        // 🎥 ПОВОРОТ + ПЛАВАНИЕ
        // =========================================================
        private void LateUpdate()
        {
            float y = Mathf.Sin(Time.time * floatSpeed) * floatAmount;

            dialogueRoot.transform.localPosition =
                rootBaseLocalPosition + new Vector3(0f, y, 0f);

            if (lookAtCamera)
                LookAtCamera();
        }

        // =========================================================
        // 🎯 ВЫЗОВ ДИАЛОГА
        // =========================================================
        public void TryShow(NPCDialogueIntent intent, float chance, float handStrength)
        {
            
            // ❗ блокируем ВСЁ (и запуск, и показ)
            if (isShowing || isStarting)
                return;

            if (intent == NPCDialogueIntent.None)
                return;

            if (Random.value > chance)
                return;

            string line = GetLine(intent, handStrength);

            if (string.IsNullOrWhiteSpace(line))
                return;

            Show(line);
        }

        private void Show(string text)
        {
            // ❗ СРАЗУ блокируем запуск (это ключевой фикс)
            isStarting = true;

            if (showRoutine != null)
                StopCoroutine(showRoutine);

            showRoutine = StartCoroutine(ShowRoutine(text));
        }

        // =========================================================
        // 🎬 АНИМАЦИЯ ПОКАЗА
        // =========================================================
        private IEnumerator ShowRoutine(string text)
        {
            yield return new WaitForSeconds(Random.Range(0.2f, 0.4f));

            // ❗ теперь диалог официально начался
            isStarting = false;
            isShowing = true;

            bubbleVisual.gameObject.SetActive(true);
            dialogueText.gameObject.SetActive(true);

            // ❗ ВАЖНО: текст НЕ задаём сразу
            dialogueText.text = "";
            dialogueText.alpha = 0f;

            // ❗ фикс TMP (убирает "мигание")
            Canvas.ForceUpdateCanvases();

            bubbleVisual.localScale = Vector3.zero;

            float t = 0f;

            // =========================================================
            // 🎈 ПОЯВЛЕНИЕ ПУЗЫРЯ
            // =========================================================
            while (t < 1f)
            {
                t += Time.deltaTime / appearDuration;

                float k = EaseOutBack(Mathf.Clamp01(t));

                bubbleVisual.localScale =
                    Vector3.LerpUnclamped(Vector3.zero, bubbleBaseScale, k);

                yield return null;
            }

            yield return new WaitForSeconds(textStartDelay);

            t = 0f;

            // =========================================================
            // ✍️ ПЛАВНОЕ ПОЯВЛЕНИЕ ТЕКСТА (НО БЕЗ САМОГО ТЕКСТА)
            // =========================================================
            while (t < 1f)
            {
                t += Time.deltaTime / 0.25f;

                float k = EaseOutQuad(Mathf.Clamp01(t));

                dialogueText.alpha = k;

                yield return null;
            }

            // =========================================================
            // ⌨️ ПЕЧАТЬ (ТОЛЬКО ТУТ ПОЯВЛЯЕТСЯ ТЕКСТ)
            // =========================================================
            yield return StartCoroutine(TypeText(text));

            yield return new WaitForSeconds(showDuration);

            t = 0f;

            // =========================================================
            // 🌫 ИСЧЕЗНОВЕНИЕ
            // =========================================================
            while (t < 1f)
            {
                t += Time.deltaTime / disappearDuration;

                float k = Mathf.Clamp01(t);

                float bubbleEase = EaseInOutCubic(k);
                float textEase = EaseOutQuad(k);

                dialogueText.alpha = 1f - textEase;

                bubbleVisual.localScale =
                    Vector3.LerpUnclamped(bubbleBaseScale, Vector3.zero, bubbleEase);

                yield return null;
            }

            HideInstant();

            showRoutine = null;
            isShowing = false;
        }

        // =========================================================
        // ⌨️ ПЕЧАТЬ ТЕКСТА
        // =========================================================
        private IEnumerator TypeText(string text)
        {
            dialogueText.text = "";

            int soundCounter = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                dialogueText.text += c;

                bool canPlay = true;

                // ❗️ пропускаем пробелы
                if (skipSpaces && char.IsWhiteSpace(c))
                    canPlay = false;

                if (canPlay)
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


        // =========================================================
        // 🎥 LOOK AT CAMERA
        // =========================================================
        private void LookAtCamera()
        {
            Vector3 dir =
                dialogueRoot.transform.position - targetCamera.transform.position;

            if (dir.sqrMagnitude > 0.001f)
                dialogueRoot.transform.rotation = Quaternion.LookRotation(dir);
        }

        // =========================================================
        // ❌ СКРЫТИЕ
        // =========================================================
        private void HideInstant()
        {
            dialogueText.alpha = 0f;
            dialogueText.text = "";
            dialogueText.gameObject.SetActive(false);

            bubbleVisual.localScale = Vector3.zero;
            bubbleVisual.gameObject.SetActive(false);
        }

        // =========================================================
        // 🧠 ЛОГИКА ДИАЛОГОВ
        // =========================================================
        private string GetLine(NPCDialogueIntent intent, float hand)
        {
            switch (intent)
            {
                // =========================================
                // 😐 СПОКОЙНЫЙ
                // =========================================
                case NPCDialogueIntent.CalmComment:
                    return PickRandom(
                        "Хм...",
                        "Посмотрим.",
                        "Интересно.",
                        "Не спеши.",
                        "Я подожду.",
                        "Продолжай.",
                        "Игра только начинается.",
                        "Ничего необычного.",
                        "Ты уверен?",
                        "Я наблюдаю.",
                        "Пока всё тихо.",
                        "Делай ход.",
                        "Любопытно.",
                        "Ты думаешь?",
                        "Ход за тобой."
                    );

                // =========================================
                // 😰 НЕРВНЫЙ
                // =========================================
                case NPCDialogueIntent.NervousLie:
                    return PickRandom(
                        "Всё нормально...",
                        "Я контролирую ситуацию...",
                        "Это просто совпадение...",
                        "Не так уж и плохо...",
                        "Я ещё в игре...",
                        "Спокойно...",
                        "Я не проиграю...",
                        "Это не проблема...",
                        "Мне просто не везёт...",
                        "Сейчас всё изменится...",
                        "Да, всё нормально...",
                        "Я справлю ситуацию...",
                        "Не сейчас...",
                        "Только не это...",
                        "Я держусь..."
                    );

                // =========================================
                // 😏 УВЕРЕННЫЙ / ПРОВОКАЦИЯ
                // =========================================
                case NPCDialogueIntent.ConfidentTaunt:

                    if (hand > 0.75f)
                        return PickRandom(
                            "Ты уже проиграл.",
                            "Это конец.",
                            "Я ждал этого.",
                            "Ты попал.",
                            "Слишком поздно.",
                            "Ты не выберешься.",
                            "Ох, это была ошибка.",
                            "Я вижу тебя насквозь.",
                            "Давай, попробуй.",
                            "Ну же, ходи...",
                            "У тебя нет шансов.",
                            "Я веду эту игру.",
                            "Ты в ловушке.",
                            "Это мой раунд.",
                            "Заканчивай."
                        );
                    else
                        return PickRandom(
                            "Рискнёшь?",
                            "Давай, покажи мощь.",
                            "Интересный ход...",
                            "Посмотрим...",
                            "Ну давай.",
                            "Не бойся.",
                            "Ты колеблешься.",
                            "Продолжай.",
                            "Слабовато.",
                            "И это всё?",
                            "Хочешь рискнуть?",
                            "Ты сомневаешься.",
                            "Я жду.",
                            "Давай дальше.",
                            "Сделай ход."
                        );

                // =========================================
                // 🧐 ПОДОЗРЕНИЕ
                // =========================================
                case NPCDialogueIntent.SuspiciousQuestion:
                    return PickRandom(
                        "Странно...",
                        "Не верю.",
                        "Ты что-то скрываешь.",
                        "Слишком чисто играешь.",
                        "Подозрительно.",
                        "Ты блефуешь?",
                        "Что-то не сходится.",
                        "Это выглядит не честно.",
                        "Ты уверен в этом?",
                        "Я чувствую что-то не то...",
                        "Ты ведёшь себя странно.",
                        "Ты какой-то тихий...",
                        "Что ты задумал?",
                        "Это ловушка?",
                        "Ты пытаешься мухлевать?"
                    );

                // =========================================
                // 😡 АГРЕССИЯ
                // =========================================
                case NPCDialogueIntent.AggressiveThreat:
                    return PickRandom(
                        "Играй уже.",
                        "БЫСТРЕЕ.",
                        "Не тяни время.",
                        "Ставь.",
                        "ДАВАЙ!",
                        "ЖИВЕЕ!",
                        "Хватит думать.",
                        "Ты меня утомил.",
                        "Делай ход.",
                        "Не испытывай мой терпение.",
                        "Сколько можно?",
                        "Давай решайся.",
                        "Я не буду вечно ждать.",
                        "Мы играем?",
                        "Давай уже дальше."
                    );

                // =========================================
                // 🔪 ТЁМНЫЙ / ДАВЯЩИЙ
                // =========================================
                case NPCDialogueIntent.KnifeRemark:
                    return PickRandom(
                        "Сегодня кто-то проиграет всё...",
                        "Мне это нравится...",
                        "Риск — это игра на смерть.",
                        "Я не остановлюсь.",
                        "Кто-то уйдёт ни с чем.",
                        "Ты чувствуешь это?",
                        "Ставки растут.",
                        "Я вижу, что-то грядёт.",
                        "Это только начало.",
                        "Назад дороги нет.",
                        "Ты уже зашёл слишком далеко.",
                        "Это конец для кого-то.",
                        "Игра стала серьёзной.",
                        "Теперь всё по-настоящему.",
                        "Ты слышишь это?"
                    );

                // =========================================
                // 😎 СПОКОЙНЫЙ БЛЕФ
                // =========================================
                case NPCDialogueIntent.BluffCalm:
                    return PickRandom(
                        "У меня сильная рука.",
                        "Лучше сбрось.",
                        "Ты уверен?",
                        "Я бы не рисковал.",
                        "Это плохая идея.",
                        "Подумай ещё раз.",
                        "Ты совершаешь ошибку.",
                        "Не стоит продолжать.",
                        "Я бы остановился.",
                        "Ты заходишь слишком далеко.",
                        "Это закончится плохо.",
                        "Поверь мне.",
                        "Я предупреждал.",
                        "Ещё не поздно выйти из игры.",
                        "Не делай этого."
                    );

                // =========================================
                // 😰 НЕРВНЫЙ БЛЕФ
                // =========================================
                case NPCDialogueIntent.BluffNervous:
                    return PickRandom(
                        "Ты уверен?..",
                        "Это плохая идея...",
                        "Может... не стоит?",
                        "Ты точно хочешь это сделать?",
                        "Я бы не стал...",
                        "Это рискованно...",
                        "Хорошо подумай...",
                        "Сейчас не время...",
                        "Не надо...",
                        "Ты можешь пожалеть...",
                        "Лучше остановись...",
                        "Это закончится печально...",
                        "Я серьёзно...",
                        "Не делай этого...",
                        "Подумай ещё раз..."
                    );

                default:
                    return "";
            }
        }

        private string PickRandom(params string[] lines)
        {
            return lines[Random.Range(0, lines.Length)];
        }

        // =========================================================
        // 🎨 ПЛАВНЫЕ АНИМАЦИИ
        // =========================================================
        private float EaseOutQuad(float x)
        {
            return 1f - (1f - x) * (1f - x);
        }

        private float EaseInOutCubic(float x)
        {
            return x < 0.5f
                ? 4f * x * x * x
                : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
        }

        private float EaseOutBack(float x)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }
        private void PlayTypingSound()
        {
            if (typingAudioSource == null)
                return;

            if (typingClips == null || typingClips.Length == 0)
                return;

            AudioClip clip = typingClips[Random.Range(0, typingClips.Length)];

            typingAudioSource.pitch = Random.Range(pitchMin, pitchMax);
            typingAudioSource.PlayOneShot(clip, typingVolume);
        }

    }
}