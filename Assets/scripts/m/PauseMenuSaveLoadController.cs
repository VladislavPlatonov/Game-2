using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Poker;

public class PauseMenuSaveLoadController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PokerGame game;
    [SerializeField] private PauseMenuController pauseMenuController;

    [Header("Save Panel")]
    [SerializeField] private TextMeshProUGUI saveStatusText;

    [Header("Load Panel")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private PauseSaveSlotUI saveSlotPrefab;
    [SerializeField] private Button loadSelectedButton;
    [SerializeField] private Button deleteSelectedButton;
    [SerializeField] private TextMeshProUGUI emptyText;

    private readonly List<PauseSaveSlotUI> spawnedSlots = new();
    private string selectedSavePath;

    private string SaveFolderPath => Path.Combine(Application.persistentDataPath, "saves");

    private void Awake()
    {
        if (game == null)
            game = FindFirstObjectByType<PokerGame>();

        if (pauseMenuController == null)
            pauseMenuController = FindFirstObjectByType<PauseMenuController>();

        if (!Directory.Exists(SaveFolderPath))
            Directory.CreateDirectory(SaveFolderPath);
    }

    private void Start()
    {
        ResetLoadButtonsState();
    }

    public void OnSavePanelOpened()
    {
        if (saveStatusText != null)
            saveStatusText.text = string.Empty;
    }

    public void OnLoadPanelOpened()
    {
        RefreshSaveList();
    }

    public void SaveNow()
    {
        StartCoroutine(SaveNowRoutine());
    }

    private IEnumerator SaveNowRoutine()
    {
        if (game == null)
        {
            if (saveStatusText != null)
                saveStatusText.text = "Ошибка: PokerGame не найден";
            yield break;
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string jsonFileName = $"save_{timestamp}.json";
        string pngFileName = $"save_{timestamp}.png";

        string jsonPath = Path.Combine(SaveFolderPath, jsonFileName);
        string pngPath = Path.Combine(SaveFolderPath, pngFileName);

        PokerUnifiedSaveData data = new PokerUnifiedSaveData
        {
            saveId = Guid.NewGuid().ToString(),
            saveName = "СЕЙВ",
            sceneName = SceneManager.GetActiveScene().name,
            createdAt = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
            screenshotFileName = pngFileName,

            playerHp = game.GetPlayerHP(),
            aiHp = game.GetAIHP(),
            potHp = game.GetCurrentPot(),
            playerIsDealer = game.GetPlayerIsDealer()
        };

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(jsonPath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[PauseMenuSaveLoadController] Save json error: {e}");

            if (saveStatusText != null)
                saveStatusText.text = "Ошибка при сохранении";
            yield break;
        }

        yield return new WaitForEndOfFrame();

        try
        {
            Texture2D shot = ScreenCapture.CaptureScreenshotAsTexture();
            byte[] pngBytes = shot.EncodeToPNG();
            File.WriteAllBytes(pngPath, pngBytes);
            Destroy(shot);

            if (saveStatusText != null)
                saveStatusText.text = "Игра успешно сохранена";
        }
        catch (Exception e)
        {
            Debug.LogError($"[PauseMenuSaveLoadController] Screenshot save error: {e}");

            if (saveStatusText != null)
                saveStatusText.text = "Ошибка при сохранении скриншота";
        }
    }

    public void RefreshSaveList()
    {
        selectedSavePath = null;
        ResetLoadButtonsState();
        ClearSpawnedSlots();

        if (contentRoot == null || saveSlotPrefab == null)
        {
            Debug.LogWarning("[PauseMenuSaveLoadController] contentRoot или saveSlotPrefab не назначены.");
            ShowEmptyText(true);
            return;
        }

        if (!Directory.Exists(SaveFolderPath))
        {
            ShowEmptyText(true);
            return;
        }

        string[] files = Directory.GetFiles(SaveFolderPath, "*.json");

        if (files == null || files.Length == 0)
        {
            ShowEmptyText(true);
            return;
        }

        // Старые сверху, новые снизу
        Array.Sort(files, (a, b) => File.GetLastWriteTime(a).CompareTo(File.GetLastWriteTime(b)));

        int validCount = 0;

        foreach (string file in files)
        {
            try
            {
                string json = File.ReadAllText(file);
                PokerUnifiedSaveData data = JsonUtility.FromJson<PokerUnifiedSaveData>(json);

                if (data == null)
                    continue;

                PauseSaveSlotUI slot = Instantiate(saveSlotPrefab, contentRoot);
                slot.Setup(data, file, this, validCount + 1);
                slot.SetSelected(false);

                spawnedSlots.Add(slot);
                validCount++;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PauseMenuSaveLoadController] Не удалось прочитать сейв {file}: {e}");
            }
        }

        ShowEmptyText(validCount == 0);
    }

    public void SelectSave(string filePath, PauseSaveSlotUI clickedSlot)
    {
        selectedSavePath = filePath;

        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] != null)
                spawnedSlots[i].SetSelected(spawnedSlots[i] == clickedSlot);
        }

        if (loadSelectedButton != null)
            loadSelectedButton.interactable = true;

        if (deleteSelectedButton != null)
            deleteSelectedButton.interactable = true;
    }

    public void LoadSelectedSave()
    {
        if (string.IsNullOrEmpty(selectedSavePath))
            return;

        if (!File.Exists(selectedSavePath))
            return;

        if (game == null)
        {
            Debug.LogWarning("[PauseMenuSaveLoadController] PokerGame не найден.");
            return;
        }

        try
        {
            string json = File.ReadAllText(selectedSavePath);
            PokerUnifiedSaveData data = JsonUtility.FromJson<PokerUnifiedSaveData>(json);

            if (data == null)
                return;

            game.LoadUnifiedSave(data);

            if (pauseMenuController != null)
                pauseMenuController.ResumeGame();
        }
        catch (Exception e)
        {
            Debug.LogError($"[PauseMenuSaveLoadController] Load error: {e}");
        }
    }

    public void DeleteSelectedSave()
    {
        if (string.IsNullOrEmpty(selectedSavePath))
            return;

        if (!File.Exists(selectedSavePath))
            return;

        try
        {
            string json = File.ReadAllText(selectedSavePath);
            PokerUnifiedSaveData data = JsonUtility.FromJson<PokerUnifiedSaveData>(json);

            File.Delete(selectedSavePath);

            if (data != null && !string.IsNullOrEmpty(data.screenshotFileName))
            {
                string screenshotPath = Path.Combine(SaveFolderPath, data.screenshotFileName);
                if (File.Exists(screenshotPath))
                    File.Delete(screenshotPath);
            }

            selectedSavePath = null;
            RefreshSaveList();
        }
        catch (Exception e)
        {
            Debug.LogError($"[PauseMenuSaveLoadController] Delete error: {e}");
        }
    }

    private void ResetLoadButtonsState()
    {
        if (loadSelectedButton != null)
            loadSelectedButton.interactable = false;

        if (deleteSelectedButton != null)
            deleteSelectedButton.interactable = false;
    }

    private void ClearSpawnedSlots()
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] != null)
                Destroy(spawnedSlots[i].gameObject);
        }

        spawnedSlots.Clear();
    }

    private void ShowEmptyText(bool show)
    {
        if (emptyText != null)
            emptyText.gameObject.SetActive(show);
    }
}
