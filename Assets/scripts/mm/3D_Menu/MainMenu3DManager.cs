using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu3DManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform mainButtonsRoot;
    [SerializeField] private Transform settingsPanel;
    [SerializeField] private Transform loadPanel;
    [SerializeField] private Transform exitPanel;
    [SerializeField] private GameObject loadingOverlay;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "bckup";

    [Header("Main buttons positions")]
    [SerializeField] private Vector3 mainButtonsShownPos = new Vector3(-3.5f, 0f, 6f);
    [SerializeField] private Vector3 mainButtonsShiftedLeftPos = new Vector3(-7.5f, 0f, 6f);

    [Header("Panel positions")]
    [SerializeField] private Vector3 panelShownPos = new Vector3(2.5f, 0f, 6f);
    [SerializeField] private Vector3 panelHiddenPos = new Vector3(8.5f, 0f, 6f);

    [Header("Animation")]
    [SerializeField] private float moveDuration = 0.35f;

    private Coroutine animationRoutine;
    private Transform currentPanel;

    private void Start()
    {
        if (mainButtonsRoot != null)
            mainButtonsRoot.localPosition = mainButtonsShownPos;

        HidePanelImmediate(settingsPanel);
        HidePanelImmediate(loadPanel);
        HidePanelImmediate(exitPanel);

        if (loadingOverlay != null)
            loadingOverlay.SetActive(false);
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
        }
    }


    public void OpenSettingsPanel()
    {
        OpenPanel(settingsPanel);
    }

    public void OpenLoadPanel()
    {
        OpenPanel(loadPanel);
    }

    public void OpenExitPanel()
    {
        OpenPanel(exitPanel);
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
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OpenPanel(Transform panel)
    {
        if (panel == null) return;

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(OpenPanelRoutine(panel));
    }

    private IEnumerator OpenPanelRoutine(Transform panelToOpen)
    {
        if (currentPanel != null && currentPanel != panelToOpen)
        {
            currentPanel.localPosition = panelHiddenPos;
            currentPanel.gameObject.SetActive(false);
        }

        currentPanel = panelToOpen;
        currentPanel.gameObject.SetActive(true);
        currentPanel.localPosition = panelHiddenPos;

        float time = 0f;

        Vector3 mainStart = mainButtonsRoot.localPosition;
        Vector3 panelStart = currentPanel.localPosition;

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

        Vector3 mainStart = mainButtonsRoot.localPosition;
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