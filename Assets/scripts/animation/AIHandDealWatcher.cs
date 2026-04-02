using UnityEngine;
using Poker;

public class AIHandDealWatcher : MonoBehaviour
{
    [SerializeField] private PokerGame game;
    [SerializeField] private AIHandEmotionController handController;

    private int lastAICardCount = -1;
    private bool initialized;

    private void Awake()
    {
        if (game == null)
            game = FindFirstObjectByType<PokerGame>();

        if (handController == null)
            handController = FindFirstObjectByType<AIHandEmotionController>();
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

        lastAICardCount = game.GetAIHand().Count;
        initialized = true;
    }

    private void HandleCardsChanged()
    {
        if (game == null || handController == null)
            return;

        int currentAICardCount = game.GetAIHand().Count;

        if (!initialized)
        {
            InitializeSnapshot();
            return;
        }

        if (currentAICardCount > lastAICardCount)
        {
            handController.PlayDealGrab();
        }

        lastAICardCount = currentAICardCount;
    }
}
