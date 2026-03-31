using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Poker;

public class PauseMenuSaveLoadController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PokerGame game;
    [SerializeField] private PauseMenuController pauseMenuController;

    [Header("Save UI")]
    [SerializeField] private TextMeshProUGUI saveStatusText;

    [Header("Load UI")]
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
        if (game == null)
        {
            if (saveStatusText != null)
                saveStatusText.text = "Ошибка: PokerGame не найден";
            return;
        }

        try
        {
            if (!Directory.Exists(SaveFolderPath))
                Directory.CreateDirectory(SaveFolderPath);

            PokerSimpleSaveData data = new PokerSimpleSaveData
            {
                saveId = Guid.NewGuid().ToString(),
                saveName = "Сейв " + DateTime.Now.ToString("HH:mm:ss"),
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                createdAt = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                playerHp = game.GetPlayerHP(),
                aiHp = game.GetAIHP(),
                playerIsDealer = game.GetPlayerIsDealer()
            };

            string fileName = "save_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
            string filePath = Path.Combine(SaveFolderPath, fileName);

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(filePath, json);

            if (saveStatusText != null)
                saveStatusText.text = "Игра успешно сохранена";
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveNow error: {e}");

            if (saveStatusText != null)
                saveStatusText.text = "Ошибка при сохранении";
        }
    }

    public void RefreshSaveList()
    {
        selectedSavePath = null;
        ResetLoadButtonsState();
        ClearSpawnedSlots();

        if (contentRoot == null || saveSlotPrefab == null)
        {
            Debug.LogWarning("PauseMenuSaveLoadController: contentRoot или saveSlotPrefab не назначены.");
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

        Array.Sort(files, (a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));

        int validCount = 0;

        foreach (string file in files)
        {
            try
            {
                string json = File.ReadAllText(file);
                PokerSimpleSaveData data = JsonUtility.FromJson<PokerSimpleSaveData>(json);

                if (data == null)
                    continue;

                PauseSaveSlotUI slot = Instantiate(saveSlotPrefab, contentRoot);
                slot.Setup(data, file, this);
                slot.SetSelected(false);

                spawnedSlots.Add(slot);
                validCount++;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Не удалось прочитать сейв {file}: {e}");
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
            Debug.LogWarning("PauseMenuSaveLoadController: PokerGame не найден.");
            return;
        }

        try
        {
            string json = File.ReadAllText(selectedSavePath);
            PokerSimpleSaveData data = JsonUtility.FromJson<PokerSimpleSaveData>(json);

            if (data == null)
                return;

            game.LoadSimpleSave(data);

            if (pauseMenuController != null)
                pauseMenuController.BackToPauseRoot();
        }
        catch (Exception e)
        {
            Debug.LogError($"LoadSelectedSave error: {e}");
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
            File.Delete(selectedSavePath);
            selectedSavePath = null;
            RefreshSaveList();
        }
        catch (Exception e)
        {
            Debug.LogError($"DeleteSelectedSave error: {e}");
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