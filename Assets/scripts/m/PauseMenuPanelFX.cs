using System.Collections;
using UnityEngine;

public class PauseMenuPanelFX : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform menuRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Appear / Hide")]
    [SerializeField] private Vector2 hiddenOffset = new Vector2(0f, 40f);
    [SerializeField] private float hiddenScale = 0.94f;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float fadeSpeed = 10f;

    private Vector2 shownPos;
    private Vector2 hiddenPos;
    private Vector3 shownScale;
    private Vector3 hiddenScaleVec;

    private Coroutine transitionRoutine;
    private bool initialized;

    private void Awake()
    {
        InitializeIfNeeded();
    }

    private void InitializeIfNeeded()
    {
        if (initialized) return;

        if (menuRoot == null)
            menuRoot = transform as RectTransform;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        shownPos = menuRoot.anchoredPosition;
        hiddenPos = shownPos + hiddenOffset;

        shownScale = Vector3.one;
        hiddenScaleVec = Vector3.one * hiddenScale;

        initialized = true;
    }

    private void OnEnable()
    {
        InitializeIfNeeded();

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        menuRoot.anchoredPosition = hiddenPos;
        menuRoot.localScale = hiddenScaleVec;
        canvasGroup.alpha = 0f;

        transitionRoutine = StartCoroutine(ShowRoutine());
    }

    public void HideAnimated()
    {
        InitializeIfNeeded();

        if (!gameObject.activeInHierarchy)
            return;

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        while (Vector2.Distance(menuRoot.anchoredPosition, shownPos) > 0.1f ||
               Mathf.Abs(canvasGroup.alpha - 1f) > 0.01f ||
               Vector3.Distance(menuRoot.localScale, shownScale) > 0.001f)
        {
            menuRoot.anchoredPosition = Vector2.Lerp(
                menuRoot.anchoredPosition,
                shownPos,
                Time.unscaledDeltaTime * moveSpeed
            );

            menuRoot.localScale = Vector3.Lerp(
                menuRoot.localScale,
                shownScale,
                Time.unscaledDeltaTime * moveSpeed
            );

            canvasGroup.alpha = Mathf.Lerp(
                canvasGroup.alpha,
                1f,
                Time.unscaledDeltaTime * fadeSpeed
            );

            yield return null;
        }

        menuRoot.anchoredPosition = shownPos;
        menuRoot.localScale = shownScale;
        canvasGroup.alpha = 1f;
        transitionRoutine = null;
    }

    private IEnumerator HideRoutine()
    {
        while (Vector2.Distance(menuRoot.anchoredPosition, hiddenPos) > 0.1f ||
               Mathf.Abs(canvasGroup.alpha - 0f) > 0.01f ||
               Vector3.Distance(menuRoot.localScale, hiddenScaleVec) > 0.001f)
        {
            menuRoot.anchoredPosition = Vector2.Lerp(
                menuRoot.anchoredPosition,
                hiddenPos,
                Time.unscaledDeltaTime * moveSpeed
            );

            menuRoot.localScale = Vector3.Lerp(
                menuRoot.localScale,
                hiddenScaleVec,
                Time.unscaledDeltaTime * moveSpeed
            );

            canvasGroup.alpha = Mathf.Lerp(
                canvasGroup.alpha,
                0f,
                Time.unscaledDeltaTime * fadeSpeed
            );

            yield return null;
        }

        menuRoot.anchoredPosition = hiddenPos;
        menuRoot.localScale = hiddenScaleVec;
        canvasGroup.alpha = 0f;

        transitionRoutine = null;
        gameObject.SetActive(false);
    }
}
