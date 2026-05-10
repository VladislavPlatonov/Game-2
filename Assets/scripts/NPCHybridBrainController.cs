using System.Collections.Generic;
using UnityEngine;

namespace Poker
{
    // =========================================================
    // 🧠 ГЛАВНЫЙ КОНТРОЛЛЕР ИИ ПРОТИВНИКА
    // Отвечает за:
    // • сбор игровых параметров
    // • передачу данных в нейросеть
    // • получение результата
    // • запуск эмоций, звуков и диалогов
    // =========================================================
    public class NPCHybridBrainController : MonoBehaviour
    {
        // =========================================================
        // 🔗 ССЫЛКИ НА ДРУГИЕ СИСТЕМЫ
        // =========================================================

        [Header("Refs")]

        // Главная игровая система
        [SerializeField] private PokerGame game;

        // Контроллер эмоций NPC
        [SerializeField] private NPCEmotionBridge emotionBridge;

        // Контроллер диалогов NPC
        [SerializeField] private NPCDialogueController dialogueController;

        // =========================================================
        // 🧠 НАСТРОЙКИ НЕЙРОСЕТИ
        // =========================================================

        [Header("Neural Network")]

        // Использовать ли ONNX модель
        [SerializeField] private bool useONNX = true;

        // Объект запуска нейросети
        [SerializeField] private NPCHybridONNXRunner onnxRunner;

        // =========================================================
        // 🔧 ИНИЦИАЛИЗАЦИЯ
        // =========================================================

        private void Awake()
        {
            // Если ссылки не назначены вручную —
            // ищем объекты автоматически на сцене

            if (game == null)
                game = FindFirstObjectByType<PokerGame>();

            if (emotionBridge == null)
                emotionBridge = FindFirstObjectByType<NPCEmotionBridge>();

            if (dialogueController == null)
                dialogueController = FindFirstObjectByType<NPCDialogueController>();

            if (onnxRunner == null)
                onnxRunner = FindFirstObjectByType<NPCHybridONNXRunner>();
        }

        // =========================================================
        // 🎯 ОСНОВНОЙ МЕТОД ПРИНЯТИЯ РЕШЕНИЯ
        // =========================================================

        public NPCHybridBrainResult Decide(int toCall, int aiHp, int bigBlind)
        {
            Debug.Log("🔥 Decide() вызван");

            // =========================================
            // 📦 СОЗДАЁМ ВХОДНЫЕ ДАННЫЕ ДЛЯ ИИ
            // =========================================

            NPCHybridBrainInput input = BuildInput(toCall, aiHp, bigBlind);

            // Переменная для хранения результата
            NPCHybridBrainResult result;

            // =========================================
            // 🧠 ИСПОЛЬЗОВАНИЕ НЕЙРОСЕТИ
            // =========================================

            if (useONNX && onnxRunner != null)
            {
                Debug.Log("🧠 USING ONNX");

                // Отправляем данные в модель
                result = onnxRunner.Predict(input);
            }
            else
            {
                // Если нейросеть выключена —
                // используем резервный ИИ

                Debug.Log("⚠ USING OLD AI");

                result = GetFallback();
            }

            // =========================================
            // 🧾 DEBUG ИНФОРМАЦИЯ
            // =========================================

            Debug.Log("=== AI DECISION ===");
            Debug.Log("Emotion: " + result.emotion);
            Debug.Log("Action: " + result.action);

            // =========================================
            // УСТАНОВКА ЭМОЦИИ NPC
            // =========================================

            if (emotionBridge != null)
                emotionBridge.SetEmotion(result.emotion);

            // =========================================
            // СМЕНА ЗВУКОВОГО СОСТОЯНИЯ
            // =========================================

            if (AudioManager.Instance != null)
                AudioManager.Instance.SetTensionState(MapEmotion(result.emotion));

            // =========================================
            // ПОКАЗ ДИАЛОГА NPC
            // =========================================

            if (dialogueController != null)
                dialogueController.TryShow(
                    result.dialogueIntent,
                    result.dialogueChance,
                    input.aiHandStrength
                );

            // Возвращаем итоговое решение ИИ
            return result;
        }

        // =========================================================
        // 🎭 КОНВЕРТАЦИЯ ЭМОЦИЙ В АУДИО СОСТОЯНИЯ
        // =========================================================

        private AudioManager.TensionState MapEmotion(EnemyHeadCalmController.EmotionState emotion)
        {
            switch (emotion)
            {
                case EnemyHeadCalmController.EmotionState.Calm:
                    return AudioManager.TensionState.Calm;

                case EnemyHeadCalmController.EmotionState.Nervous:
                    return AudioManager.TensionState.Nervous;

                case EnemyHeadCalmController.EmotionState.Panic:
                    return AudioManager.TensionState.Panic;

                case EnemyHeadCalmController.EmotionState.Focus:
                    return AudioManager.TensionState.Focus;

                case EnemyHeadCalmController.EmotionState.Suspicious:
                    return AudioManager.TensionState.Suspicious;

                case EnemyHeadCalmController.EmotionState.Greedy:
                    return AudioManager.TensionState.Greedy;

                case EnemyHeadCalmController.EmotionState.Aggressive:
                    return AudioManager.TensionState.Aggressive;

                case EnemyHeadCalmController.EmotionState.BluffCalm:
                    return AudioManager.TensionState.BluffCalm;

                case EnemyHeadCalmController.EmotionState.BluffNervous:
                    return AudioManager.TensionState.BluffNervous;

                case EnemyHeadCalmController.EmotionState.Unhinged:
                    return AudioManager.TensionState.Unhinged;
            }

            // Если эмоция не определена —
            // возвращаем спокойное состояние

            return AudioManager.TensionState.Calm;
        }

        // =========================================================
        // ⚠ РЕЗЕРВНЫЙ ИИ
        // Используется если нейросеть отключена
        // =========================================================

        private NPCHybridBrainResult GetFallback()
        {
            return new NPCHybridBrainResult
            {
                action = PlayerActionType.Call,
                emotion = EnemyHeadCalmController.EmotionState.Calm,
                dialogueIntent = NPCDialogueIntent.None,
                confidence = 0.5f,
                dialogueChance = 0.2f
            };
        }

        // =========================================================
        // 📦 СОЗДАНИЕ ВХОДНЫХ ДАННЫХ ДЛЯ НЕЙРОСЕТИ
        // =========================================================

        private NPCHybridBrainInput BuildInput(int toCall, int aiHp, int bigBlind)
        {
            // Получаем HP игрока
            // Если game == null → используем 0

            int playerHp = game != null ? game.GetPlayerHP() : 0;

            // Получаем размер банка
            int pot = game != null ? game.GetCurrentPot() : 0;

            // =====================================================
            // 🎴 ОЦЕНКА СИЛЫ РУКИ ИИ
            // =====================================================

            // Реальная оценка силы карт
            float rawHand = EvaluateAIHandStrength();

            // Уменьшаем вероятность сильных рук
            float aiHandStrength = Mathf.Pow(rawHand, 1.7f);

            // =====================================================
            // РАСЧЁТ РИСКА
            // =====================================================

            // Давление из-за необходимости уравнять ставку
            float callPressure =
                toCall <= 0
                ? 0f
                : Mathf.Clamp01((float)toCall / Mathf.Max(1, aiHp));

            // Давление из-за размера банка
            float potPressure =
                Mathf.Clamp01((float)pot / Mathf.Max(1f, aiHp + playerHp));

            // Общий уровень риска
            float riskLevel = Mathf.Clamp01(
                callPressure * 0.7f +
                potPressure * 0.2f +
                (1f - aiHandStrength) * 0.4f
            );

            // =====================================================
            // 🎭 РАСЧЁТ БЛЕФА
            // =====================================================

            // Если рука слабая и риск высокий —
            // шанс блефа увеличивается

            float bluffChance = (aiHandStrength < 0.35f && riskLevel > 0.6f)
                ? 0.25f
                : 0.05f;

            // Случайное значение для определения блефа
            float bluffRoll = Random.value;

            // =====================================================
            // 🧾 DEBUG ИНФОРМАЦИЯ
            // =====================================================

            Debug.Log($"[INPUT] Hand:{aiHandStrength:F2} Risk:{riskLevel:F2} BluffRoll:{bluffRoll:F2}");

            // =====================================================
            // 📦 СОБИРАЕМ ВХОДНЫЕ ДАННЫЕ
            // =====================================================

            return new NPCHybridBrainInput
            {
                aiHandStrength = aiHandStrength,
                riskLevel = riskLevel,
                potPressure = potPressure,
                callPressure = callPressure,

                // Нормализованный HP ИИ
                aiHpNormalized = Mathf.Clamp01(aiHp / 100f),

                // Нормализованный HP игрока
                playerHpNormalized = Mathf.Clamp01(playerHp / 100f),

                // Фактор блефа
                bluffFactor = Random.value,

                // Агрессия игрока
                playerAggression = Mathf.Clamp01((float)game.GetPlayerBetThisRound() / Mathf.Max(1, pot)),

                // Подозрительность игрока
                playerSuspicion = Mathf.Clamp01((float)game.GetCurrentBet() / Mathf.Max(1, pot)),

                // Текущая стадия игры
                isPreflop = 1f,
                isFlop = 0f,
                isTurn = 0f,
                isRiver = 0f,

                // Возможные действия ИИ
                canCheck = toCall == 0 ? 1f : 0f,
                canCall = toCall > 0 ? 1f : 0f,
                canRaise = aiHp > toCall + bigBlind ? 1f : 0f,
                canFold = toCall > 0 ? 1f : 0f,

                // Возможность блефа
                knifeAvailable = bluffRoll < bluffChance ? 1f : 0f,

                // Случайное настроение
                randomMood = Random.value
            };
        }

        // =========================================================
        // 🎴 ОЦЕНКА СИЛЫ РУКИ ИИ
        // =========================================================

        private float EvaluateAIHandStrength()
        {
            // Если игра не найдена —
            // возвращаем среднее значение

            if (game == null)
                return 0.5f;

            // Получаем карты ИИ
            IReadOnlyList<Card> aiHand = game.GetAIHand();

            // Получаем общие карты
            IReadOnlyList<Card> community = game.GetCommunityCards();

            // Если карт недостаточно —
            // возвращаем среднее значение

            if (aiHand == null || aiHand.Count < 2)
                return 0.5f;

            // Если общих карт мало —
            // используем preflop оценку

            if (community == null || community.Count < 3)
                return EvaluatePreflopStrength(aiHand[0], aiHand[1]);

            // Собираем все карты вместе
            List<Card> cards = new List<Card>();

            cards.AddRange(aiHand);
            cards.AddRange(community);

            if (cards.Count < 5)
                return EvaluatePreflopStrength(aiHand[0], aiHand[1]);

            // Оцениваем комбинацию
            HandValue value = HandEvaluator.Evaluate7(cards);

            // Оценка категории комбинации
            float categoryScore = ((int)value.Category - 1) / 8f;

            // Оценка кикера
            float kickerScore = 0f;

            if (value.Tiebreak != null && value.Tiebreak.Count > 0)
                kickerScore = Mathf.Clamp01((value.Tiebreak[0] - 2f) / 12f);

            // Финальная сила руки
            return Mathf.Clamp01(categoryScore * 0.8f + kickerScore * 0.2f);
        }

        // =========================================================
        // 🃏 ОЦЕНКА PREFLOP РУКИ
        // =========================================================

        private float EvaluatePreflopStrength(Card a, Card b)
        {
            // Старшая карта
            int high = Mathf.Max(a.Value, b.Value);

            // Младшая карта
            int low = Mathf.Min(a.Value, b.Value);

            // Проверка на пару
            bool pair = a.Rank == b.Rank;

            // Проверка на одномастность
            bool suited = a.Suit == b.Suit;

            // Разница между картами
            int gap = Mathf.Abs(a.Value - b.Value);

            // Базовая сила
            float score = 0.15f;

            // Сила старшей карты
            score += Mathf.InverseLerp(2f, 14f, high) * 0.35f;

            // Сила младшей карты
            score += Mathf.InverseLerp(2f, 14f, low) * 0.20f;

            // Бонус за пару
            if (pair)
                score += 0.35f;

            // Бонус за одномастность
            if (suited)
                score += 0.08f;

            // Бонус за близость карт
            if (gap <= 1)
                score += 0.08f;
            else if (gap <= 3)
                score += 0.04f;

            // Возвращаем итоговую силу руки
            return Mathf.Clamp01(score);
        }
    }
}