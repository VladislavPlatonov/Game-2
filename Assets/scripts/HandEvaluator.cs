// HandEvaluator.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace Poker
{
    public enum HandCategory
    {
        HighCard = 1,
        Pair = 2,
        TwoPair = 3,
        ThreeOfAKind = 4,
        Straight = 5,
        Flush = 6,
        FullHouse = 7,
        FourOfAKind = 8,
        StraightFlush = 9
    }

    public struct HandValue : IComparable<HandValue>
    {
        public HandCategory Category;
        public List<int> Tiebreak; // по убыванию

        public int CompareTo(HandValue other)
        {
            if (Category != other.Category) return Category.CompareTo(other.Category);
            int n = Math.Min(Tiebreak.Count, other.Tiebreak.Count);
            for (int i = 0; i < n; i++)
            {
                if (Tiebreak[i] != other.Tiebreak[i]) return Tiebreak[i].CompareTo(other.Tiebreak[i]);
            }
            return 0;
        }
    }

    public static class HandEvaluator
    {
        // Вход: 7 карт (2 руки + до 5 стола)
        public static HandValue Evaluate7(IReadOnlyList<Card> cards)
        {
            if (cards == null || cards.Count < 5) throw new ArgumentException("Need at least 5 cards");

            // Берём лучшую из всех комбинаций 5 из N
            HandValue best = default;
            bool first = true;

            int n = cards.Count;
            for (int a = 0; a < n - 4; a++)
                for (int b = a + 1; b < n - 3; b++)
                    for (int c = b + 1; c < n - 2; c++)
                        for (int d = c + 1; d < n - 1; d++)
                            for (int e = d + 1; e < n; e++)
                            {
                                var five = new List<Card> { cards[a], cards[b], cards[c], cards[d], cards[e] };
                                var hv = Evaluate5(five);
                                if (first || hv.CompareTo(best) > 0)
                                {
                                    best = hv;
                                    first = false;
                                }
                            }

            return best;
        }

        // 5 карт → категория и тайбрейки
        private static HandValue Evaluate5(List<Card> five)
        {
            five = five.OrderByDescending(x => x.Value).ToList();

            bool isFlush = five.All(c => c.Suit == five[0].Suit);

            var values = five.Select(c => c.Value).OrderByDescending(v => v).ToList();
            var distinct = values.Distinct().ToList();

            bool isStraight = IsStraight(values, out int straightHigh);

            // группировки по рангу
            var groups = values
                .GroupBy(v => v)
                .OrderByDescending(g => g.Count())
                .ThenByDescending(g => g.Key)
                .ToList();

            // Straight Flush
            if (isFlush && isStraight)
            {
                return new HandValue
                {
                    Category = HandCategory.StraightFlush,
                    Tiebreak = new List<int> { straightHigh }
                };
            }

            // Four
            if (groups[0].Count() == 4)
            {
                int quad = groups[0].Key;
                int kicker = groups[1].Key;
                return new HandValue
                {
                    Category = HandCategory.FourOfAKind,
                    Tiebreak = new List<int> { quad, kicker }
                };
            }

            // Full House
            if (groups[0].Count() == 3 && groups[1].Count() == 2)
            {
                int trips = groups[0].Key;
                int pair = groups[1].Key;
                return new HandValue
                {
                    Category = HandCategory.FullHouse,
                    Tiebreak = new List<int> { trips, pair }
                };
            }

            // Flush
            if (isFlush)
            {
                return new HandValue
                {
                    Category = HandCategory.Flush,
                    Tiebreak = values
                };
            }

            // Straight
            if (isStraight)
            {
                return new HandValue
                {
                    Category = HandCategory.Straight,
                    Tiebreak = new List<int> { straightHigh }
                };
            }

            // Trips
            if (groups[0].Count() == 3)
            {
                int trips = groups[0].Key;
                var kickers = groups.Skip(1).Select(g => g.Key).OrderByDescending(x => x).ToList();
                return new HandValue
                {
                    Category = HandCategory.ThreeOfAKind,
                    Tiebreak = new List<int> { trips }.Concat(kickers).ToList()
                };
            }

            // Two Pair
            if (groups[0].Count() == 2 && groups[1].Count() == 2)
            {
                int highPair = Math.Max(groups[0].Key, groups[1].Key);
                int lowPair = Math.Min(groups[0].Key, groups[1].Key);
                int kicker = groups[2].Key;
                return new HandValue
                {
                    Category = HandCategory.TwoPair,
                    Tiebreak = new List<int> { highPair, lowPair, kicker }
                };
            }

            // Pair
            if (groups[0].Count() == 2)
            {
                int pair = groups[0].Key;
                var kickers = groups.Skip(1).Select(g => g.Key).OrderByDescending(x => x).ToList();
                return new HandValue
                {
                    Category = HandCategory.Pair,
                    Tiebreak = new List<int> { pair }.Concat(kickers).ToList()
                };
            }

            // High Card
            return new HandValue
            {
                Category = HandCategory.HighCard,
                Tiebreak = values
            };
        }

        private static bool IsStraight(List<int> valuesDesc, out int straightHigh)
        {
            // обработка A-5 (wheel)
            var distinct = valuesDesc.Distinct().OrderByDescending(x => x).ToList();
            if (distinct.Count < 5)
            {
                straightHigh = 0;
                return false;
            }

            // обычная
            for (int i = 0; i <= distinct.Count - 5; i++)
            {
                int start = distinct[i];
                bool ok = true;
                for (int k = 1; k < 5; k++)
                {
                    if (!distinct.Contains(start - k)) { ok = false; break; }
                }
                if (ok)
                {
                    straightHigh = start;
                    return true;
                }
            }

            // A-5
            if (distinct.Contains(14) && distinct.Contains(5) && distinct.Contains(4) && distinct.Contains(3) && distinct.Contains(2))
            {
                straightHigh = 5;
                return true;
            }

            straightHigh = 0;
            return false;
        }
    }
}