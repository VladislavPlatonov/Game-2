using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseSaveSlotUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI saveNameText;
    [SerializeField] private TextMeshProUGUI saveDateText;
    [SerializeField] private TextMeshProUGUI saveSceneText;

    private string savePath;
    private PauseMenuSaveLoadController owner;

    public void Setup(PokerSimpleSaveData data, string filePath, PauseMenuSaveLoadController controller)
    {
        savePath = filePath;
        owner = controller;

        if (saveNameText != null)
            saveNameText.text = string.IsNullOrWhiteSpace(data.saveName) ? "Без имени" : data.saveName;

        if (saveDateText != null)
            saveDateText.text = data.createdAt;

        if (saveSceneText != null)
            saveSceneText.text = $"Сцена: {data.sceneName}";

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        owner.SelectSave(savePath, this);
    }

    public void SetSelected(bool selected)
    {
        if (background == null) return;

        background.color = selected
            ? new Color(0.8f, 0.8f, 0.8f, 0.9f)
            : new Color(1f, 1f, 1f, 0.35f);
    }
}
