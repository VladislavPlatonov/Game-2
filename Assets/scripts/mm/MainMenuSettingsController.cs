using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuSettingsController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject loadingScreenPanel;
    [SerializeField] private GameObject exitConfirmPanel;

    [Header("Audio")]
    [SerializeField] private Slider volumeSlider;

    [Header("Resolution")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Scene Loading")]
    [SerializeField] private string gameplaySceneName = "bckup";

    private Resolution[] resolutions;
    private readonly List<Resolution> filteredResolutions = new();

    private const string VolumeKey = "menu_volume";
    private const string ResolutionIndexKey = "resolution_index";

    private void Start()
    {
        SetupPanels();
        SetupVolume();
        SetupResolutionDropdown();
    }

    private void SetupPanels()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (loadingScreenPanel != null)
            loadingScreenPanel.SetActive(false);

        if (exitConfirmPanel != null)
            exitConfirmPanel.SetActive(false);
    }

    private void SetupVolume()
    {
        if (volumeSlider == null) return;

        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        volumeSlider.onValueChanged.RemoveAllListeners();
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutions = Screen.resolutions;
        filteredResolutions.Clear();
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution res = resolutions[i];

            bool alreadyAdded = false;
            for (int j = 0; j < filteredResolutions.Count; j++)
            {
                if (filteredResolutions[j].width == res.width &&
                    filteredResolutions[j].height == res.height)
                {
                    alreadyAdded = true;
                    break;
                }
            }

            if (alreadyAdded)
                continue;

            filteredResolutions.Add(res);
            options.Add(res.width + " x " + res.height);

            if (res.width == Screen.currentResolution.width &&
                res.height == Screen.currentResolution.height)
            {
                currentResolutionIndex = filteredResolutions.Count - 1;
            }
        }

        resolutionDropdown.AddOptions(options);

        int savedIndex = PlayerPrefs.GetInt(ResolutionIndexKey, currentResolutionIndex);
        savedIndex = Mathf.Clamp(savedIndex, 0, Mathf.Max(0, filteredResolutions.Count - 1));

        if (filteredResolutions.Count > 0)
        {
            resolutionDropdown.value = savedIndex;
            resolutionDropdown.RefreshShownValue();
        }

        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    public void OpenSettings()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        if (exitConfirmPanel != null)
            exitConfirmPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    public void StartGame()
    {
        if (loadingScreenPanel != null)
            loadingScreenPanel.SetActive(true);

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenExitConfirm()
    {
        if (exitConfirmPanel != null)
            exitConfirmPanel.SetActive(true);
    }

    public void CloseExitConfirm()
    {
        if (exitConfirmPanel != null)
            exitConfirmPanel.SetActive(false);
    }

    public void ExitGame()
    {
        PlayerPrefs.Save();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= filteredResolutions.Count)
            return;

        Resolution resolution = filteredResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.FullScreenWindow);

        PlayerPrefs.SetInt(ResolutionIndexKey, resolutionIndex);
        PlayerPrefs.Save();
    }
}