using UnityEngine;

namespace Poker
{
    public class SoulManager : MonoBehaviour
    {
        public static SoulManager Instance { get; private set; }

        [Header("Starting Souls")]
        [SerializeField] private int startingPlayerSouls = 100;
        [SerializeField] private int startingAISouls = 100;

        [Header("Scene Lifetime")]
        [SerializeField] private bool dontDestroyOnLoad = true;

        private int playerSouls;
        private int aiSouls;

        public System.Action<int> OnPlayerSoulsChanged;
        public System.Action<int> OnAISoulsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            ResetSouls();
        }

        public int GetPlayerSouls()
        {
            return playerSouls;
        }

        public int GetAISouls()
        {
            return aiSouls;
        }

        public void ResetSouls()
        {
            playerSouls = Mathf.Max(0, startingPlayerSouls);
            aiSouls = Mathf.Max(0, startingAISouls);

            Notify();
        }

        public void NewGameReset()
        {
            ResetSouls();
        }

        public void SetSouls(int player, int ai)
        {
            playerSouls = Mathf.Max(0, player);
            aiSouls = Mathf.Max(0, ai);

            Notify();
        }

        public void AddPlayerSouls(int amount)
        {
            playerSouls = Mathf.Max(0, playerSouls + amount);
            OnPlayerSoulsChanged?.Invoke(playerSouls);
        }

        public void AddAISouls(int amount)
        {
            aiSouls = Mathf.Max(0, aiSouls + amount);
            OnAISoulsChanged?.Invoke(aiSouls);
        }

        public int TakePlayer(int amount)
        {
            int pay = Mathf.Clamp(amount, 0, playerSouls);

            playerSouls -= pay;
            OnPlayerSoulsChanged?.Invoke(playerSouls);

            return pay;
        }

        public int TakeAI(int amount)
        {
            int pay = Mathf.Clamp(amount, 0, aiSouls);

            aiSouls -= pay;
            OnAISoulsChanged?.Invoke(aiSouls);

            return pay;
        }

        public void AwardPot(int pot, bool playerWinner)
        {
            if (playerWinner)
            {
                playerSouls = Mathf.Max(0, playerSouls + pot);
                OnPlayerSoulsChanged?.Invoke(playerSouls);
            }
            else
            {
                aiSouls = Mathf.Max(0, aiSouls + pot);
                OnAISoulsChanged?.Invoke(aiSouls);
            }
        }

        public SaveData GetSaveData()
        {
            return new SaveData
            {
                playerSouls = playerSouls,
                aiSouls = aiSouls
            };
        }

        public void LoadSaveData(SaveData data)
        {
            if (data == null) return;

            SetSouls(data.playerSouls, data.aiSouls);
        }

        public void SetPlayerSouls(int value)
        {
            playerSouls = Mathf.Max(0, value);
            OnPlayerSoulsChanged?.Invoke(playerSouls);
        }

        public void SetAISouls(int value)
        {
            aiSouls = Mathf.Max(0, value);
            OnAISoulsChanged?.Invoke(aiSouls);
        }

        private void Notify()
        {
            OnPlayerSoulsChanged?.Invoke(playerSouls);
            OnAISoulsChanged?.Invoke(aiSouls);
        }
    }
}