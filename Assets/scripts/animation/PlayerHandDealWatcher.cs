using UnityEngine;
using Poker;

public class PlayerHandDealWatcher : MonoBehaviour
{
    [SerializeField] private PokerGame game;
    [SerializeField] private PlayerHandBalatroController handController;

    private GameState lastState;
    private bool initialized;
    private bool dealPlayedThisPreflop;

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
        int currentPlayerCount = game.GetPlayerHand().Count;

        // Если начался новый префлоп — готовимся проиграть анимацию новой раздачи
        if (currentState == GameState.Preflop && lastState != GameState.Preflop)
        {
            dealPlayedThisPreflop = false;
        }

        // Как только в префлопе у игрока уже есть карты, играем подбор 1 раз
        if (currentState == GameState.Preflop && !dealPlayedThisPreflop && currentPlayerCount >= 2)
        {
            handController.PlayDealGrab();
            dealPlayedThisPreflop = true;
        }

        // Когда выходим из префлопа, просто обновляем состояние
        if (currentState != GameState.Preflop)
        {
            // ничего не сбрасываем здесь специально,
            // сброс будет при следующем входе в новый Preflop
        }

        lastState = currentState;
    }
}
