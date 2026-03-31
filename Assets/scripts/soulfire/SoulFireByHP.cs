using UnityEngine;
using YourNamespace;

public class SoulFireByHP : MonoBehaviour
{
    [System.Serializable]
    public class HPVisualStage
    {
        [Header("HP Range")]
        public int minHP;
        public int maxHP;

        [Header("Base Visual")]
        public float scaleMultiplier = 1f;
        public float fireIntensity = 1f;
    }

    [Header("References")]
    [SerializeField] private Transform fireTransform;
    [SerializeField] private VFX_FireController fireController;

    [Header("Stages")]
    [SerializeField] private HPVisualStage[] stages;

    [Header("Smooth")]
    [SerializeField] private float smoothSpeed = 6f;

    [Header("Base Colors")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.8f, 1f);
    [SerializeField] private Color overloadColor = new Color(0.7f, 0.95f, 1f);
    [SerializeField] private Color dyingColor = new Color(0.08f, 0.2f, 0.5f);

    [Header("Overload Soul (150+ HP)")]
    [SerializeField] private bool enableOverloadEffect = true;
    [SerializeField] private int overloadStartHP = 150;
    [SerializeField] private int overloadMaxHP = 200;
    [SerializeField] private float overloadPulseSpeed = 5.5f;
    [SerializeField] private float minOverloadScalePulse = 0.02f;
    [SerializeField] private float maxOverloadScalePulse = 0.12f;
    [SerializeField] private float minOverloadIntensityPulse = 0.08f;
    [SerializeField] private float maxOverloadIntensityPulse = 0.50f;

    [Header("Dying Soul (<40 HP)")]
    [SerializeField] private bool enableDyingEffect = true;
    [SerializeField] private int dyingHPThreshold = 40;
    [SerializeField] private float dyingShakeSpeed = 18f;
    [SerializeField] private float minDyingShakeAmount = 0.002f;
    [SerializeField] private float maxDyingShakeAmount = 0.02f;
    [SerializeField] private float minDyingFlickerAmount = 0.05f;
    [SerializeField] private float maxDyingFlickerAmount = 0.45f;
    [SerializeField] private float dyingColorBlendStrength = 0.7f;

    private Vector3 baseScale;
    private Vector3 baseLocalPosition;

    private Vector3 targetScale;
    private Vector3 currentScale;

    private float targetIntensity = 1f;
    private float currentIntensity = 1f;

    private Color targetColor;
    private Color currentColor;

    private int currentHP;

    private void Awake()
    {
        if (fireTransform == null)
            fireTransform = transform;

        if (fireController == null)
            fireController = GetComponent<VFX_FireController>();

        baseScale = fireTransform.localScale;
        baseLocalPosition = fireTransform.localPosition;

        currentScale = baseScale;
        targetScale = baseScale;

        targetColor = normalColor;
        currentColor = normalColor;
    }

    private void Start()
    {
        ApplyImmediateVisual();
    }

    private void Update()
    {
        currentScale = Vector3.Lerp(currentScale, targetScale, Time.deltaTime * smoothSpeed);
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * smoothSpeed);
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * smoothSpeed);

        Vector3 finalScale = currentScale;
        float finalIntensity = currentIntensity;
        Color finalColor = currentColor;
        Vector3 finalLocalPosition = baseLocalPosition;

        // OVERLOAD EFFECT (150+ HP): pulse + brighter color
        if (enableOverloadEffect && currentHP >= overloadStartHP)
        {
            float overloadT = Mathf.InverseLerp(overloadStartHP, overloadMaxHP, currentHP);
            float pulse = (Mathf.Sin(Time.time * overloadPulseSpeed) + 1f) * 0.5f;

            float scalePulseAmount = Mathf.Lerp(minOverloadScalePulse, maxOverloadScalePulse, overloadT);
            float intensityPulseAmount = Mathf.Lerp(minOverloadIntensityPulse, maxOverloadIntensityPulse, overloadT);

            float scalePulse = Mathf.Lerp(0f, scalePulseAmount, pulse);
            float intensityPulse = Mathf.Lerp(0f, intensityPulseAmount, pulse);

            finalScale = currentScale * (1f + scalePulse);
            finalIntensity = currentIntensity + intensityPulse;

            float colorPulseBlend = Mathf.Lerp(0.15f, 0.65f, overloadT) * pulse;
            finalColor = Color.Lerp(finalColor, overloadColor, colorPulseBlend);
        }

        // DYING EFFECT (<40 HP): shake + flicker + darker color
        if (enableDyingEffect && currentHP <= dyingHPThreshold)
        {
            float dyingT = 1f - Mathf.InverseLerp(0f, dyingHPThreshold, currentHP);

            float shakeAmount = Mathf.Lerp(minDyingShakeAmount, maxDyingShakeAmount, dyingT);
            float flickerAmount = Mathf.Lerp(minDyingFlickerAmount, maxDyingFlickerAmount, dyingT);

            float noiseX = (Mathf.PerlinNoise(Time.time * dyingShakeSpeed, 0f) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(0f, Time.time * dyingShakeSpeed) - 0.5f) * 2f;

            finalLocalPosition += new Vector3(noiseX, noiseY, 0f) * shakeAmount;

            float flicker = (Mathf.Sin(Time.time * dyingShakeSpeed * 1.3f) + 1f) * 0.5f;
            finalIntensity -= flicker * flickerAmount;

            float dyingColorBlend = dyingT * dyingColorBlendStrength;
            finalColor = Color.Lerp(finalColor, dyingColor, dyingColorBlend);

            // небольшое сжатие умирающей души
            float collapse = Mathf.Lerp(1f, 0.92f, dyingT);
            finalScale *= collapse;
        }

        if (fireTransform != null)
        {
            fireTransform.localScale = finalScale;
            fireTransform.localPosition = finalLocalPosition;
        }

        if (fireController != null)
        {
            fireController.SetFireIntensity(Mathf.Max(0f, finalIntensity));
            fireController.SetFireColor(finalColor);
        }
    }

    public void SetHP(int hp)
    {
        currentHP = hp;

        HPVisualStage stage = GetStageForHP(hp);
        if (stage == null)
            return;

        targetScale = baseScale * stage.scaleMultiplier;
        targetIntensity = stage.fireIntensity;
        targetColor = normalColor;
    }

    public void SetHPInstant(int hp)
    {
        currentHP = hp;

        HPVisualStage stage = GetStageForHP(hp);
        if (stage == null)
            return;

        targetScale = baseScale * stage.scaleMultiplier;
        targetIntensity = stage.fireIntensity;
        targetColor = normalColor;

        ApplyImmediateVisual();
    }

    private void ApplyImmediateVisual()
    {
        currentScale = targetScale;
        currentIntensity = targetIntensity;
        currentColor = targetColor;

        if (fireTransform != null)
        {
            fireTransform.localScale = currentScale;
            fireTransform.localPosition = baseLocalPosition;
        }

        if (fireController != null)
        {
            fireController.SetFireIntensity(currentIntensity);
            fireController.SetFireColor(currentColor);
        }
    }

    private HPVisualStage GetStageForHP(int hp)
    {
        if (stages == null || stages.Length == 0)
            return null;

        for (int i = 0; i < stages.Length; i++)
        {
            if (hp >= stages[i].minHP && hp <= stages[i].maxHP)
                return stages[i];
        }

        if (hp < stages[0].minHP)
            return stages[0];

        return stages[stages.Length - 1];
    }
}