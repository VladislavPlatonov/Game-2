// BasicPokerAI.cs
using UnityEngine;

namespace Poker
{
    public class BasicPokerAI : MonoBehaviour
    {
        [Range(0f, 1f)] public float aggressiveness = 0.35f;

        public PlayerActionType Decide(int toCall, int aiHp)
        {
            if (aiHp <= 0) return PlayerActionType.Fold;

            if (toCall == 0)
            {
                // либо чек, либо иногда рейз
                if (Random.value < aggressiveness * 0.25f && aiHp > 2) return PlayerActionType.Raise;
                return PlayerActionType.Check;
            }

            // если колл слишком дорогой Ч иногда фолд
            float danger = (float)toCall / Mathf.Max(1, aiHp);
            if (danger > 0.65f && Random.value > aggressiveness) return PlayerActionType.Fold;

            // чаще колл
            if (Random.value < aggressiveness * 0.2f && aiHp > toCall + 2) return PlayerActionType.Raise;
            return PlayerActionType.Call;
        }

        public int GetRaiseAmount(int toCall, int aiHp, int bigBlind)
        {
            // минимально: (toCall + BB), максимум: aiHp
            int min = Mathf.Min(aiHp, toCall + bigBlind);
            int max = aiHp;
            if (min >= max) return aiHp;
            int add = Random.Range(0, Mathf.Max(1, bigBlind * 2));
            return Mathf.Clamp(min + add, min, max);
        }
    }
}