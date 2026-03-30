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
    [SerializeField] private GameObject pauseRootButtons;
    [SerializeField] private GameObject savePanel;
    [SerializeField] private GameObject loadPanel;

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
    }

    private void Start()
    {
        if (savePanel != null) savePanel.SetActive(false);
        if (loadPanel != null) loadPanel.SetActive(false);

        if (loadSelectedButton != null)
            loadSelectedButton.interactable = false;

        if (deleteSelectedButton != null)
            deleteSelectedButton.interactable = false;
    }

    public void OpenSavePanel()
    {
        if (pauseRootButtons != null) pauseRootButtons.SetActive(false);
        if (loadPanel != null) loadPanel.SetActive(false);
        if (savePanel != null) savePanel.SetActive(true);

        if (saveStatusText != null)
            saveStatusText.text = "";
    }

    public void OpenLoadPanel()
    {
        if (pauseRootButtons != null) pauseRootButtons.SetActive(false);
        if (savePanel != null) savePanel.SetActive(false);
        if (loadPanel != null) loadPanel.SetActive(true);

        RefreshSaveList();
    }

    public void BackToRoot()
    {
        if (savePanel != null) savePanel.SetActive(false);
        if (loadPanel != null) loadPanel.SetActive(false);
        if (pauseRootButtons != null) pauseRootButtons.SetActive(true);
    }

    public void SaveNow()
    {
        if (game == null)
        {
            if (saveStatusText != null)
                saveStatusText.text = "Ошибка: PokerGame не найден";
            return;
        }

        if (!Directory.Exists(SaveFolderPath))
            Directory.CreateDirectory(SaveFolderPath);

        PokerSimpleSaveData data = new PokerSimpleSaveData
        {
            saveId = System.Guid.NewGuid().ToString(),
            saveName = "Сейв " + System.DateTime.Now.ToString("HH:mm:ss"),
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            createdAt = System.DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
            playerHp = game.GetPlayerHP(),
            aiHp = game.GetAIHP(),
            playerIsDealer = game.GetPlayerIsDealer()
        };

        string fileName = "save_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
        string filePath = Path.Combine(SaveFolderPath, fileName);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);

        if (saveStatusText != null)
            saveStatusText.text = "Игра успешно сохранена";
    }

    public void RefreshSaveList()
    {
        selectedSavePath = null;

        if (loadSelectedButton != null)
            loadSelectedButton.interactable = false;

        if (deleteSelectedButton != null)
            deleteSelectedButton.interactable = false;

        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] != null)
                Destroy(spawnedSlots[i].gameObject);
        }
        spawnedSlots.Clear();

        if (!Directory.Exists(SaveFolderPath))
        {
            if (emptyText != null)
                emptyText.gameObject.SetActive(true);
            return;
        }

        string[] files = Directory.GetFiles(SaveFolderPath, "*.json");

        if (files.Length == 0)
        {
            if (emptyText != null)
                emptyText.gameObject.SetActive(true);
            return;
        }

        if (emptyText != null)
            emptyText.gameObject.SetActive(false);

        foreach (string file in files)
        {
            string json = File.ReadAllText(file);
            PokerSimpleSaveData data = JsonUtility.FromJson<PokerSimpleSaveData>(json);

            if (data == null)
                continue;

            PauseSaveSlotUI slot = Instantiate(saveSlotPrefab, contentRoot);
            slot.Setup(data, file, this);
            slot.SetSelected(false);
            spawnedSlots.Add(slot);
        }
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
        if (string.IsNullOrEmpty(selectedSavePath) || !File.Exists(selectedSavePath) || game == null)
            return;

        string json = File.ReadAllText(selectedSavePath);
        PokerSimpleSaveData data = JsonUtility.FromJson<PokerSimpleSaveData>(json);

        if (data == null) return;

        game.LoadSimpleSave(data);
        BackToRoot();
    }

    public void DeleteSelectedSave()
    {
        if (string.IsNullOrEmpty(selectedSavePath) || !File.Exists(selectedSavePath))
            return;

        File.Delete(selectedSavePath);
        selectedSavePath = null;
        RefreshSaveList();
    }
}