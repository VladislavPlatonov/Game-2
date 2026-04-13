using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Main Pause Panel (container)")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Root content inside pause menu (buttons)")]
    [SerializeField] private GameObject pauseRootContent;

    [Header("Sub-panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject savePanel;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private GameObject exitConfirmPanel;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject gameplayButtonsPanel;

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreenPanel;

    [Header("Save / Load Logic")]
    [SerializeField] private PauseMenuSaveLoadController saveLoadController;

    [Header("Exit -> Main Menu Scene")]
    [SerializeField] private string mainMenuSceneName = "SampleScene";

    [Header("Input")]
    [SerializeField] private float escBlockSeconds = 0.18f;

    [Header("Animation")]
    [SerializeField] private float panelHideDelay = 0.20f;

    private bool isPaused;
    private bool isTransitioning;
    private float escBlockUntil;

    private void Start()
    {
        HideAllInPauseImmediate();

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (loadingScreenPanel != null)
            loadingScreenPanel.SetActive(false);

        SetGameplayButtonsVisible(true);
        SetPaused(false);
    }

    private void Update()
    {
        if (isTransitioning)
            return;

        if (Time.unscaledTime < escBlockUntil)
            return;

        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (!isPaused)
        {
            OpenPauseMenu();
            return;
        }

        if (IsAnySubPanelOpen())
            BackToPauseRoot();
        else
            ResumeGame();
    }

    public void OpenPauseMenu()
    {
        if (isTransitioning) return;

        StartCoroutine(OpenPauseMenuRoutine());
    }

    public void ResumeGame()
    {
        if (isTransitioning) return;

        StartCoroutine(ResumeGameRoutine());
    }

    public void OpenSettings()
    {
        if (isTransitioning) return;

        BlockEscShort();
        SetPaused(true);
        EnsurePauseVisible();
        SetGameplayButtonsVisible(false);

        HidePanel(pauseRootContent);
        HidePanel(savePanel);
        HidePanel(loadPanel);
        HidePanel(exitConfirmPanel);

        ShowPanel(settingsPanel);
    }

    public void OpenSave()
    {
        if (isTransitioning) return;

        BlockEscShort();
        SetPaused(true);
        EnsurePauseVisible();
        SetGameplayButtonsVisible(false);

        HidePanel(pauseRootContent);
        HidePanel(settingsPanel);
        HidePanel(loadPanel);
        HidePanel(exitConfirmPanel);

        ShowPanel(savePanel);

        if (saveLoadController != null)
            saveLoadController.OnSavePanelOpened();
    }

    public void OpenLoad()
    {
        if (isTransitioning) return;

        BlockEscShort();
        SetPaused(true);
        EnsurePauseVisible();
        SetGameplayButtonsVisible(false);

        HidePanel(pauseRootContent);
        HidePanel(settingsPanel);
        HidePanel(savePanel);
        HidePanel(exitConfirmPanel);

        ShowPanel(loadPanel);

        if (saveLoadController != null)
            saveLoadController.OnLoadPanelOpened();
    }

    public void OpenExitConfirm()
    {
        if (isTransitioning) return;

        BlockEscShort();
        SetPaused(true);
        EnsurePauseVisible();
        SetGameplayButtonsVisible(false);

        HidePanel(pauseRootContent);
        HidePanel(settingsPanel);
        HidePanel(savePanel);
        HidePanel(loadPanel);

        ShowPanel(exitConfirmPanel);
    }

    public void BackToPauseRoot()
    {
        if (isTransitioning) return;

        BlockEscShort();
        SetPaused(true);
        EnsurePauseVisible();
        SetGameplayButtonsVisible(false);

        HidePanel(settingsPanel);
        HidePanel(savePanel);
        HidePanel(loadPanel);
        HidePanel(exitConfirmPanel);

        ShowPanel(pauseRootContent);
    }

    public void ExitConfirmYes()
    {
        if (isTransitioning) return;

        StartCoroutine(ExitToMainMenuRoutine());
    }

    public void ExitConfirmNo()
    {
        if (isTransitioning) return;

        BackToPauseRoot();
    }

    private IEnumerator OpenPauseMenuRoutine()
    {
        isTransitioning = true;
        BlockEscShort();

        SetPaused(true);
        SetGameplayButtonsVisible(false);

        EnsurePauseVisible();
        ShowRootOnly();

        yield return null;

        isTransitioning = false;
    }

    private IEnumerator ResumeGameRoutine()
    {
        isTransitioning = true;
        BlockEscShort();

        HidePanel(pauseRootContent);
        HidePanel(settingsPanel);
        HidePanel(savePanel);
        HidePanel(loadPanel);
        HidePanel(exitConfirmPanel);

        if (pauseMenuPanel != null)
            HidePanel(pauseMenuPanel);

        yield return new WaitForSecondsRealtime(panelHideDelay);

        SetPaused(false);
        SetGameplayButtonsVisible(true);

        isTransitioning = false;
    }

    private IEnumerator ExitToMainMenuRoutine()
    {
        isTransitioning = true;
        BlockEscShort();

        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetGameplayButtonsVisible(false);

        HidePanel(pauseRootContent);
        HidePanel(settingsPanel);
        HidePanel(savePanel);
        HidePanel(loadPanel);
        HidePanel(exitConfirmPanel);

        if (pauseMenuPanel != null)
            HidePanel(pauseMenuPanel);

        if (loadingScreenPanel != null)
            loadingScreenPanel.SetActive(true);

        yield return new WaitForSecondsRealtime(panelHideDelay);
        yield return new WaitForSecondsRealtime(0.5f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(mainMenuSceneName);

        while (!operation.isDone)
            yield return null;
    }

    private void EnsurePauseVisible()
    {
        if (pauseMenuPanel != null && !pauseMenuPanel.activeSelf)
            pauseMenuPanel.SetActive(true);
    }

    private void ShowRootOnly()
    {
        HidePanel(settingsPanel);
        HidePanel(savePanel);
        HidePanel(loadPanel);
        HidePanel(exitConfirmPanel);

        ShowPanel(pauseRootContent);
    }

    private void HideAllInPauseImmediate()
    {
        SetPanelImmediate(pauseRootContent, false);
        SetPanelImmediate(settingsPanel, false);
        SetPanelImmediate(savePanel, false);
        SetPanelImmediate(loadPanel, false);
        SetPanelImmediate(exitConfirmPanel, false);
    }

    private bool IsAnySubPanelOpen()
    {
        return (settingsPanel != null && settingsPanel.activeSelf)
            || (savePanel != null && savePanel.activeSelf)
            || (loadPanel != null && loadPanel.activeSelf)
            || (exitConfirmPanel != null && exitConfirmPanel.activeSelf);
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel == null)
            return;

        if (!panel.activeSelf)
            panel.SetActive(true);
    }

    private void HidePanel(GameObject panel)
    {
        if (panel == null)
            return;

        PauseMenuPanelFX fx = panel.GetComponent<PauseMenuPanelFX>();
        if (fx != null && panel.activeSelf)
            fx.HideAnimated();
        else
            panel.SetActive(false);
    }

    private void SetPanelImmediate(GameObject panel, bool state)
    {
        if (panel == null)
            return;

        panel.SetActive(state);
    }

    private void SetGameplayButtonsVisible(bool visible)
    {
        if (gameplayButtonsPanel != null)
            gameplayButtonsPanel.SetActive(visible);
    }

    private void BlockEscShort()
    {
        escBlockUntil = Time.unscaledTime + escBlockSeconds;
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;

        if (Poker.PokerBootstrap.Instance != null)
            Poker.PokerBootstrap.Instance.SetPaused(paused);
        else
            Time.timeScale = paused ? 0f : 1f;
    }
}
