using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BottomActionButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Refs")]
    [SerializeField] private RectTransform textRoot;
    [SerializeField] private TextMeshProUGUI targetText;

    [Header("Scale")]
    [SerializeField] private float normalTextScale = 1f;
    [SerializeField] private float hoverTextScale = 1.10f;
    [SerializeField] private float pressedTextScale = 0.96f;
    [SerializeField] private float textScaleSpeed = 14f;

    [Header("Color")]
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color hoverTextColor = new Color(0.55f, 0.85f, 1f, 1f);
    [SerializeField] private float textColorSpeed = 14f;

    [Header("Wobble")]
    [SerializeField] private float wobbleAngle = 6f;
    [SerializeField] private float wobbleSpeed = 7f;
    [SerializeField] private float wobbleScalePulse = 0.025f;

    private bool isHovered;
    private bool isPressed;

    private Vector3 baseScale;
    private Quaternion baseRotation;

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (textRoot == null && targetText != null)
            textRoot = targetText.rectTransform;

        if (textRoot != null)
        {
            baseScale = textRoot.localScale;
            baseRotation = textRoot.localRotation;
        }

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
        UpdateTextVisuals();
    }

    private void UpdateTextVisuals()
    {
        if (textRoot == null)
            return;

        float targetScaleMultiplier = normalTextScale;

        if (isPressed)
            targetScaleMultiplier = pressedTextScale;
        else if (isHovered)
            targetScaleMultiplier = hoverTextScale;

        float wobbleRot = 0f;
        float wobblePulse = 0f;

        if (isHovered && !isPressed)
        {
            float t = Time.unscaledTime * wobbleSpeed;
            wobbleRot = Mathf.Sin(t) * wobbleAngle;
            wobblePulse = Mathf.Abs(Mathf.Sin(t * 0.8f)) * wobbleScalePulse;
        }

        Vector3 targetScale = baseScale * (targetScaleMultiplier + wobblePulse);
        Quaternion targetRotation = baseRotation * Quaternion.Euler(0f, 0f, wobbleRot);

        textRoot.localScale = Vector3.Lerp(
            textRoot.localScale,
            targetScale,
            Time.unscaledDeltaTime * textScaleSpeed
        );

        textRoot.localRotation = Quaternion.Lerp(
            textRoot.localRotation,
            targetRotation,
            Time.unscaledDeltaTime * textScaleSpeed
        );

        if (targetText != null)
        {
            Color targetColor = isHovered ? hoverTextColor : normalTextColor;
            targetText.color = Color.Lerp(
                targetText.color,
                targetColor,
                Time.unscaledDeltaTime * textColorSpeed
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
        {
            textRoot.localScale = baseScale * normalTextScale;
            textRoot.localRotation = baseRotation;
        }

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
