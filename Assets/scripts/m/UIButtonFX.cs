using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UIButtonFX : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Refs")]
    [SerializeField] private RectTransform textRoot;
    [SerializeField] private TextMeshProUGUI targetText;

    [Header("Animation")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float pressScale = 0.92f;
    [SerializeField] private float smoothTime = 0.08f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.3f, 0.6f, 1f);

    private float targetScale = 1f;
    private float velocity;
    private Vector3 baseScale;

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (textRoot == null && targetText != null)
            textRoot = targetText.rectTransform;

        if (textRoot != null)
            baseScale = textRoot.localScale;
    }

    private void OnEnable()
    {
        targetScale = 1f;

        if (textRoot != null)
            textRoot.localScale = baseScale;

        if (targetText != null)
            targetText.color = normalColor;
    }

    private void Update()
    {
        if (textRoot == null)
            return;

        float current = textRoot.localScale.x / Mathf.Max(0.0001f, baseScale.x);

        float scale = Mathf.SmoothDamp(
            current,
            targetScale,
            ref velocity,
            smoothTime
        );

        textRoot.localScale = baseScale * scale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = hoverScale;

        if (targetText != null)
            targetText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = 1f;

        if (targetText != null)
            targetText.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = pressScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = hoverScale;
    }
}
