using System.Collections.Generic;
using UnityEngine;

namespace Poker
{
    public class NPCHybridBrainController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PokerGame game;
        [SerializeField] private NPCEmotionBridge emotionBridge;
        [SerializeField] private NPCDialogueController dialogueController;

        [Header("Neural Network")]
        [SerializeField] private bool useONNX = true;
        [SerializeField] private NPCHybridONNXRunner onnxRunner;

        private void Awake()
        {
            if (game == null)
                game = FindFirstObjectByType<PokerGame>();

            if (emotionBridge == null)
                emotionBridge = FindFirstObjectByType<NPCEmotionBridge>();

            if (dialogueController == null)
                dialogueController = FindFirstObjectByType<NPCDialogueController>();

            if (onnxRunner == null)
                onnxRunner = FindFirstObjectByType<NPCHybridONNXRunner>();
        }

        public NPCHybridBrainResult Decide(int toCall, int aiHp, int bigBlind)
        {
            Debug.Log("🔥 Decide() вызван");

            NPCHybridBrainInput input = BuildInput(toCall, aiHp, bigBlind);

            NPCHybridBrainResult result;

            if (useONNX && onnxRunner != null)
            {
                Debug.Log("🧠 USING ONNX");
                result = onnxRunner.Predict(input);
            }
            else
            {
                Debug.Log("⚠ USING OLD AI");
                result = GetFallback();
            }

            Debug.Log("=== AI DECISION ===");
            Debug.Log("Emotion: " + result.emotion);
            Debug.Log("Action: " + result.action);

            if (emotionBridge != null)
                emotionBridge.SetEmotion(result.emotion);

            if (AudioManager.Instance != null)
                AudioManager.Instance.SetTensionState(MapEmotion(result.emotion));

            if (dialogueController != null)
                dialogueController.TryShow(
                    result.dialogueIntent,
                    result.dialogueChance,
                    input.aiHandStrength   // 🔥 ВАЖНО
                );

            return result;
        }

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

            return AudioManager.TensionState.Calm;
        }
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

        // ============================================
        // 🔥 ГЛАВНАЯ ЛОГИКА — ВХОД ДЛЯ НЕЙРОНКИ
        // ============================================
        private NPCHybridBrainInput BuildInput(int toCall, int aiHp, int bigBlind)
        {
            int playerHp = game != null ? game.GetPlayerHP() : 0;
            int pot = game != null ? game.GetCurrentPot() : 0;

            // =========================
            // 🎴 СИЛА РУКИ (НЕ РАНДОМ!)
            // =========================

            float rawHand = EvaluateAIHandStrength();

            // 🔥 делаем сильные руки реже (очень важно)
            float aiHandStrength = Mathf.Pow(rawHand, 1.7f);

            // =========================
            // 💀 РИСК
            // =========================

            float callPressure = toCall <= 0 ? 0f : Mathf.Clamp01((float)toCall / Mathf.Max(1, aiHp));
            float potPressure = Mathf.Clamp01((float)pot / Mathf.Max(1f, aiHp + playerHp));

            float riskLevel = Mathf.Clamp01(
                callPressure * 0.7f +
                potPressure * 0.2f +
                (1f - aiHandStrength) * 0.4f
            );

            // =========================
            // 🎭 БЛЕФ (редкий)
            // =========================

            float bluffChance = (aiHandStrength < 0.35f && riskLevel > 0.6f)
                ? 0.25f   // только в плохой ситуации
                : 0.05f;

            float bluffRoll = Random.value;

            // =========================
            // 🧠 ЛОГ ДЛЯ ДЕБАГА
            // =========================

            Debug.Log($"[INPUT] Hand:{aiHandStrength:F2} Risk:{riskLevel:F2} BluffRoll:{bluffRoll:F2}");

            // =========================
            // 📦 СОБИРАЕМ INPUT
            // =========================

            return new NPCHybridBrainInput
            {
                aiHandStrength = aiHandStrength,
                riskLevel = riskLevel,
                potPressure = potPressure,
                callPressure = callPressure,

                aiHpNormalized = Mathf.Clamp01(aiHp / 100f),
                playerHpNormalized = Mathf.Clamp01(playerHp / 100f),

                bluffFactor = Random.value,

                playerAggression = Mathf.Clamp01((float)game.GetPlayerBetThisRound() / Mathf.Max(1, pot)),
                playerSuspicion = Mathf.Clamp01((float)game.GetCurrentBet() / Mathf.Max(1, pot)),

                isPreflop = 1f,
                isFlop = 0f,
                isTurn = 0f,
                isRiver = 0f,

                canCheck = toCall == 0 ? 1f : 0f,
                canCall = toCall > 0 ? 1f : 0f,
                canRaise = aiHp > toCall + bigBlind ? 1f : 0f,
                canFold = toCall > 0 ? 1f : 0f,

                knifeAvailable = bluffRoll < bluffChance ? 1f : 0f,
                randomMood = Random.value
            };
        }

        // ============================================
        // 🎴 ОЦЕНКА РУКИ
        // ============================================
        private float EvaluateAIHandStrength()
        {
            if (game == null)
                return 0.5f;

            IReadOnlyList<Card> aiHand = game.GetAIHand();
            IReadOnlyList<Card> community = game.GetCommunityCards();

            if (aiHand == null || aiHand.Count < 2)
                return 0.5f;

            if (community == null || community.Count < 3)
                return EvaluatePreflopStrength(aiHand[0], aiHand[1]);

            List<Card> cards = new List<Card>();
            cards.AddRange(aiHand);
            cards.AddRange(community);

            if (cards.Count < 5)
                return EvaluatePreflopStrength(aiHand[0], aiHand[1]);

            HandValue value = HandEvaluator.Evaluate7(cards);

            float categoryScore = ((int)value.Category - 1) / 8f;

            float kickerScore = 0f;
            if (value.Tiebreak != null && value.Tiebreak.Count > 0)
                kickerScore = Mathf.Clamp01((value.Tiebreak[0] - 2f) / 12f);

            return Mathf.Clamp01(categoryScore * 0.8f + kickerScore * 0.2f);
        }

        private float EvaluatePreflopStrength(Card a, Card b)
        {
            int high = Mathf.Max(a.Value, b.Value);
            int low = Mathf.Min(a.Value, b.Value);

            bool pair = a.Rank == b.Rank;
            bool suited = a.Suit == b.Suit;
            int gap = Mathf.Abs(a.Value - b.Value);

            float score = 0.15f;

            score += Mathf.InverseLerp(2f, 14f, high) * 0.35f;
            score += Mathf.InverseLerp(2f, 14f, low) * 0.20f;

            if (pair)
                score += 0.35f;

            if (suited)
                score += 0.08f;

            if (gap <= 1)
                score += 0.08f;
            else if (gap <= 3)
                score += 0.04f;

            return Mathf.Clamp01(score);
        }
    }
}