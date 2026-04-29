using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Poker
{
    public class PokerGame : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int smallBlind = 2;
        [SerializeField] private int bigBlind = 4;
        [SerializeField] private float aiThinkDelay = 1.0f;
        [SerializeField] private float nextHandDelay = 1.2f;

        [Header("Game Over")]
        [SerializeField] private int winHpTarget = 200;

        [Header("New Game")]
        [SerializeField] private bool resetSoulsOnFreshStart = true;

        [Header("Refs")]
        [SerializeField] private BasicPokerAI ai;
        [SerializeField] private NPCHybridBrainController hybridBrain;

        public event Action OnCardsChanged;
        public event Action OnGameStateChanged;
        public event Action OnPotUpdated;
        public event Action<string> OnActionLog;
        public event Action<bool> OnTurnChanged;

        private PokerDeck deck;

        private readonly List<Card> playerHand = new List<Card>(2);
        private readonly List<Card> aiHand = new List<Card>(2);
        private readonly List<Card> community = new List<Card>(5);

        private int pot;
        private int currentBet;
        private int playerBetThisRound;
        private int aiBetThisRound;

        private bool waitingForPlayer;
        private bool handInProgress;
        private bool playerFolded;
        private bool aiFolded;

        private bool playerIsDealer = true;

        private GameState state = GameState.None;
        private string gameOverMessage = "";

        private PokerUnifiedSaveData pendingLoadedSave;

        private void Awake()
        {
            deck = new PokerDeck();

            if (ai == null)
                ai = FindFirstObjectByType<BasicPokerAI>();

            if (hybridBrain == null)
                hybridBrain = FindFirstObjectByType<NPCHybridBrainController>();
        }

        private void Start()
        {
            if (pendingLoadedSave != null)
            {
                LoadUnifiedSave(pendingLoadedSave);
                pendingLoadedSave = null;
                return;
            }

            if (resetSoulsOnFreshStart && SoulManager.Instance != null)
                SoulManager.Instance.NewGameReset();

            StartNewHand();
        }

        public void PreparePendingLoad(PokerUnifiedSaveData data)
        {
            pendingLoadedSave = data;
        }

        public IReadOnlyList<Card> GetPlayerHand() => playerHand;
        public IReadOnlyList<Card> GetAIHand() => aiHand;
        public IReadOnlyList<Card> GetCommunityCards() => community;

        public GameState GetCurrentState() => state;
        public int GetCurrentPot() => pot;
        public int GetCurrentBet() => currentBet;
        public int GetPlayerBetThisRound() => playerBetThisRound;
        public int GetAIBetThisRound() => aiBetThisRound;

        public bool IsWaitingForPlayerAction() => waitingForPlayer;

        public int GetPlayerHP() => SoulManager.Instance != null ? SoulManager.Instance.GetPlayerSouls() : 0;
        public int GetAIHP() => SoulManager.Instance != null ? SoulManager.Instance.GetAISouls() : 0;
        public bool GetPlayerIsDealer() => playerIsDealer;

        public string GetGameOverMessage()
        {
            return gameOverMessage;
        }

        // =========================
        // PLAYER ACTIONS
        // =========================

        public void PlayerFold()
        {
            if (!CanPlayerAct()) return;

            playerFolded = true;
            waitingForPlayer = false;

            OnTurnChanged?.Invoke(false);
            LogAction("Игрок: сброс");

            ResolveHandByFold();
        }

        public void PlayerCheck()
        {
            if (!CanPlayerAct()) return;
            if (GetToCallForPlayer() != 0) return;

            waitingForPlayer = false;
            OnTurnChanged?.Invoke(false);

            LogAction("Игрок: чек");

            StartCoroutine(AITurnThenAdvance());
        }

        public void PlayerCall()
        {
            if (!CanPlayerAct()) return;

            int toCall = GetToCallForPlayer();

            if (toCall <= 0)
            {
                PlayerCheck();
                return;
            }

            int paid = PayPlayer(toCall);

            waitingForPlayer = false;
            OnTurnChanged?.Invoke(false);

            LogAction($"Игрок: колл {paid} HP");

            OnPotUpdated?.Invoke();
            OnGameStateChanged?.Invoke();

            StartCoroutine(AITurnThenAdvance());
        }

        public void PlayerRaise(int amount)
        {
            if (!CanPlayerAct()) return;

            int toCall = GetToCallForPlayer();
            int hp = GetPlayerHP();

            amount = Mathf.Clamp(amount, 0, hp + playerBetThisRound);
            if (amount <= currentBet) return;

            int paidTotal = 0;

            if (toCall > 0)
                paidTotal += PayPlayer(toCall);

            int extraNeeded = amount - playerBetThisRound;

            if (extraNeeded > 0)
                paidTotal += PayPlayer(extraNeeded);

            currentBet = Mathf.Max(currentBet, playerBetThisRound);

            waitingForPlayer = false;
            OnTurnChanged?.Invoke(false);

            LogAction($"Игрок: повысил до {currentBet} HP");

            OnPotUpdated?.Invoke();
            OnGameStateChanged?.Invoke();

            StartCoroutine(AITurnThenAdvance());
        }

        public void PlayerAllIn()
        {
            if (!CanPlayerAct()) return;

            int hp = GetPlayerHP();
            if (hp <= 0) return;

            int paid = PayPlayer(hp);
            currentBet = Mathf.Max(currentBet, playerBetThisRound);

            waitingForPlayer = false;
            OnTurnChanged?.Invoke(false);

            LogAction($"Игрок: ва-банк ({paid} HP)");

            OnPotUpdated?.Invoke();
            OnGameStateChanged?.Invoke();

            StartCoroutine(AITurnThenAdvance());
        }

        // =========================
        // CORE
        // =========================

        private void StartNewHand(bool postBlinds = true)
        {
            if (SoulManager.Instance == null)
            {
                Debug.LogError("[PokerGame] SoulManager missing in scene.");
                return;
            }

            if (CheckGameOverByHP())
                return;

            StopAllCoroutines();

            deck.ResetAndShuffle();

            playerHand.Clear();
            aiHand.Clear();
            community.Clear();

            pot = 0;
            currentBet = 0;
            playerBetThisRound = 0;
            aiBetThisRound = 0;

            playerFolded = false;
            aiFolded = false;

            handInProgress = true;
            waitingForPlayer = false;
            gameOverMessage = "";

            state = GameState.Preflop;

            playerHand.Add(deck.Draw());
            aiHand.Add(deck.Draw());
            playerHand.Add(deck.Draw());
            aiHand.Add(deck.Draw());

            if (postBlinds)
            {
                PostBlinds();

                LogAction("Новая раздача");
                LogAction(playerIsDealer
                    ? $"Игрок: малый блайнд {smallBlind} HP"
                    : $"Противник: малый блайнд {smallBlind} HP");

                LogAction(playerIsDealer
                    ? $"Противник: большой блайнд {bigBlind} HP"
                    : $"Игрок: большой блайнд {bigBlind} HP");

                OnPotUpdated?.Invoke();
            }
            else
            {
                LogAction("Сейв загружен");
            }

            waitingForPlayer = true;

            OnCardsChanged?.Invoke();
            OnPotUpdated?.Invoke();
            OnGameStateChanged?.Invoke();
            OnTurnChanged?.Invoke(true);

            LogPlayerResponseHint();
        }

        private void PostBlinds()
        {
            if (playerIsDealer)
            {
                PayPlayer(smallBlind);
                PayAI(bigBlind);
                currentBet = bigBlind;
            }
            else
            {
                PayAI(smallBlind);
                PayPlayer(bigBlind);
                currentBet = bigBlind;
            }
        }

        private IEnumerator AITurnThenAdvance()
        {
            yield return new WaitForSecondsRealtime(aiThinkDelay);

            if (!handInProgress) yield break;
            if (playerFolded || aiFolded) yield break;

            int toCall = GetToCallForAI();
            int aiHp = GetAIHP();

            NPCHybridBrainResult brainResult;

            if (hybridBrain != null)
            {
                brainResult = hybridBrain.Decide(toCall, aiHp, bigBlind);
            }
            else
            {
                brainResult = new NPCHybridBrainResult
                {
                    action = ai != null ? ai.Decide(toCall, aiHp) : PlayerActionType.Call,
                    raiseTo = 0,
                    emotion = EnemyHeadCalmController.EmotionState.Calm,
                    dialogueIntent = NPCDialogueIntent.None,
                    confidence = 1f,
                    dialogueChance = 0f
                };
            }

            PlayerActionType decision = brainResult.action;

            if (decision == PlayerActionType.Fold && toCall > 0)
            {
                aiFolded = true;
                LogAction("Противник: сброс");
                ResolveHandByFold();
                yield break;
            }

            if (decision == PlayerActionType.Check && toCall == 0)
            {
                LogAction("Противник: чек");
            }
            else if (decision == PlayerActionType.Call || (decision == PlayerActionType.Check && toCall > 0))
            {
                int paid = 0;

                if (toCall > 0)
                    paid = PayAI(toCall);

                LogAction(paid > 0 ? $"Противник: колл {paid} HP" : "Противник: чек");

                OnPotUpdated?.Invoke();
                OnGameStateChanged?.Invoke();
            }
            else if (decision == PlayerActionType.Raise)
            {
                int raiseTo = brainResult.raiseTo > 0
                    ? brainResult.raiseTo
                    : GetFallbackRaiseAmount(toCall, aiHp);

                int paid = 0;

                if (toCall > 0)
                    paid += PayAI(toCall);

                int extra = raiseTo - aiBetThisRound;

                if (extra > 0)
                    paid += PayAI(extra);

                currentBet = Mathf.Max(currentBet, aiBetThisRound);

                LogAction($"Противник: повысил до {currentBet} HP");

                OnPotUpdated?.Invoke();
                OnCardsChanged?.Invoke();
                OnGameStateChanged?.Invoke();

                waitingForPlayer = true;

                LogPlayerResponseHint();
                OnTurnChanged?.Invoke(true);

                yield break;
            }

            EndBettingRoundAndAdvanceStreet();
        }

        private void EndBettingRoundAndAdvanceStreet()
        {
            if (GetToCallForPlayer() > 0)
            {
                waitingForPlayer = true;
                OnTurnChanged?.Invoke(true);
                LogPlayerResponseHint();
                OnGameStateChanged?.Invoke();
                return;
            }

            playerBetThisRound = 0;
            aiBetThisRound = 0;
            currentBet = 0;

            if (state == GameState.Preflop)
            {
                community.Add(deck.Draw());
                community.Add(deck.Draw());
                community.Add(deck.Draw());

                state = GameState.Flop;
                waitingForPlayer = true;

                LogAction("Открыт флоп");
                OnTurnChanged?.Invoke(true);
                LogPlayerResponseHint();
            }
            else if (state == GameState.Flop)
            {
                community.Add(deck.Draw());

                state = GameState.Turn;
                waitingForPlayer = true;

                LogAction("Открыт тёрн");
                OnTurnChanged?.Invoke(true);
                LogPlayerResponseHint();
            }
            else if (state == GameState.Turn)
            {
                community.Add(deck.Draw());

                state = GameState.River;
                waitingForPlayer = true;

                LogAction("Открыт ривер");
                OnTurnChanged?.Invoke(true);
                LogPlayerResponseHint();
            }
            else if (state == GameState.River)
            {
                state = GameState.Showdown;
                waitingForPlayer = false;
                OnTurnChanged?.Invoke(false);

                LogAction("Вскрытие");
                ResolveShowdown();
                return;
            }

            OnCardsChanged?.Invoke();
            OnPotUpdated?.Invoke();
            OnGameStateChanged?.Invoke();
        }

        private void ResolveHandByFold()
        {
            state = GameState.HandOver;
            handInProgress = false;
            waitingForPlayer = false;

            bool playerWinner = aiFolded && !playerFolded;
            bool aiWinner = playerFolded && !aiFolded;

            if (playerWinner)
            {
                SoulManager.Instance.AwardPot(pot, true);
                LogAction($"Игрок забирает банк {pot} HP");
            }
            else if (aiWinner)
            {
                SoulManager.Instance.AwardPot(pot, false);
                LogAction($"Противник забирает банк {pot} HP");
            }

            OnPotUpdated?.Invoke();
            OnGameStateChanged?.Invoke();
            OnCardsChanged?.Invoke();
            OnTurnChanged?.Invoke(false);

            if (CheckGameOverByHP())
                return;

            StartCoroutine(NextHandAfterDelay());
        }

        private void ResolveShowdown()
        {
            while (community.Count < 5)
                community.Add(deck.Draw());

            List<Card> player7 = new List<Card>(playerHand);
            player7.AddRange(community);

            List<Card> ai7 = new List<Card>(aiHand);
            ai7.AddRange(community);

            HandValue pVal = HandEvaluator.Evaluate7(player7);
            HandValue aVal = HandEvaluator.Evaluate7(ai7);

            int cmp = pVal.CompareTo(aVal);

            if (cmp > 0)
            {
                SoulManager.Instance.AwardPot(pot, true);
                LogAction($"Игрок выиграл вскрытие и забрал {pot} HP");
            }
            else if (cmp < 0)
            {
                SoulManager.Instance.AwardPot(pot, false);
                LogAction($"Противник выиграл вскрытие и забрал {pot} HP");
            }
            else
            {
                int p = pot / 2;
                int a = pot - p;

                SoulManager.Instance.AwardPot(p, true);
                SoulManager.Instance.AwardPot(a, false);

                LogAction("Ничья — банк поделен");
            }

            state = GameState.HandOver;
            handInProgress = false;
            waitingForPlayer = false;

            OnPotUpdated?.Invoke();
            OnGameStateChanged?.Invoke();
            OnCardsChanged?.Invoke();
            OnTurnChanged?.Invoke(false);

            if (CheckGameOverByHP())
                return;

            StartCoroutine(NextHandAfterDelay());
        }

        private IEnumerator NextHandAfterDelay()
        {
            yield return new WaitForSecondsRealtime(nextHandDelay);

            pot = 0;
            OnPotUpdated?.Invoke();

            playerIsDealer = !playerIsDealer;

            StartNewHand();
        }

        // =========================
        // GAME OVER
        // =========================
       


        private bool CheckGameOverByHP()
        {
            if (SoulManager.Instance == null)
                return false;

            bool playerWon = GetPlayerHP() >= winHpTarget;
            bool aiWon = GetAIHP() >= winHpTarget;

            if (!playerWon && !aiWon)
                return false;

            StopAllCoroutines();

            handInProgress = false;
            waitingForPlayer = false;
            playerFolded = false;
            aiFolded = false;

            state = GameState.GameOver;

            if (playerWon && aiWon)
            {
                gameOverMessage = "НИЧЬЯ";
                LogAction("Игра окончена: ничья");
            }
            else if (playerWon)
            {
                gameOverMessage = "ВЫ ВЫИГРАЛИ";
                LogAction("Игра окончена: вы выиграли");
            }
            else
            {
                gameOverMessage = "ВЫ ПРОИГРАЛИ";
                LogAction("Игра окончена: вы проиграли");
            }

            OnPotUpdated?.Invoke();
            OnCardsChanged?.Invoke();
            OnGameStateChanged?.Invoke();
            OnTurnChanged?.Invoke(false);
            
            return true;
        }

        // =========================
        // HELPERS
        // =========================

        private bool CanPlayerAct()
        {
            if (!handInProgress) return false;
            if (!waitingForPlayer) return false;
            if (state == GameState.Showdown || state == GameState.HandOver || state == GameState.GameOver) return false;
            if (playerFolded || aiFolded) return false;

            return true;
        }

        private int GetToCallForPlayer()
        {
            return Mathf.Max(0, currentBet - playerBetThisRound);
        }

        private int GetToCallForAI()
        {
            return Mathf.Max(0, currentBet - aiBetThisRound);
        }

        private int PayPlayer(int amount)
        {
            int paid = SoulManager.Instance.TakePlayer(amount);
            playerBetThisRound += paid;
            pot += paid;
            return paid;
        }

        private int PayAI(int amount)
        {
            int paid = SoulManager.Instance.TakeAI(amount);
            aiBetThisRound += paid;
            pot += paid;
            return paid;
        }

        private int GetFallbackRaiseAmount(int toCall, int aiHp)
        {
            if (ai != null)
                return ai.GetRaiseAmount(toCall, aiHp, bigBlind);

            int min = Mathf.Min(aiHp, toCall + bigBlind);
            int max = aiHp;

            if (min >= max)
                return aiHp;

            return Mathf.Clamp(min + bigBlind, min, max);
        }

        private void LogAction(string text)
        {
            OnActionLog?.Invoke(text);
        }

        private void LogPlayerResponseHint()
        {
            int toCall = GetToCallForPlayer();

            if (toCall <= 0)
                LogAction("Ваш ход: можно чек, повысить или ва-банк");
            else
                LogAction($"Ваш ход: нужно уравнять {toCall} HP");
        }

        // =========================
        // SAVE / LOAD
        // =========================

        public void LoadUnifiedSave(PokerUnifiedSaveData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[PokerGame] LoadUnifiedSave: data is null");
                return;
            }

            if (SoulManager.Instance == null)
            {
                Debug.LogWarning("[PokerGame] LoadUnifiedSave: SoulManager is null");
                return;
            }

            StopAllCoroutines();

            SoulManager.Instance.SetPlayerSouls(data.playerHp);
            SoulManager.Instance.SetAISouls(data.aiHp);

            playerIsDealer = data.playerIsDealer;

            deck.ResetAndShuffle();

            playerHand.Clear();
            aiHand.Clear();
            community.Clear();

            playerHand.Add(deck.Draw());
            aiHand.Add(deck.Draw());
            playerHand.Add(deck.Draw());
            aiHand.Add(deck.Draw());

            pot = data.potHp;
            currentBet = 0;
            playerBetThisRound = 0;
            aiBetThisRound = 0;

            playerFolded = false;
            aiFolded = false;

            gameOverMessage = "";
            handInProgress = true;
            waitingForPlayer = true;

            state = GameState.Preflop;

            OnCardsChanged?.Invoke();
            OnPotUpdated?.Invoke();
            OnGameStateChanged?.Invoke();
            OnTurnChanged?.Invoke(true);

            LogAction("Сейв загружен");
            LogPlayerResponseHint();

            CheckGameOverByHP();
        }
    }
}