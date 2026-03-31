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

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreenPanel;

    [Header("Save / Load Logic")]
    [SerializeField] private PauseMenuSaveLoadController saveLoadController;

    [Header("Exit -> Main Menu Scene")]
    [SerializeField] private string mainMenuSceneName = "SampleScene";

    [Header("Anti double input")]
    [SerializeField] private float escBlockSeconds = 0.15f;

    private bool isPaused;
    private float escBlockUntil;

    private void Start()
    {
        HideAllInPause();

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (loadingScreenPanel != null)
            loadingScreenPanel.SetActive(false);

        SetPaused(false);
    }

    private void Update()
    {
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
        {
            BackToPauseRoot();
        }
        else
        {
            ResumeGame();
        }
    }

    public void OpenPauseMenu()
    {
        BlockEscShort();
        SetPaused(true);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        ShowRootOnly();
    }

    public void ResumeGame()
    {
        BlockEscShort();
        HideAllInPause();

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        SetPaused(false);
    }

    public void OpenSettings()
    {
        BlockEscShort();
        SetPaused(true);
        EnsurePauseVisible();
        ShowSubPanel(settingsPanel);
    }

    public void OpenSave()
    {
        BlockEscShort();
        SetPaused(true);
        EnsurePauseVisible();
        ShowSubPanel(savePanel);

        if (saveLoadController != null)
            saveLoadController.OnSavePanelOpened();
    }

    public void OpenLoad()
    {
        BlockEscShort();
        SetPaused(true);
        EnsurePauseVisible();
        ShowSubPanel(loadPanel);

        if (saveLoadController != null)
            saveLoadController.OnLoadPanelOpened();
    }

    public void OpenExitConfirm()
    {
        BlockEscShort();
        SetPaused(true);
        EnsurePauseVisible();
        ShowSubPanel(exitConfirmPanel);
    }

    public void BackToPauseRoot()
    {
        BlockEscShort();
        SetPaused(true);
        EnsurePauseVisible();
        ShowRootOnly();
    }

    public void ExitConfirmYes()
    {
        BlockEscShort();
        StartCoroutine(ExitToMainMenuRoutine());
    }

    public void ExitConfirmNo()
    {
        BlockEscShort();
        BackToPauseRoot();
    }

    private IEnumerator ExitToMainMenuRoutine()
    {
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        HideAllInPause();

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (loadingScreenPanel != null)
            loadingScreenPanel.SetActive(true);

        yield return null;
        yield return new WaitForSecondsRealtime(1f);

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
        if (pauseRootContent != null)
            pauseRootContent.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (savePanel != null)
            savePanel.SetActive(false);

        if (loadPanel != null)
            loadPanel.SetActive(false);

        if (exitConfirmPanel != null)
            exitConfirmPanel.SetActive(false);
    }

    private void ShowSubPanel(GameObject panelToOpen)
    {
        if (pauseRootContent != null)
            pauseRootContent.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (savePanel != null)
            savePanel.SetActive(false);

        if (loadPanel != null)
            loadPanel.SetActive(false);

        if (exitConfirmPanel != null)
            exitConfirmPanel.SetActive(false);

        if (panelToOpen != null)
            panelToOpen.SetActive(true);
    }

    private void HideAllInPause()
    {
        if (pauseRootContent != null)
            pauseRootContent.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (savePanel != null)
            savePanel.SetActive(false);

        if (loadPanel != null)
            loadPanel.SetActive(false);

        if (exitConfirmPanel != null)
            exitConfirmPanel.SetActive(false);
    }

    private bool IsAnySubPanelOpen()
    {
        return (settingsPanel != null && settingsPanel.activeSelf)
            || (savePanel != null && savePanel.activeSelf)
            || (loadPanel != null && loadPanel.activeSelf)
            || (exitConfirmPanel != null && exitConfirmPanel.activeSelf);
    }

    private void BlockEscShort()
    {
        escBlockUntil = Time.unscaledTime + escBlockSeconds;
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;

        if (Poker.PokerBootstrap.Instance != null)
        {
            Poker.PokerBootstrap.Instance.SetPaused(paused);
        }
        else
        {
            Time.timeScale = paused ? 0f : 1f;
        }
    }
}