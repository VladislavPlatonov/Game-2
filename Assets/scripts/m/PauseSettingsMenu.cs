using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseSettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Button volumeMinusButton;
    [SerializeField] private Button volumePlusButton;
    [SerializeField] private RectTransform volumeBarFill;
    [SerializeField] private TextMeshProUGUI volumePercentText;

    [Header("Resolution")]
    [SerializeField] private Button resolutionCurrentButton;
    [SerializeField] private TextMeshProUGUI resolutionCurrentText;
    [SerializeField] private GameObject resolutionDropdownPanel;

    [Header("Resolution List")]
    [SerializeField] private Transform resolutionListRoot;
    [SerializeField] private GameObject resolutionButtonPrefab;

    [Header("Settings")]
    [SerializeField] private float volumeStep = 0.1f;

    private List<Resolution> resolutions = new List<Resolution>();

    private float volume = 1f;
    private float visualVolume = 1f;

    private int currentResolutionIndex = 0;

    private const string VolumeKey = "volume";
    private const string WidthKey = "res_w";
    private const string HeightKey = "res_h";

    private void Start()
    {
        volumeMinusButton.onClick.AddListener(() => ChangeVolume(-volumeStep));
        volumePlusButton.onClick.AddListener(() => ChangeVolume(volumeStep));

        resolutionCurrentButton.onClick.AddListener(ToggleDropdown);

        LoadResolutions();
        LoadSettings();
        BuildResolutionList();

        visualVolume = volume;

        RefreshUI();

        resolutionDropdownPanel.SetActive(false);
    }

    private void Update()
    {
        AnimateVolumeBar();
    }

    // ================= 🔊 ГРОМКОСТЬ =================

    private void ChangeVolume(float delta)
    {
        volume = Mathf.Clamp01(volume + delta);

        AudioListener.volume = volume;

        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();

        RefreshVolumeText();
    }

    private void AnimateVolumeBar()
    {
        // ПЛАВНАЯ АНИМАЦИЯ (железно работает)
        visualVolume = Mathf.Lerp(visualVolume, volume, Time.unscaledDeltaTime * 10f);

        if (volumeBarFill != null)
            volumeBarFill.localScale = new Vector3(visualVolume, 1f, 1f);
    }

    private void RefreshVolumeText()
    {
        if (volumePercentText != null)
            volumePercentText.text = Mathf.RoundToInt(volume * 100f) + "%";
    }

    // ================= 🖥 РАЗРЕШЕНИЯ =================

    private void LoadResolutions()
    {
        resolutions.Clear();
        HashSet<string> seen = new HashSet<string>();

        foreach (var r in Screen.resolutions)
        {
            string key = r.width + "x" + r.height;

            if (seen.Contains(key)) continue;

            seen.Add(key);
            resolutions.Add(r);
        }
    }

    private void BuildResolutionList()
    {
        foreach (Transform child in resolutionListRoot)
            Destroy(child.gameObject);

        for (int i = 0; i < resolutions.Count; i++)
        {
            var r = resolutions[i];

            GameObject btnObj = Instantiate(resolutionButtonPrefab, resolutionListRoot);
            btnObj.SetActive(true);

            TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            Button btn = btnObj.GetComponent<Button>();

            txt.text = $"{r.width}x{r.height}";

            int index = i;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnResolutionSelected(index));
        }
    }

    private void OnResolutionSelected(int index)
    {
        currentResolutionIndex = index;

        var r = resolutions[index];
        Screen.SetResolution(r.width, r.height, true);

        PlayerPrefs.SetInt(WidthKey, r.width);
        PlayerPrefs.SetInt(HeightKey, r.height);
        PlayerPrefs.Save();

        RefreshResolution();

        resolutionDropdownPanel.SetActive(false);
        Canvas.ForceUpdateCanvases();
    }

    private void RefreshResolution()
    {
        if (resolutionCurrentText != null && resolutions.Count > 0)
        {
            var r = resolutions[currentResolutionIndex];
            resolutionCurrentText.text = $"{r.width}x{r.height}";
        }
    }

    private void ToggleDropdown()
    {
        resolutionDropdownPanel.SetActive(!resolutionDropdownPanel.activeSelf);
        Canvas.ForceUpdateCanvases();
    }

    // ================= 💾 ЗАГРУЗКА =================

    private void LoadSettings()
    {
        volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        AudioListener.volume = volume;

        int w = PlayerPrefs.GetInt(WidthKey, Screen.currentResolution.width);
        int h = PlayerPrefs.GetInt(HeightKey, Screen.currentResolution.height);

        for (int i = 0; i < resolutions.Count; i++)
        {
            if (resolutions[i].width == w && resolutions[i].height == h)
            {
                currentResolutionIndex = i;
                break;
            }
        }
    }

    private void RefreshUI()
    {
        RefreshVolumeText();
        RefreshResolution();
    }
}
