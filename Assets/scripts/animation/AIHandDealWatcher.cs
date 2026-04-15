using UnityEngine;
using Poker;

public class AIHandDealWatcher : MonoBehaviour
{
    [SerializeField] private PokerGame game;
    [SerializeField] private AIHandEmotionController handController;

    private GameState lastState;
    private bool initialized;
    private bool dealPlayedThisPreflop;

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

        game.OnCardsChanged += HandleStateOrCardsChanged;
        game.OnGameStateChanged += HandleStateOrCardsChanged;

        InitializeSnapshot();
    }

    private void OnDisable()
    {
        if (game == null) return;

        game.OnCardsChanged -= HandleStateOrCardsChanged;
        game.OnGameStateChanged -= HandleStateOrCardsChanged;
    }

    private void InitializeSnapshot()
    {
        if (game == null) return;

        lastState = game.GetCurrentState();
        dealPlayedThisPreflop = false;
        initialized = true;
    }

    private void HandleStateOrCardsChanged()
    {
        if (game == null || handController == null)
            return;

        if (!initialized)
        {
            InitializeSnapshot();
            return;
        }

        GameState currentState = game.GetCurrentState();
        int currentAICardCount = game.GetAIHand().Count;

        // Новый префлоп = готовимся к новой анимации раздачи
        if (currentState == GameState.Preflop && lastState != GameState.Preflop)
        {
            dealPlayedThisPreflop = false;
        }

        // Как только в новом префлопе у AI уже есть карты — играем подбор
        if (currentState == GameState.Preflop && !dealPlayedThisPreflop && currentAICardCount >= 2)
        {
            handController.PlayDealGrab();
            dealPlayedThisPreflop = true;
        }

        lastState = currentState;
    }
}
