using System;
using System.Collections.Generic;

namespace Poker
{
    public interface IPokerGame
    {
        event Action OnCardsChanged;
        event Action OnGameStateChanged;
        event Action OnPotUpdated;
        event Action OnTurnChanged;

        IReadOnlyList<Card> GetAIHand();
        IReadOnlyList<Card> GetCommunityCards();
        int GetCurrentBet();
        int GetCurrentPot();
        GameState GetCurrentState();
        int GetPlayerBetThisRound();
        IReadOnlyList<Card> GetPlayerHand();
        int GetPlayerHP();
        bool IsWaitingForPlayerAction();
        void PlayerAllIn();
        void PlayerCall();
        void PlayerCheck();
        void PlayerFold();
        void PlayerRaise(int amountTotalThisRound);
        void StartNewHand();
    }
}