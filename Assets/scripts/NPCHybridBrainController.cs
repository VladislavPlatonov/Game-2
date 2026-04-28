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

        [Header("AI Personality")]
        [Range(0f, 1f)][SerializeField] private float aggression = 0.35f;
        [Range(0f, 1f)][SerializeField] private float bluffChance = 0.25f;
        [Range(0f, 1f)][SerializeField] private float weirdness = 0.08f;

        [Header("Debug")]
        [SerializeField] private bool debugLogBrain;

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

        private void Start()
        {
            
        }

        public NPCHybridBrainResult Decide(int toCall, int aiHp, int bigBlind)
        {
            

            NPCHybridBrainInput input = BuildInput(toCall, aiHp, bigBlind);

            NPCHybridBrainResult result;

            if (useONNX && onnxRunner != null)
            {
                result = onnxRunner.Predict(input);
            }
            else
            {
                result = PredictHybrid(input);
            }

            result.action = ValidateAction(result.action, input);

            if (result.action == PlayerActionType.Raise)
                result.raiseTo = CalculateRaiseAmount(input);

            NPCTrainingDataRecorder.SaveExample(input, result);
            

            if (emotionBridge != null)
                emotionBridge.SetEmotion(result.emotion);

            if (dialogueController != null)
                dialogueController.TryShow(result.dialogueIntent, result.dialogueChance);

            if (debugLogBrain)
            {
                Debug.Log(
                    $"[NPC Brain] Action: {result.action}, Emotion: {result.emotion}, Dialogue: {result.dialogueIntent}, " +
                    $"Hand: {input.aiHandStrength:0.00}, Risk: {input.riskLevel:0.00}, CallPressure: {input.callPressure:0.00}"
                );
            }

            return result;
        }

        private NPCHybridBrainInput BuildInput(int toCall, int aiHp, int bigBlind)
        {
            int playerHp = game != null ? game.GetPlayerHP() : 0;
            int pot = game != null ? game.GetCurrentPot() : 0;

            GameState state = game != null ? game.GetCurrentState() : GameState.None;

            float maxHp = Mathf.Max(1f, aiHp + playerHp + pot);

            float aiHandStrength = EvaluateAIHandStrength();

            float callPressure = toCall <= 0 ? 0f : Mathf.Clamp01((float)toCall / Mathf.Max(1, aiHp));
            float potPressure = Mathf.Clamp01((float)pot / Mathf.Max(1f, maxHp));

            float riskLevel = Mathf.Clamp01(
                callPressure * 0.55f +
                potPressure * 0.25f +
                (1f - aiHandStrength) * 0.20f
            );

            return new NPCHybridBrainInput
            {
                aiHandStrength = aiHandStrength,
                riskLevel = riskLevel,
                potPressure = potPressure,
                callPressure = callPressure,

                aiHpNormalized = Mathf.Clamp01(aiHp / 100f),
                playerHpNormalized = Mathf.Clamp01(playerHp / 100f),

                playerAggression = EstimatePlayerAggression(),
                playerSuspicion = EstimatePlayerSuspicion(),

                isPreflop = state == GameState.Preflop ? 1f : 0f,
                isFlop = state == GameState.Flop ? 1f : 0f,
                isTurn = state == GameState.Turn ? 1f : 0f,
                isRiver = state == GameState.River ? 1f : 0f,

                canCheck = toCall == 0 ? 1f : 0f,
                canCall = toCall > 0 && aiHp > 0 ? 1f : 0f,
                canRaise = aiHp > toCall + bigBlind ? 1f : 0f,
                canFold = toCall > 0 ? 1f : 0f,

                knifeAvailable = Random.value < weirdness ? 1f : 0f,
                randomMood = Random.value,

                toCall = toCall,
                aiHp = aiHp,
                currentPot = pot,
                bigBlind = bigBlind
            };
        }

        private NPCHybridBrainResult PredictHybrid(NPCHybridBrainInput input)
        {
            // Позже здесь будет ONNX:
            // result = onnxModel.Predict(inputFeatures);

            bool wantsBluff = Random.value < bluffChance;
            bool wantsWeird = input.knifeAvailable > 0.5f && Random.value < weirdness;

            NPCHybridBrainResult result = new NPCHybridBrainResult
            {
                action = PlayerActionType.Check,
                raiseTo = 0,
                emotion = EnemyHeadCalmController.EmotionState.Calm,
                dialogueIntent = NPCDialogueIntent.None,
                confidence = 0.7f,
                dialogueChance = 0.25f
            };

            if (wantsWeird)
            {
                result.emotion = EnemyHeadCalmController.EmotionState.Unhinged;
                result.dialogueIntent = NPCDialogueIntent.KnifeRemark;
                result.dialogueChance = 0.45f;

                result.action = input.canCheck > 0.5f ? PlayerActionType.Check : PlayerActionType.Call;
                return result;
            }

            if (wantsBluff)
            {
                if (input.aiHandStrength < 0.38f)
                {
                    result.emotion = EnemyHeadCalmController.EmotionState.BluffCalm;
                    result.dialogueIntent = NPCDialogueIntent.CalmComment;
                    result.dialogueChance = 0.35f;

                    if (input.canRaise > 0.5f && Random.value < aggression + 0.25f)
                        result.action = PlayerActionType.Raise;
                    else if (input.canCheck > 0.5f)
                        result.action = PlayerActionType.Check;
                    else
                        result.action = PlayerActionType.Call;

                    return result;
                }

                if (input.aiHandStrength > 0.65f)
                {
                    result.emotion = EnemyHeadCalmController.EmotionState.BluffNervous;
                    result.dialogueIntent = NPCDialogueIntent.NervousLie;
                    result.dialogueChance = 0.45f;

                    if (input.canRaise > 0.5f && Random.value < aggression)
                        result.action = PlayerActionType.Raise;
                    else if (input.canCheck > 0.5f)
                        result.action = PlayerActionType.Check;
                    else
                        result.action = PlayerActionType.Call;

                    return result;
                }
            }

            if (input.riskLevel > 0.82f && input.aiHandStrength < 0.35f)
            {
                result.emotion = EnemyHeadCalmController.EmotionState.Panic;
                result.dialogueIntent = NPCDialogueIntent.NervousLie;
                result.dialogueChance = 0.25f;

                result.action = input.canFold > 0.5f ? PlayerActionType.Fold : PlayerActionType.Check;
                return result;
            }

            if (input.riskLevel > 0.58f)
            {
                result.emotion = EnemyHeadCalmController.EmotionState.Nervous;
                result.dialogueIntent = NPCDialogueIntent.NervousLie;
                result.dialogueChance = 0.25f;

                if (input.canCall > 0.5f)
                    result.action = PlayerActionType.Call;
                else
                    result.action = PlayerActionType.Check;

                return result;
            }

            if (input.aiHandStrength > 0.78f)
            {
                result.emotion = EnemyHeadCalmController.EmotionState.Greedy;
                result.dialogueIntent = NPCDialogueIntent.ConfidentTaunt;
                result.dialogueChance = 0.35f;

                if (input.canRaise > 0.5f && Random.value < 0.65f + aggression * 0.25f)
                    result.action = PlayerActionType.Raise;
                else if (input.canCall > 0.5f)
                    result.action = PlayerActionType.Call;
                else
                    result.action = PlayerActionType.Check;

                return result;
            }

            if (input.playerAggression > 0.65f || input.playerSuspicion > 0.65f)
            {
                result.emotion = EnemyHeadCalmController.EmotionState.Suspicious;
                result.dialogueIntent = NPCDialogueIntent.SuspiciousQuestion;
                result.dialogueChance = 0.30f;

                result.action = input.canCheck > 0.5f ? PlayerActionType.Check : PlayerActionType.Call;
                return result;
            }

            if (input.aiHandStrength > 0.55f && input.canRaise > 0.5f && Random.value < aggression * 0.45f)
            {
                result.emotion = EnemyHeadCalmController.EmotionState.Aggressive;
                result.dialogueIntent = NPCDialogueIntent.AggressiveThreat;
                result.dialogueChance = 0.35f;
                result.action = PlayerActionType.Raise;
                return result;
            }

            result.emotion = EnemyHeadCalmController.EmotionState.Calm;
            result.dialogueIntent = NPCDialogueIntent.CalmComment;
            result.dialogueChance = 0.15f;

            if (input.canCheck > 0.5f)
                result.action = PlayerActionType.Check;
            else
                result.action = PlayerActionType.Call;

            return result;
        }

        private PlayerActionType ValidateAction(PlayerActionType action, NPCHybridBrainInput input)
        {
            if (input.aiHp <= 0)
                return PlayerActionType.Fold;

            if (action == PlayerActionType.Check && input.canCheck < 0.5f)
                return input.canCall > 0.5f ? PlayerActionType.Call : PlayerActionType.Fold;

            if (action == PlayerActionType.Call && input.canCall < 0.5f)
                return input.canCheck > 0.5f ? PlayerActionType.Check : PlayerActionType.Fold;

            if (action == PlayerActionType.Raise && input.canRaise < 0.5f)
                return input.canCall > 0.5f ? PlayerActionType.Call : PlayerActionType.Check;

            if (action == PlayerActionType.Fold && input.canFold < 0.5f)
                return PlayerActionType.Check;

            return action;
        }

        private int CalculateRaiseAmount(NPCHybridBrainInput input)
        {
            int min = Mathf.Min(input.aiHp, input.toCall + input.bigBlind);
            int max = input.aiHp;

            if (min >= max)
                return input.aiHp;

            float strengthBonus = Mathf.Lerp(0.2f, 1f, input.aiHandStrength);
            int extra = Mathf.RoundToInt(input.bigBlind * Mathf.Lerp(1f, 3f, strengthBonus));

            return Mathf.Clamp(min + extra, min, max);
        }

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

        private float EstimatePlayerAggression()
        {
            if (game == null)
                return 0.5f;

            int playerBet = game.GetPlayerBetThisRound();
            int pot = Mathf.Max(1, game.GetCurrentPot());

            return Mathf.Clamp01((float)playerBet / pot);
        }

        private float EstimatePlayerSuspicion()
        {
            if (game == null)
                return 0.35f;

            int currentBet = game.GetCurrentBet();
            int pot = Mathf.Max(1, game.GetCurrentPot());

            return Mathf.Clamp01((float)currentBet / pot);
        }
    }
}