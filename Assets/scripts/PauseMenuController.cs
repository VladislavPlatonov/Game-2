using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Main Pause Panel (container)")]
    [SerializeField] private GameObject pauseMenuPanel;   // общий контейнер паузы (НЕ выключаем при переключении)

    [Header("Root content inside pause menu (buttons)")]
    [SerializeField] private GameObject pauseRootContent; // контейнер с Continue/Save/Load/Settings/Exit (его скрываем)

    [Header("Sub-panels (often children of pauseMenuPanel)")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject savePanel;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private GameObject exitConfirmPanel;


    [Header("Optional: lock cursor while playing")]
    [SerializeField] private bool lockCursorWhenUnpaused = true;

    [Header("Exit -> Main Menu Scene")]
    [SerializeField] private string mainMenuSceneName = "SampleScene";

    [Header("Anti double input")]
    [SerializeField] private float escBlockSeconds = 0.15f;

    private bool isPaused;
    private float escBlockUntil;

    private void Start()
    {
        // старт: меню скрыто
        HideAllInPause();
        pauseMenuPanel?.SetActive(false);
        SetPaused(false);
    }

    private void Update()
    {
        if (Time.unscaledTime < escBlockUntil) return;

        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (!isPaused)
        {
            OpenPauseMenu();
            return;
        }

        // В паузе: если открыт подпункт -> назад в корень, иначе продолжить
        if (IsAnySubPanelOpen())
            BackToPauseRoot();
        else
            ResumeGame();
    }

    // ---------------- Buttons ----------------

    public void OpenPauseMenu()
    {
        BlockEscShort();
        SetPaused(true);

        if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
        ShowRootOnly();
    }

    public void ResumeGame()
    {
        BlockEscShort();
        HideAllInPause();
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
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
    }

    public void OpenLoad()
    {
        BlockEscShort();
        SetPaused(true);
        EnsurePauseVisible();
        ShowSubPanel(loadPanel);
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

        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExitConfirmNo()
    {
        BlockEscShort();
        BackToPauseRoot();
    }

    // ---------------- Internals ----------------

    private void EnsurePauseVisible()
    {
        if (pauseMenuPanel && !pauseMenuPanel.activeSelf)
            pauseMenuPanel.SetActive(true);
    }

    private void ShowRootOnly()
    {
        // показываем кнопки, скрываем все подпункты
        if (pauseRootContent) pauseRootContent.SetActive(true);

        if (settingsPanel) settingsPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(false);
        if (exitConfirmPanel) exitConfirmPanel.SetActive(false);
    }

    private void ShowSubPanel(GameObject panel)
    {
        // скрыть кнопки, показать конкретный подпункт
        if (pauseRootContent) pauseRootContent.SetActive(false);

        if (settingsPanel) settingsPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(false);
        if (exitConfirmPanel) exitConfirmPanel.SetActive(false);

        if (panel) panel.SetActive(true);
    }

    private void HideAllInPause()
    {
        if (pauseRootContent) pauseRootContent.SetActive(false);

        if (settingsPanel) settingsPanel.SetActive(false);
        if (savePanel) savePanel.SetActive(false);
        if (loadPanel) loadPanel.SetActive(false);
        if (exitConfirmPanel) exitConfirmPanel.SetActive(false);
    }

    private bool IsAnySubPanelOpen()
    {
        return (settingsPanel && settingsPanel.activeSelf)
            || (savePanel && savePanel.activeSelf)
            || (loadPanel && loadPanel.activeSelf)
            || (exitConfirmPanel && exitConfirmPanel.activeSelf);
    }

    private void BlockEscShort()
    {
        escBlockUntil = Time.unscaledTime + escBlockSeconds;
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;

        // курсор + Time.timeScale будет управлять Bootstrap
        if (Poker.PokerBootstrap.Instance != null)
        {
            Poker.PokerBootstrap.Instance.SetPaused(paused);
        }
        else
            Time.timeScale = paused ? 0f : 1f;
    }
}

