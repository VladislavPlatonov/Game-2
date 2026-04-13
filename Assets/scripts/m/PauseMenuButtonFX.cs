using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuButtonFX : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Refs")]
    [SerializeField] private RectTransform visualRoot;
    [SerializeField] private Image targetImage;
    [SerializeField] private TextMeshProUGUI targetText;

    [Header("Scale")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float pressedScale = 0.96f;
    [SerializeField] private float scaleSpeed = 12f;

    [Header("Colors")]
    [SerializeField] private Color normalImageColor = new Color(0.16f, 0.16f, 0.16f, 0.85f);
    [SerializeField] private Color hoverImageColor = new Color(0.24f, 0.24f, 0.24f, 0.95f);

    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color hoverTextColor = new Color(1f, 0.97f, 0.75f, 1f);

    [Header("Shake")]
    [SerializeField] private float hoverAnimDuration = 0.18f;
    [SerializeField] private float shakeAmountX = 1.8f;
    [SerializeField] private float shakeAmountY = 1.2f;
    [SerializeField] private float shakeSpeed = 22f;
    [SerializeField] private float punchAmount = 0.02f;

    private bool isHovered;
    private bool isPressed;
    private float hoverAnimTimer;

    private Vector2 baseAnchoredPos;
    private Vector3 baseScale;

    private void Awake()
    {
        if (visualRoot == null)
            visualRoot = transform as RectTransform;

        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (targetText == null)
            targetText = GetComponentInChildren<TextMeshProUGUI>(true);

        baseAnchoredPos = visualRoot.anchoredPosition;
        baseScale = visualRoot.localScale;

        ApplyInstantVisualState();
    }

    private void OnEnable()
    {
        ResetStateInstant();
    }

    private void OnDisable()
    {
        ResetStateInstant();
    }

    private void Update()
    {
        UpdateVisuals();
    }

    public void ForceResetState()
    {
        ResetStateInstant();
    }

    private void ResetStateInstant()
    {
        isHovered = false;
        isPressed = false;
        hoverAnimTimer = 0f;

        if (visualRoot != null)
        {
            visualRoot.anchoredPosition = baseAnchoredPos;
            visualRoot.localScale = baseScale * normalScale;
        }

        ApplyInstantVisualState();
    }

    private void ApplyInstantVisualState()
    {
        if (targetImage != null)
            targetImage.color = normalImageColor;

        if (targetText != null)
            targetText.color = normalTextColor;
    }

    private void UpdateVisuals()
    {
        if (visualRoot == null)
            return;

        Vector2 targetPos = baseAnchoredPos;
        float targetScaleMultiplier = normalScale;

        if (isPressed)
            targetScaleMultiplier = pressedScale;
        else if (isHovered)
            targetScaleMultiplier = hoverScale;

        if (hoverAnimTimer > 0f)
        {
            hoverAnimTimer -= Time.unscaledDeltaTime;

            float normalized = 1f - Mathf.Clamp01(hoverAnimTimer / hoverAnimDuration);
            float t = Time.unscaledTime * shakeSpeed;

            float jitterX = (Mathf.PerlinNoise(t * 1.7f, 0.13f) - 0.5f) * 2f * shakeAmountX;
            float jitterY = (Mathf.PerlinNoise(0.57f, t * 2.1f) - 0.5f) * 2f * shakeAmountY;

            float fadeOut = 1f - normalized;

            targetPos += new Vector2(jitterX, jitterY) * fadeOut;

            float punch = Mathf.Abs(Mathf.Sin(t * 0.45f)) * punchAmount * fadeOut;
            targetScaleMultiplier += punch;
        }

        Vector3 targetScale = baseScale * targetScaleMultiplier;

        visualRoot.anchoredPosition = Vector2.Lerp(
            visualRoot.anchoredPosition,
            targetPos,
            Time.unscaledDeltaTime * 18f
        );

        visualRoot.localScale = Vector3.Lerp(
            visualRoot.localScale,
            targetScale,
            Time.unscaledDeltaTime * scaleSpeed
        );

        if (targetImage != null)
        {
            Color imgTarget = isHovered ? hoverImageColor : normalImageColor;
            targetImage.color = Color.Lerp(targetImage.color, imgTarget, Time.unscaledDeltaTime * 14f);
        }

        if (targetText != null)
        {
            Color txtTarget = isHovered ? hoverTextColor : normalTextColor;
            targetText.color = Color.Lerp(targetText.color, txtTarget, Time.unscaledDeltaTime * 14f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        hoverAnimTimer = hoverAnimDuration;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }
}
