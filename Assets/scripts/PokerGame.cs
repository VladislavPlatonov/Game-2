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

        [Header("Refs")]
        [SerializeField] private BasicPokerAI ai;

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

        private PokerUnifiedSaveData pendingLoadedSave;

        private void Awake()
        {
            deck = new PokerDeck();

            if (ai == null)
                ai = FindFirstObjectByType<BasicPokerAI>();
        }

        private void Start()
        {
            if (pendingLoadedSave != null)
            {
                LoadUnifiedSave(pendingLoadedSave);
                pendingLoadedSave = null;
                return;
            }

            StartNewHand();
        }

        public void PreparePendingLoad(PokerUnifiedSaveData data)
        {
            pendingLoadedSave = data;
        }

        // =========================
        // PUBLIC API
        // =========================

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

            if (GetPlayerHP() <= 0 || GetAIHP() <= 0)
            {
                state = GameState.GameOver;
                OnGameStateChanged?.Invoke();
                LogAction("Игра окончена");
                return;
            }

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

            var decision = ai != null ? ai.Decide(toCall, aiHp) : PlayerActionType.Call;

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
            }
            else if (decision == PlayerActionType.Raise)
            {
                int raiseTo = ai.GetRaiseAmount(toCall, aiHp, bigBlind);

                int paid = 0;

                if (toCall > 0)
                    paid += PayAI(toCall);

                int extra = raiseTo - aiBetThisRound;
                if (extra > 0)
                    paid += PayAI(extra);

                currentBet = Mathf.Max(currentBet, aiBetThisRound);

                waitingForPlayer = true;

                LogAction($"Противник: повысил до {currentBet} HP");
                LogPlayerResponseHint();

                OnPotUpdated?.Invoke();
                OnCardsChanged?.Invoke();
                OnGameStateChanged?.Invoke();
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

            StartCoroutine(NextHandAfterDelay());
        }

        private void ResolveShowdown()
        {
            var player7 = new List<Card>(playerHand);
            player7.AddRange(community);

            var ai7 = new List<Card>(aiHand);
            ai7.AddRange(community);

            var pVal = HandEvaluator.Evaluate7(player7);
            var aVal = HandEvaluator.Evaluate7(ai7);

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

        private int GetToCallForPlayer() => Mathf.Max(0, currentBet - playerBetThisRound);
        private int GetToCallForAI() => Mathf.Max(0, currentBet - aiBetThisRound);

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

            handInProgress = true;
            waitingForPlayer = true;
            state = GameState.Preflop;

            OnCardsChanged?.Invoke();
            OnPotUpdated?.Invoke();
            OnGameStateChanged?.Invoke();
            OnTurnChanged?.Invoke(true);

            LogAction("Сейв загружен");
            LogPlayerResponseHint();
        }

    }
}
