using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuLoadController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private GameObject loadingScreenPanel;

    [Header("Save List")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private SaveSlotUI saveSlotPrefab;
    [SerializeField] private Button loadSelectedButton;
    [SerializeField] private Button deleteSelectedButton;
    [SerializeField] private TextMeshProUGUI emptyText;

    [Header("Scene")]
    [SerializeField] private string fallbackGameplayScene = "bckup";

    private readonly List<SaveSlotUI> spawnedSlots = new();
    private string selectedSavePath;

    public static string PendingLoadSavePath;

    private string SaveFolderPath => Path.Combine(Application.persistentDataPath, "saves");

    private void Start()
    {
        if (loadPanel != null)
            loadPanel.SetActive(false);

        if (loadSelectedButton != null)
        {
            loadSelectedButton.onClick.RemoveAllListeners();
            loadSelectedButton.onClick.AddListener(LoadSelectedSave);
            loadSelectedButton.interactable = false;
        }

        if (deleteSelectedButton != null)
        {
            deleteSelectedButton.onClick.RemoveAllListeners();
            deleteSelectedButton.onClick.AddListener(DeleteSelectedSave);
            deleteSelectedButton.interactable = false;
        }
    }

    public void OpenLoadPanel()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (loadPanel != null)
            loadPanel.SetActive(true);

        RefreshSaveList();
    }

    public void CloseLoadPanel()
    {
        if (loadPanel != null)
            loadPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
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
            SaveFileData data = JsonUtility.FromJson<SaveFileData>(json);

            if (data == null)
                continue;

            SaveSlotUI slot = Instantiate(saveSlotPrefab, contentRoot);
            slot.Setup(data, file, this);
            slot.SetSelected(false);
            spawnedSlots.Add(slot);
        }
    }

    public void SelectSave(string filePath, SaveSlotUI clickedSlot)
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
        if (string.IsNullOrEmpty(selectedSavePath) || !File.Exists(selectedSavePath))
            return;

        string json = File.ReadAllText(selectedSavePath);
        SaveFileData data = JsonUtility.FromJson<SaveFileData>(json);

        PendingLoadSavePath = selectedSavePath;

        if (loadingScreenPanel != null)
            loadingScreenPanel.SetActive(true);

        string sceneToLoad = data != null && !string.IsNullOrWhiteSpace(data.sceneName)
            ? data.sceneName
            : fallbackGameplayScene;

        SceneManager.LoadScene(sceneToLoad);
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