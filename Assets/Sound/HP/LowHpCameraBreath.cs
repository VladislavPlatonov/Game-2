using UnityEngine;
using Poker;

public class LowHpCameraBreath : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SoulManager soulManager;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private AudioSource heartbeatSource;

    [Header("HP Thresholds")]
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int stressHpThreshold = 75;
    [SerializeField] private int panicHpThreshold = 40;

    [Header("Breath Motion")]
    [SerializeField] private float calmAmplitude = 0.003f;
    [SerializeField] private float stressAmplitude = 0.010f;
    [SerializeField] private float panicAmplitude = 0.025f;

    [SerializeField] private float calmCycleSpeed = 0.8f;
    [SerializeField] private float stressCycleSpeed = 1.2f;
    [SerializeField] private float panicCycleSpeed = 1.8f;

    [Header("Pulse")]
    [SerializeField] private float pulseAmplitude = 0.004f;
    [SerializeField] private float pulseSpeed = 5.0f;

    [Header("Return")]
    [SerializeField] private float returnSpeed = 8f;

    [Header("Heartbeat")]
    [SerializeField] private float heartbeatMaxVolume = 0.55f;
    [SerializeField] private float heartbeatMinPitch = 0.9f;
    [SerializeField] private float heartbeatMaxPitch = 1.15f;

    private Vector3 baseLocalPos;

    private void Awake()
    {
        if (soulManager == null)
            soulManager = SoulManager.Instance != null ? SoulManager.Instance : FindFirstObjectByType<SoulManager>();

        if (cameraTarget == null && Camera.main != null)
            cameraTarget = Camera.main.transform;

        if (cameraTarget != null)
            baseLocalPos = cameraTarget.localPosition;
    }

    private void Start()
    {
        if (heartbeatSource != null)
        {
            heartbeatSource.volume = 0f;

            if (!heartbeatSource.isPlaying && heartbeatSource.clip != null)
                heartbeatSource.Play();
        }
    }

    private void LateUpdate()
    {
        if (cameraTarget == null || soulManager == null)
            return;

        int hp = Mathf.Clamp(soulManager.GetPlayerSouls(), 0, maxHp);

        float stress01 = GetStress01(hp);

        if (stress01 <= 0.001f)
        {
            cameraTarget.localPosition = Vector3.Lerp(
                cameraTarget.localPosition,
                baseLocalPos,
                Time.deltaTime * returnSpeed
            );

            UpdateHeartbeat(0f);
            return;
        }

        float amplitude = Mathf.Lerp(calmAmplitude, panicAmplitude, stress01);
        float cycleSpeed = Mathf.Lerp(calmCycleSpeed, panicCycleSpeed, stress01);

        // Дыхательный цикл:
        // медленный вдох вверх, резкий выдох вниз
        float breath = AsymmetricBreath(Time.time * cycleSpeed);

        // Лёгкая пульсация на низком HP
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude * stress01;

        Vector3 targetPos = baseLocalPos + new Vector3(0f, breath * amplitude + pulse, 0f);

        cameraTarget.localPosition = Vector3.Lerp(
            cameraTarget.localPosition,
            targetPos,
            Time.deltaTime * 10f
        );

        UpdateHeartbeat(stress01);
    }

    private float GetStress01(int hp)
    {
        if (hp >= stressHpThreshold)
            return 0f;

        // 0 при 75+, 1 при 0
        return 1f - Mathf.InverseLerp(0f, stressHpThreshold, hp);
    }

    /// <summary>
    /// Даёт форму дыхания:
    /// медленно вверх, резко вниз.
    /// Диапазон примерно -1..1
    /// </summary>
    private float AsymmetricBreath(float t)
    {
        float phase = Mathf.Repeat(t, 1f);

        // 0..0.72 = медленный вдох вверх
        if (phase < 0.72f)
        {
            float inhaleT = phase / 0.72f;
            return Mathf.Lerp(-0.2f, 1f, inhaleT);
        }
        // 0.72..1 = быстрый выдох вниз
        else
        {
            float exhaleT = (phase - 0.72f) / 0.28f;
            return Mathf.Lerp(1f, -1f, exhaleT);
        }
    }

    private void UpdateHeartbeat(float stress01)
    {
        if (heartbeatSource == null)
            return;

        float volume = Mathf.Lerp(0f, heartbeatMaxVolume, stress01);
        float pitch = Mathf.Lerp(heartbeatMinPitch, heartbeatMaxPitch, stress01);

        heartbeatSource.volume = volume;
        heartbeatSource.pitch = pitch;
    }
}