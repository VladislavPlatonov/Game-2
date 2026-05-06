using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Poker
{
    public class PokerActionPanel : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PokerGame game;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private TextMeshProUGUI needToCallText;
        [SerializeField] private TextMeshProUGUI actionLogText;

        [Header("Log")]
        [SerializeField] private int maxLines = 8;

        private readonly Queue<string> lines = new();

        private void Awake()
        {
            if (game == null)
                game = FindFirstObjectByType<PokerGame>();
        }

        private void OnEnable()
        {
            if (game == null) return;

            game.OnActionLog += AddLine;
            game.OnTurnChanged += RefreshTurn;
            game.OnPotUpdated += RefreshNeedToCall;
            game.OnGameStateChanged += RefreshNeedToCall;

            RefreshTurn(game.IsWaitingForPlayerAction());
            RefreshNeedToCall();
        }

        private void OnDisable()
        {
            if (game == null) return;

            game.OnActionLog -= AddLine;
            game.OnTurnChanged -= RefreshTurn;
            game.OnPotUpdated -= RefreshNeedToCall;
            game.OnGameStateChanged -= RefreshNeedToCall;
        }

        private void RefreshTurn(bool playerTurn)
        {
            if (turnText == null) return;
            turnText.text = playerTurn ? "" : "’Œƒ œ–Œ“»¬Õ» ¿";
        }

        private void RefreshNeedToCall()
        {
            if (needToCallText == null || game == null) return;

            int toCall = Mathf.Max(0, game.GetCurrentBet() - game.GetPlayerBetThisRound());

            if (toCall <= 0)
                needToCallText.text = "ÃŒ∆ÕŒ ◊≈ ";
            else
                needToCallText.text = $"Õ”∆ÕŒ ”–¿¬Õﬂ“‹: {toCall} HP";
        }

        public void AddLine(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || actionLogText == null) return;

            lines.Enqueue(text);

            while (lines.Count > maxLines)
                lines.Dequeue();

            actionLogText.text = string.Join("\n", lines);
        }
    }
}
