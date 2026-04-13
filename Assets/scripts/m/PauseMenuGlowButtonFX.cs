using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenuTextOnlyButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Main Text")]
    [SerializeField] private RectTransform textRoot;
    [SerializeField] private TextMeshProUGUI targetText;

    [Header("Glow Text / Back Text")]
    [SerializeField] private RectTransform glowRoot;

    [Header("Scale")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float pressedScale = 0.95f;
    [SerializeField] private float scaleSpeed = 14f;

    [Header("Text Color")]
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color hoverTextColor = new Color(0.55f, 0.85f, 1f, 1f);
    [SerializeField] private float colorSpeed = 14f;

    private bool isHovered;
    private bool isPressed;

    private Vector3 textBaseScale;
    private Vector3 glowBaseScale;

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (textRoot == null && targetText != null)
            textRoot = targetText.rectTransform;

        if (textRoot != null)
            textBaseScale = textRoot.localScale;

        if (glowRoot != null)
            glowBaseScale = glowRoot.localScale;

        ForceResetVisuals();
    }

    private void OnEnable()
    {
        ForceResetState();
        ForceResetVisuals();
    }

    private void OnDisable()
    {
        ForceResetState();
        ForceResetVisuals();
    }

    private void Update()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        float targetScaleMul = normalScale;

        if (isPressed)
            targetScaleMul = pressedScale;
        else if (isHovered)
            targetScaleMul = hoverScale;

        if (textRoot != null)
        {
            Vector3 targetTextScale = textBaseScale * targetScaleMul;
            textRoot.localScale = Vector3.Lerp(
                textRoot.localScale,
                targetTextScale,
                Time.unscaledDeltaTime * scaleSpeed
            );
        }

        if (glowRoot != null)
        {
            Vector3 targetGlowScale = glowBaseScale * targetScaleMul;
            glowRoot.localScale = Vector3.Lerp(
                glowRoot.localScale,
                targetGlowScale,
                Time.unscaledDeltaTime * scaleSpeed
            );
        }

        if (targetText != null)
        {
            Color targetColor = isHovered ? hoverTextColor : normalTextColor;
            targetText.color = Color.Lerp(
                targetText.color,
                targetColor,
                Time.unscaledDeltaTime * colorSpeed
            );
        }
    }

    private void ForceResetState()
    {
        isHovered = false;
        isPressed = false;
    }

    private void ForceResetVisuals()
    {
        if (textRoot != null)
            textRoot.localScale = textBaseScale * normalScale;

        if (glowRoot != null)
            glowRoot.localScale = glowBaseScale * normalScale;

        if (targetText != null)
            targetText.color = normalTextColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
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