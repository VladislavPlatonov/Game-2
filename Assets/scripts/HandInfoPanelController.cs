using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Poker;

public class HandInfoPanelController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PokerGame game;
    [SerializeField] private TextMeshProUGUI handInfoText;

    private void Awake()
    {
        if (game == null)
            game = FindFirstObjectByType<PokerGame>();
    }

    private void OnEnable()
    {
        if (game == null) return;

        game.OnCardsChanged += Refresh;
        game.OnGameStateChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (game == null) return;

        game.OnCardsChanged -= Refresh;
        game.OnGameStateChanged -= Refresh;
    }

    private void Refresh()
    {
        if (game == null || handInfoText == null)
            return;

        List<Card> cards = new List<Card>();
        cards.AddRange(game.GetPlayerHand());
        cards.AddRange(game.GetCommunityCards());

        if (cards.Count == 0)
        {
            handInfoText.text = " ŒÃ¡»Õ¿÷»ﬂ: -";
            return;
        }

        string comboName = GetCombinationName(cards);
        handInfoText.text = $" ŒÃ¡»Õ¿÷»ﬂ: {comboName}";
    }

    private string GetCombinationName(List<Card> cards)
    {
        var rankGroups = cards
            .GroupBy(c => (int)c.Rank)
            .Select(g => g.Count())
            .OrderByDescending(c => c)
            .ToList();

        bool isFlush = cards
            .GroupBy(c => c.Suit)
            .Any(g => g.Count() >= 5);

        bool isStraight = HasStraight(cards);

        if (isFlush && isStraight)
            return "—“–»“-‘À≈ÿ";

        if (rankGroups.Count > 0 && rankGroups[0] == 4)
            return " ¿–≈";

        if (rankGroups.Count > 1 && rankGroups[0] == 3 && rankGroups[1] >= 2)
            return "‘”ÀÀ-’¿”—";

        if (isFlush)
            return "‘À≈ÿ";

        if (isStraight)
            return "—“–»“";

        if (rankGroups.Count > 0 && rankGroups[0] == 3)
            return "—≈“";

        if (rankGroups.Count > 1 && rankGroups[0] == 2 && rankGroups[1] == 2)
            return "ƒ¬≈ œ¿–€";

        if (rankGroups.Count > 0 && rankGroups[0] == 2)
            return "œ¿–¿";

        return "—“¿–ÿ¿ﬂ  ¿–“¿";
    }

    private bool HasStraight(List<Card> cards)
    {
        List<int> ranks = cards
            .Select(c => (int)c.Rank)
            .Distinct()
            .OrderBy(r => r)
            .ToList();

        // ÂÒÎË ÚÛÁ ‚˚ÒÓÍËÈ, ‰Ó·‡‚ËÏ Â„Ó Í‡Í 1 ‰Îˇ A-2-3-4-5
        if (ranks.Contains(14))
            ranks.Insert(0, 1);

        int inRow = 1;

        for (int i = 1; i < ranks.Count; i++)
        {
            if (ranks[i] == ranks[i - 1] + 1)
            {
                inRow++;
                if (inRow >= 5)
                    return true;
            }
            else
            {
                inRow = 1;
            }
        }

        return false;
    }
}
