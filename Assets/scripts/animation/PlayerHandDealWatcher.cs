using UnityEngine;
using Poker;

public class PlayerHandDealWatcher : MonoBehaviour
{
    [SerializeField] private PokerGame game;
    [SerializeField] private PlayerHandBalatroController handController;

    private int lastPlayerCardCount = -1;
    private int lastCommunityCount = -1;
    private bool initialized;

    private void Awake()
    {
        if (game == null)
            game = FindFirstObjectByType<PokerGame>();

        if (handController == null)
            handController = FindFirstObjectByType<PlayerHandBalatroController>();
    }

    private void OnEnable()
    {
        if (game == null) return;

        game.OnCardsChanged += HandleCardsChanged;
        game.OnGameStateChanged += HandleCardsChanged;

        InitializeSnapshot();
    }

    private void OnDisable()
    {
        if (game == null) return;

        game.OnCardsChanged -= HandleCardsChanged;
        game.OnGameStateChanged -= HandleCardsChanged;
    }

    private void InitializeSnapshot()
    {
        if (game == null) return;

        lastPlayerCardCount = game.GetPlayerHand().Count;
        lastCommunityCount = game.GetCommunityCards().Count;
        initialized = true;
    }

    private void HandleCardsChanged()
    {
        if (game == null || handController == null)
            return;

        int currentPlayerCount = game.GetPlayerHand().Count;
        int currentCommunityCount = game.GetCommunityCards().Count;

        if (!initialized)
        {
            InitializeSnapshot();
            return;
        }

        // Анимацию запускаем только когда увеличилось число карт у игрока на руке.
        if (currentPlayerCount > lastPlayerCardCount)
        {
            handController.PlayDealGrab();
        }

        lastPlayerCardCount = currentPlayerCount;
        lastCommunityCount = currentCommunityCount;
    }
}
