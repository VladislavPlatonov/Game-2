using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu3DManager : MonoBehaviour
{
    [Header("Main Buttons Root")]
    [SerializeField] private Transform mainButtonsRoot;

    [Header("Panels")]
    [SerializeField] private Transform settingsPanel;
    [SerializeField] private Transform loadPanel;
    [SerializeField] private Transform exitConfirmPanel;

    [Header("Loading Overlay")]
    [SerializeField] private GameObject loadingOverlay;

    [Header("Scene Loading")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Panel Animation")]
    [SerializeField] private float moveDuration = 0.35f;
    [SerializeField] private Vector3 mainButtonsShownPos = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 mainButtonsShiftedLeftPos = new Vector3(-3.5f, 0f, 0f);
    [SerializeField] private Vector3 panelHiddenPos = new Vector3(6f, 0f, 0f);
    [SerializeField] private Vector3 panelShownPos = new Vector3(0f, 0f, 0f);

    [Header("Volume UI")]
    [SerializeField] private TextMeshPro volumePercentText;
    [SerializeField] private Transform volumeBarFill;

    [Header("Volume Values")]
    [SerializeField][Range(0f, 1f)] private float currentVolume = 1f;
    [SerializeField] private float volumeStep = 0.1f;

    [Header("Volume Bar Geometry")]
    [SerializeField] private float minFillScaleX = 0.2f;
    [SerializeField] private float maxFillScaleX = 9f;
    [SerializeField] private float leftAnchorX = 3.45f;

    [Header("Volume Animation")]
    [SerializeField] private float fillSmoothSpeed = 10f;

    private float displayedVolume = 1f;

    private Transform currentPanel;
    private Coroutine animationRoutine;

    private void Start()
    {
        if (mainButtonsRoot != null)
            mainButtonsRoot.localPosition = mainButtonsShownPos;

        HidePanelImmediate(settingsPanel);
        HidePanelImmediate(loadPanel);
        HidePanelImmediate(exitConfirmPanel);

        currentVolume = Mathf.Clamp01(AudioListener.volume);
        displayedVolume = currentVolume;
        RefreshVolumeText();
        RefreshVolumeBarImmediate();

        if (loadingOverlay != null)
            loadingOverlay.SetActive(false);
    }

    private void Update()
    {
        AnimateVolumeBar();
    }

    public void ExecuteAction(Menu3DActionType actionType)
    {
        switch (actionType)
        {
            case Menu3DActionType.NewGame:
                StartNewGame();
                break;

            case Menu3DActionType.Load:
                OpenLoadPanel();
                break;

            case Menu3DActionType.Settings:
                OpenSettingsPanel();
                break;

            case Menu3DActionType.Exit:
                OpenExitPanel();
                break;

            case Menu3DActionType.BackToMain:
                BackToMain();
                break;

            case Menu3DActionType.QuitGame:
                QuitGame();
                break;

            case Menu3DActionType.VolumeDown:
                VolumeDown();
                break;

            case Menu3DActionType.VolumeUp:
                VolumeUp();
                break;
        }
    }

    public void OpenSettingsPanel()
    {
        ShowPanel(settingsPanel);
    }

    public void OpenLoadPanel()
    {
        ShowPanel(loadPanel);
    }

    public void OpenExitPanel()
    {
        ShowPanel(exitConfirmPanel);
    }

    public void BackToMain()
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(BackToMainRoutine());
    }

    public void StartNewGame()
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(StartNewGameRoutine());
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void VolumeDown()
    {
        currentVolume = Mathf.Clamp01(currentVolume - volumeStep);
        AudioListener.volume = currentVolume;
        RefreshVolumeText();
    }

    public void VolumeUp()
    {
        currentVolume = Mathf.Clamp01(currentVolume + volumeStep);
        AudioListener.volume = currentVolume;
        RefreshVolumeText();
    }

    public void SetVolumeNormalized(float value)
    {
        currentVolume = Mathf.Clamp01(value);
        AudioListener.volume = currentVolume;
        RefreshVolumeText();
    }

    private void RefreshVolumeText()
    {
        if (volumePercentText != null)
        {
            int percent = Mathf.RoundToInt(currentVolume * 100f);
            volumePercentText.text = percent + "%";
        }
    }

    private void RefreshVolumeBarImmediate()
    {
        if (volumeBarFill == null) return;

        float scaleX = Mathf.Lerp(minFillScaleX, maxFillScaleX, currentVolume);

        Vector3 scale = volumeBarFill.localScale;
        scale.x = scaleX;
        volumeBarFill.localScale = scale;

        Vector3 pos = volumeBarFill.localPosition;
        pos.x = leftAnchorX + (scaleX * 0.5f);
        volumeBarFill.localPosition = pos;
    }

    private void AnimateVolumeBar()
    {
        displayedVolume = Mathf.Lerp(displayedVolume, currentVolume, Time.deltaTime * fillSmoothSpeed);

        if (volumeBarFill == null) return;

        float scaleX = Mathf.Lerp(minFillScaleX, maxFillScaleX, displayedVolume);

        Vector3 scale = volumeBarFill.localScale;
        scale.x = scaleX;
        volumeBarFill.localScale = scale;

        Vector3 pos = volumeBarFill.localPosition;
        pos.x = leftAnchorX + (scaleX * 0.5f);
        volumeBarFill.localPosition = pos;
    }

    private void ShowPanel(Transform panel)
    {
        if (panel == null) return;

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(ShowPanelRoutine(panel));
    }

    private IEnumerator ShowPanelRoutine(Transform panel)
    {
        if (currentPanel != null && currentPanel != panel)
            HidePanelImmediate(currentPanel);

        currentPanel = panel;
        currentPanel.gameObject.SetActive(true);

        float time = 0f;

        Vector3 mainStart = mainButtonsRoot != null ? mainButtonsRoot.localPosition : mainButtonsShownPos;
        Vector3 panelStart = panel.localPosition;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            t = EaseOutCubic(t);

            if (mainButtonsRoot != null)
                mainButtonsRoot.localPosition = Vector3.Lerp(mainStart, mainButtonsShiftedLeftPos, t);

            if (currentPanel != null)
                currentPanel.localPosition = Vector3.Lerp(panelStart, panelShownPos, t);

            yield return null;
        }

        if (mainButtonsRoot != null)
            mainButtonsRoot.localPosition = mainButtonsShiftedLeftPos;

        if (currentPanel != null)
            currentPanel.localPosition = panelShownPos;

        animationRoutine = null;
    }

    private IEnumerator BackToMainRoutine()
    {
        Transform panelToHide = currentPanel;

        float time = 0f;

        Vector3 mainStart = mainButtonsRoot != null ? mainButtonsRoot.localPosition : mainButtonsShiftedLeftPos;
        Vector3 panelStart = panelToHide != null ? panelToHide.localPosition : panelShownPos;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            t = EaseOutCubic(t);

            if (mainButtonsRoot != null)
                mainButtonsRoot.localPosition = Vector3.Lerp(mainStart, mainButtonsShownPos, t);

            if (panelToHide != null)
                panelToHide.localPosition = Vector3.Lerp(panelStart, panelHiddenPos, t);

            yield return null;
        }

        if (mainButtonsRoot != null)
            mainButtonsRoot.localPosition = mainButtonsShownPos;

        if (panelToHide != null)
        {
            panelToHide.localPosition = panelHiddenPos;
            panelToHide.gameObject.SetActive(false);
        }

        currentPanel = null;
        animationRoutine = null;
    }

    private IEnumerator StartNewGameRoutine()
    {
        if (loadingOverlay != null)
            loadingOverlay.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(gameSceneName);
    }

    private void HidePanelImmediate(Transform panel)
    {
        if (panel == null) return;

        panel.localPosition = panelHiddenPos;
        panel.gameObject.SetActive(false);
    }

    private float EaseOutCubic(float x)
    {
        return 1f - Mathf.Pow(1f - x, 3f);
    }
}