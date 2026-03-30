using System.IO;
using UnityEngine;
using Poker;

public class PendingSaveLoader : MonoBehaviour
{
    private void Awake()
    {
        if (string.IsNullOrEmpty(MainMenuLoadController.PendingLoadSavePath))
            return;

        string path = MainMenuLoadController.PendingLoadSavePath;

        if (!File.Exists(path))
        {
            Debug.LogWarning("[PendingSaveLoader] Save file not found: " + path);
            MainMenuLoadController.PendingLoadSavePath = null;
            return;
        }

        string json = File.ReadAllText(path);
        PokerSimpleSaveData data = JsonUtility.FromJson<PokerSimpleSaveData>(json);

        PokerGame game = FindFirstObjectByType<PokerGame>();
        if (game == null)
        {
            Debug.LogError("[PendingSaveLoader] PokerGame not found in scene.");
            MainMenuLoadController.PendingLoadSavePath = null;
            return;
        }

        game.PreparePendingLoad(data);

        MainMenuLoadController.PendingLoadSavePath = null;
    }
}
