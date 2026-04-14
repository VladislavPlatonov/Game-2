using TMPro;
using UnityEngine;
using Poker;

public class AnimatedHpText : MonoBehaviour
{
    public enum TargetType
    {
        Player,
        AI
    }

    [Header("Refs")]
    [SerializeField] private SoulManager soulManager;
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private Transform animatedTarget;
    [SerializeField] private TargetType targetType = TargetType.Player;

    [Header("Count Animation")]
    [SerializeField] private float countDuration = 0.4f;

    [Header("Punch Animation")]
    [SerializeField] private float losePunchScale = 1.22f;
    [SerializeField] private float gainPunchScale = 1.14f;
    [SerializeField] private float punchTime = 0.12f;

    [Header("Shake")]
    [SerializeField] private float loseShakeStrength = 12f;
    [SerializeField] private float shakeTime = 0.18f;

    [Header("Colors")]
    [SerializeField] private Color highHpColor = Color.green;
    [SerializeField] private Color midHpColor = Color.yellow;
    [SerializeField] private Color lowHpColor = Color.red;

    [Header("Thresholds")]
    [SerializeField] private int highToMidThreshold = 75;
    [SerializeField] private int midToLowThreshold = 40;
    [SerializeField] private int maxHp = 100;

    private int displayedHp;
    private Vector3 baseScale;

    private void Awake()
    {
        if (soulManager == null)
            soulManager = SoulManager.Instance != null ? SoulManager.Instance : FindFirstObjectByType<SoulManager>();

        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();

        if (animatedTarget == null)
            animatedTarget = textMesh != null ? textMesh.transform : transform;

        if (animatedTarget != null)
            baseScale = animatedTarget.localScale;
    }

    private void Start()
    {
        if (soulManager == null || textMesh == null) return;

        displayedHp = targetType == TargetType.Player
            ? soulManager.GetPlayerSouls()
            : soulManager.GetAISouls();

        UpdateVisualInstant(displayedHp);
    }

    private void OnEnable()
    {
        if (soulManager == null) return;

        soulManager.OnPlayerSoulsChanged += OnPlayerSoulsChanged;
        soulManager.OnAISoulsChanged += OnAISoulsChanged;
    }

    private void OnDisable()
    {
        if (soulManager == null) return;

        soulManager.OnPlayerSoulsChanged -= OnPlayerSoulsChanged;
        soulManager.OnAISoulsChanged -= OnAISoulsChanged;
    }

    private void OnPlayerSoulsChanged(int value)
    {
        if (targetType != TargetType.Player) return;
        AnimateTo(value);
    }

    private void OnAISoulsChanged(int value)
    {
        if (targetType != TargetType.AI) return;
        AnimateTo(value);
    }

    private void AnimateTo(int targetHp)
    {
        if (textMesh == null || animatedTarget == null) return;

        int oldHp = displayedHp;

        LeanTween.cancel(gameObject);
        LeanTween.cancel(animatedTarget.gameObject);

        LTDescr countTween = LeanTween.value(gameObject, displayedHp, targetHp, countDuration)
            .setEaseOutQuad()
            .setOnUpdate((float val) =>
            {
                displayedHp = Mathf.RoundToInt(val);
                UpdateVisualInstant(displayedHp);
            });

        if (targetHp < oldHp)
        {
            LeanTween.scale(animatedTarget.gameObject, baseScale * losePunchScale, punchTime)
                .setEaseOutQuad();

            LeanTween.delayedCall(animatedTarget.gameObject, punchTime, () =>
            {
                LeanTween.scale(animatedTarget.gameObject, baseScale, punchTime).setEaseInOutQuad();
            });

            LeanTween.rotateZ(animatedTarget.gameObject, loseShakeStrength, shakeTime / 2f)
                .setEaseShake()
                .setLoopPingPong(1);
        }
        else if (targetHp > oldHp)
        {
            LeanTween.scale(animatedTarget.gameObject, baseScale * gainPunchScale, punchTime)
                .setEaseOutQuad();

            LeanTween.delayedCall(animatedTarget.gameObject, punchTime, () =>
            {
                LeanTween.scale(animatedTarget.gameObject, baseScale, punchTime).setEaseInOutQuad();
            });
        }

        countTween.setOnComplete(() =>
        {
            displayedHp = targetHp;
            UpdateVisualInstant(displayedHp);
            animatedTarget.localScale = baseScale;
        });
    }

    private void UpdateVisualInstant(int hp)
    {
        if (textMesh == null) return;

        textMesh.text = $"ÕÏ: {hp}";
        textMesh.color = EvaluateColor(hp);
    }

    private Color EvaluateColor(int hp)
    {
        int clampedHp = Mathf.Clamp(hp, 0, maxHp);

        if (clampedHp > highToMidThreshold)
        {
            float t = Mathf.InverseLerp(highToMidThreshold, maxHp, clampedHp);
            return Color.Lerp(midHpColor, highHpColor, t);
        }
        else if (clampedHp > midToLowThreshold)
        {
            float t = Mathf.InverseLerp(midToLowThreshold, highToMidThreshold, clampedHp);
            return Color.Lerp(lowHpColor, midHpColor, t);
        }
        else
        {
            return lowHpColor;
        }
    }
}
