using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseSaveSlotUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Button selectButton;
    [SerializeField] private Graphic slotBack;

    [SerializeField] private RawImage screenshotImage;
    [SerializeField] private TextMeshProUGUI saveNameText;
    [SerializeField] private TextMeshProUGUI saveDateText;
    [SerializeField] private TextMeshProUGUI saveSceneText;

    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;

    [Header("Colors")]
    [SerializeField] private Color normalBackColor = new Color(0.12f, 0.12f, 0.12f, 0.82f);
    [SerializeField] private Color selectedBackColor = new Color(0.22f, 0.22f, 0.22f, 0.95f);

    private string savePath;
    private PauseMenuSaveLoadController owner;

    public void Setup(PokerUnifiedSaveData data, string filePath, PauseMenuSaveLoadController controller, int displayIndex)
    {
        savePath = filePath;
        owner = controller;

        if (saveNameText != null)
            saveNameText.text = $"ÑÅÉÂ {displayIndex}";

        if (saveDateText != null)
            saveDateText.text = data.createdAt;

        if (saveSceneText != null)
            saveSceneText.text = "Ñöåíà: " + data.sceneName;

        LoadScreenshot(data);

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelectClicked);
        }

        if (loadButton != null)
        {
            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(OnLoadClicked);
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDeleteClicked);
        }

        SetSelected(false);
    }

    private void LoadScreenshot(PokerUnifiedSaveData data)
    {
        if (screenshotImage == null)
            return;

        screenshotImage.texture = null;
        screenshotImage.color = Color.white;

        if (data == null || string.IsNullOrEmpty(data.screenshotFileName))
            return;

        string saveFolder = Path.Combine(Application.persistentDataPath, "saves");
        string screenshotPath = Path.Combine(saveFolder, data.screenshotFileName);

        if (!File.Exists(screenshotPath))
            return;

        byte[] bytes = File.ReadAllBytes(screenshotPath);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);

        if (!tex.LoadImage(bytes))
            return;

        screenshotImage.texture = tex;
        screenshotImage.color = Color.white;
    }

    private void OnSelectClicked()
    {
        if (owner != null)
            owner.SelectSave(savePath, this);
    }

    private void OnLoadClicked()
    {
        if (owner != null)
        {
            owner.SelectSave(savePath, this);
            owner.LoadSelectedSave();
        }
    }

    private void OnDeleteClicked()
    {
        if (owner != null)
        {
            owner.SelectSave(savePath, this);
            owner.DeleteSelectedSave();
        }
    }

    public void SetSelected(bool selected)
    {
        if (slotBack == null) return;
        slotBack.color = selected ? selectedBackColor : normalBackColor;
    }
}
