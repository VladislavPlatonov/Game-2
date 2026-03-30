using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI (optional)")]
    [SerializeField] private Slider volumeSlider;        // 0..1
    [SerializeField] private Dropdown qualityDropdown;    // качество
    [SerializeField] private Dropdown resolutionDropdown; // разрешение
    [SerializeField] private Toggle fullscreenToggle;

    private Resolution[] resolutions;

    private void Start()
    {
        // Volume
        if (volumeSlider != null)
        {
            float v = PlayerPrefs.GetFloat("settings_volume", 1f);
            volumeSlider.value = v;
            ApplyVolume(v);
            volumeSlider.onValueChanged.AddListener(ApplyVolume);
        }

        // Quality
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string>(QualitySettings.names));

            int q = PlayerPrefs.GetInt("settings_quality", QualitySettings.GetQualityLevel());
            qualityDropdown.value = q;
            qualityDropdown.RefreshShownValue();
            ApplyQuality(q);
            qualityDropdown.onValueChanged.AddListener(ApplyQuality);
        }

        // Resolutions
        if (resolutionDropdown != null)
        {
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();

            var opts = new List<string>();
            int currentIndex = 0;

            for (int i = 0; i < resolutions.Length; i++)
            {
                string label = $"{resolutions[i].width}x{resolutions[i].height} @{resolutions[i].refreshRateRatio.value:0}Hz";
                opts.Add(label);

                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentIndex = i;
                }
            }

            resolutionDropdown.AddOptions(opts);

            int savedIndex = PlayerPrefs.GetInt("settings_resolution", currentIndex);
            savedIndex = Mathf.Clamp(savedIndex, 0, resolutions.Length - 1);
            resolutionDropdown.value = savedIndex;
            resolutionDropdown.RefreshShownValue();

            ApplyResolution(savedIndex);
            resolutionDropdown.onValueChanged.AddListener(ApplyResolution);
        }

        // Fullscreen
        if (fullscreenToggle != null)
        {
            bool fs = PlayerPrefs.GetInt("settings_fullscreen", Screen.fullScreen ? 1 : 0) == 1;
            fullscreenToggle.isOn = fs;
            ApplyFullscreen(fs);
            fullscreenToggle.onValueChanged.AddListener(ApplyFullscreen);
        }
    }

    private void ApplyVolume(float v)
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat("settings_volume", v);
    }

    private void ApplyQuality(int q)
    {
        QualitySettings.SetQualityLevel(q, true);
        PlayerPrefs.SetInt("settings_quality", q);
    }

    private void ApplyResolution(int index)
    {
        if (resolutions == null || resolutions.Length == 0) return;
        var r = resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreenMode, r.refreshRateRatio);
        PlayerPrefs.SetInt("settings_resolution", index);
    }

    private void ApplyFullscreen(bool fs)
    {
        Screen.fullScreen = fs;
        PlayerPrefs.SetInt("settings_fullscreen", fs ? 1 : 0);
    }
}