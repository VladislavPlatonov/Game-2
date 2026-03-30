// PokerTypes.cs
using System;

namespace Poker
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades }
    public enum Rank
    {
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
        Jack, Queen, King, Ace
    }

    public enum GameState
    {
        None,
        Preflop,
        Flop,
        Turn,
        River,
        Showdown,
        HandOver,
        GameOver
    }

    [Serializable]
    public struct Card
    {
        public Suit Suit;
        public Rank Rank;

        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public int Value => (int)Rank;

        public override string ToString() => $"{Rank} of {Suit}";
    }

    public enum PlayerActionType
    {
        None,
        Fold,
        Check,
        Call,
        Raise,
        AllIn
    }
}