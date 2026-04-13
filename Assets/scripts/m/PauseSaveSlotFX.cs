using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseSaveSlotFX : MonoBehaviour
{
    [Header("Slot")]
    [SerializeField] private RectTransform slotRoot;
    [SerializeField] private Graphic slotBack;

    [Header("Slot Scale")]
    [SerializeField] private float normalSlotScale = 1f;
    [SerializeField] private float hoverSlotScale = 1.02f;
    [SerializeField] private float slotScaleSpeed = 10f;

    [Header("Slot Colors")]
    [SerializeField] private Color normalSlotColor = new Color(0.12f, 0.12f, 0.12f, 0.82f);
    [SerializeField] private Color hoverSlotColor = new Color(0.17f, 0.17f, 0.17f, 0.92f);

    [Header("Load Button")]
    [SerializeField] private RectTransform loadButtonRoot;
    [SerializeField] private Graphic loadButtonGraphic;
    [SerializeField] private TextMeshProUGUI loadButtonText;

    [Header("Delete Button")]
    [SerializeField] private RectTransform deleteButtonRoot;
    [SerializeField] private Graphic deleteButtonGraphic;
    [SerializeField] private TextMeshProUGUI deleteButtonText;

    [Header("Button Scale")]
    [SerializeField] private float normalButtonScale = 1f;
    [SerializeField] private float hoverButtonScale = 1.06f;
    [SerializeField] private float buttonScaleSpeed = 14f;

    [Header("Button Colors")]
    [SerializeField] private Color normalButtonColor = new Color(0.16f, 0.16f, 0.16f, 0.9f);
    [SerializeField] private Color hoverButtonColor = new Color(0.22f, 0.34f, 0.60f, 0.98f);

    [SerializeField] private Color normalButtonTextColor = Color.white;
    [SerializeField] private Color hoverButtonTextColor = new Color(0.72f, 0.90f, 1f, 1f);

    private Vector3 slotBaseScale;
    private Vector3 loadBaseScale;
    private Vector3 deleteBaseScale;

    private Camera uiCamera;

    private void Awake()
    {
        if (slotRoot == null)
            slotRoot = transform as RectTransform;

        if (slotBack == null)
            slotBack = GetComponent<Graphic>();

        slotBaseScale = slotRoot != null ? slotRoot.localScale : Vector3.one;
        loadBaseScale = loadButtonRoot != null ? loadButtonRoot.localScale : Vector3.one;
        deleteBaseScale = deleteButtonRoot != null ? deleteButtonRoot.localScale : Vector3.one;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera;

        ForceResetVisuals();
    }

    private void OnEnable()
    {
        ForceResetVisuals();
    }

    private void OnDisable()
    {
        ForceResetVisuals();
    }

    private void Update()
    {
        UpdateSlotHover();
        UpdateButtonHover(loadButtonRoot, loadButtonGraphic, loadButtonText, loadBaseScale);
        UpdateButtonHover(deleteButtonRoot, deleteButtonGraphic, deleteButtonText, deleteBaseScale);
    }

    private void UpdateSlotHover()
    {
        if (slotRoot == null)
            return;

        bool hovered = IsPointerOverRect(slotRoot);

        Vector3 targetScale = slotBaseScale * (hovered ? hoverSlotScale : normalSlotScale);
        slotRoot.localScale = Vector3.Lerp(
            slotRoot.localScale,
            targetScale,
            Time.unscaledDeltaTime * slotScaleSpeed
        );

        if (slotBack != null)
        {
            Color targetColor = hovered ? hoverSlotColor : normalSlotColor;
            slotBack.color = Color.Lerp(
                slotBack.color,
                targetColor,
                Time.unscaledDeltaTime * 12f
            );
        }
    }

    private void UpdateButtonHover(
        RectTransform buttonRoot,
        Graphic buttonGraphic,
        TextMeshProUGUI buttonText,
        Vector3 baseScale
    )
    {
        if (buttonRoot == null)
            return;

        bool hovered = IsPointerOverRect(buttonRoot);

        Vector3 targetScale = baseScale * (hovered ? hoverButtonScale : normalButtonScale);
        buttonRoot.localScale = Vector3.Lerp(
            buttonRoot.localScale,
            targetScale,
            Time.unscaledDeltaTime * buttonScaleSpeed
        );

        if (buttonGraphic != null)
        {
            Color targetColor = hovered ? hoverButtonColor : normalButtonColor;
            buttonGraphic.color = Color.Lerp(
                buttonGraphic.color,
                targetColor,
                Time.unscaledDeltaTime * 14f
            );
        }

        if (buttonText != null)
        {
            Color targetTextColor = hovered ? hoverButtonTextColor : normalButtonTextColor;
            buttonText.color = Color.Lerp(
                buttonText.color,
                targetTextColor,
                Time.unscaledDeltaTime * 14f
            );
        }
    }

    private bool IsPointerOverRect(RectTransform rect)
    {
        if (rect == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            rect,
            Input.mousePosition,
            uiCamera
        );
    }

    private void ForceResetVisuals()
    {
        if (slotRoot != null)
            slotRoot.localScale = slotBaseScale;

        if (slotBack != null)
            slotBack.color = normalSlotColor;

        if (loadButtonRoot != null)
            loadButtonRoot.localScale = loadBaseScale;

        if (deleteButtonRoot != null)
            deleteButtonRoot.localScale = deleteBaseScale;

        if (loadButtonGraphic != null)
            loadButtonGraphic.color = normalButtonColor;

        if (deleteButtonGraphic != null)
            deleteButtonGraphic.color = normalButtonColor;

        if (loadButtonText != null)
            loadButtonText.color = normalButtonTextColor;

        if (deleteButtonText != null)
            deleteButtonText.color = normalButtonTextColor;
    }
}
