// PokerDeck.cs
using System;
using System.Collections.Generic;

namespace Poker
{
    public class PokerDeck
    {
        private readonly List<Card> _cards = new List<Card>(52);
        private readonly Random _rng = new Random();

        public void ResetAndShuffle()
        {
            _cards.Clear();
            foreach (Suit s in Enum.GetValues(typeof(Suit)))
                foreach (Rank r in Enum.GetValues(typeof(Rank)))
                    _cards.Add(new Card(s, r));

            // Fisher–Yates
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
        }

        public Card Draw()
        {
            if (_cards.Count == 0) throw new InvalidOperationException("Deck is empty");
            var c = _cards[0];
            _cards.RemoveAt(0);
            return c;
        }
    }
}