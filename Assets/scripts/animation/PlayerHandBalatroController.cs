using System.Collections;
using UnityEngine;
using Poker;

public class PlayerHandBalatroController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SoulManager soulManager;
    [SerializeField] private Transform handVisual;

    [Header("HP Thresholds")]
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int stressedHpThreshold = 75;
    [SerializeField] private int panicHpThreshold = 40;

    [Header("Calm Motion")]
    [SerializeField] private float calmAmplitudeY = 0.01f;
    [SerializeField] private float calmAmplitudeX = 0.004f;
    [SerializeField] private float calmRotZ = 1.2f;
    [SerializeField] private float calmSpeed = 1.1f;

    [Header("Stressed Motion")]
    [SerializeField] private float stressedAmplitudeY = 0.025f;
    [SerializeField] private float stressedAmplitudeX = 0.01f;
    [SerializeField] private float stressedRotZ = 3.5f;
    [SerializeField] private float stressedSpeed = 2.0f;

    [Header("Panic Motion")]
    [SerializeField] private float panicAmplitudeY = 0.06f;
    [SerializeField] private float panicAmplitudeX = 0.025f;
    [SerializeField] private float panicRotZ = 7.5f;
    [SerializeField] private float panicSpeed = 4.0f;

    [Header("Extra Breath")]
    [SerializeField] private float exhaleExtraY = 0.02f;
    [SerializeField] private float exhaleSpeedMultiplier = 2.8f;

    [Header("Deal / Grab Animation")]
    [SerializeField] private Vector3 hiddenLocalOffset = new Vector3(0f, -0.7f, 0f);
    [SerializeField] private Vector3 dealPushOffset = new Vector3(0.02f, 0.05f, 0f);
    [SerializeField] private float appearDuration = 0.18f;
    [SerializeField] private float grabForwardDuration = 0.08f;
    [SerializeField] private float grabHoldDuration = 0.05f;
    [SerializeField] private float grabBackDuration = 0.12f;
    [SerializeField] private float grabTiltAngle = -8f;
    [SerializeField] private float grabScaleMultiplier = 1.04f;

    private Vector3 baseLocalPos;
    private Quaternion baseLocalRot;
    private Vector3 baseLocalScale;

    private Coroutine grabRoutine;
    private bool isPlayingGrab;

    private void Awake()
    {
        if (soulManager == null)
            soulManager = SoulManager.Instance != null ? SoulManager.Instance : FindFirstObjectByType<SoulManager>();

        if (handVisual == null)
            handVisual = transform;

        baseLocalPos = transform.localPosition;
        baseLocalRot = transform.localRotation;
        baseLocalScale = transform.localScale;
    }

    private void Update()
    {
        if (isPlayingGrab) return;
        ApplyIdleBreath();
    }

    private void ApplyIdleBreath()
    {
        if (soulManager == null) return;

        int hp = Mathf.Clamp(soulManager.GetPlayerSouls(), 0, maxHp);

        float ampY;
        float ampX;
        float rotZ;
        float speed;

        if (hp > stressedHpThreshold)
        {
            float t = Mathf.InverseLerp(stressedHpThreshold, maxHp, hp);
            ampY = Mathf.Lerp(stressedAmplitudeY, calmAmplitudeY, t);
            ampX = Mathf.Lerp(stressedAmplitudeX, calmAmplitudeX, t);
            rotZ = Mathf.Lerp(stressedRotZ, calmRotZ, t);
            speed = Mathf.Lerp(stressedSpeed, calmSpeed, t);
        }
        else if (hp > panicHpThreshold)
        {
            float t = Mathf.InverseLerp(panicHpThreshold, stressedHpThreshold, hp);
            ampY = Mathf.Lerp(panicAmplitudeY, stressedAmplitudeY, t);
            ampX = Mathf.Lerp(panicAmplitudeX, stressedAmplitudeX, t);
            rotZ = Mathf.Lerp(panicRotZ, stressedRotZ, t);
            speed = Mathf.Lerp(panicSpeed, stressedSpeed, t);
        }
        else
        {
            ampY = panicAmplitudeY;
            ampX = panicAmplitudeX;
            rotZ = panicRotZ;
            speed = panicSpeed;
        }

        float breatheY = Mathf.Sin(Time.time * speed) * ampY;
        float swayX = Mathf.Sin(Time.time * speed * 0.65f) * ampX;
        float sharpExhale = Mathf.Sin(Time.time * speed * exhaleSpeedMultiplier) *
                            exhaleExtraY *
                            (1f - Mathf.InverseLerp(panicHpThreshold, maxHp, hp));

        transform.localPosition = baseLocalPos + new Vector3(swayX, breatheY + sharpExhale, 0f);

        float z = Mathf.Sin(Time.time * speed * 0.8f) * rotZ;
        transform.localRotation = baseLocalRot * Quaternion.Euler(0f, 0f, z);
    }

    public void PlayDealGrab()
    {
        if (grabRoutine != null)
            StopCoroutine(grabRoutine);

        grabRoutine = StartCoroutine(DealGrabRoutine());
    }

    public void SnapToIdle()
    {
        if (grabRoutine != null)
            StopCoroutine(grabRoutine);

        isPlayingGrab = false;
        transform.localPosition = baseLocalPos;
        transform.localRotation = baseLocalRot;
        transform.localScale = baseLocalScale;
    }

    private IEnumerator DealGrabRoutine()
    {
        isPlayingGrab = true;

        Vector3 idlePos = baseLocalPos;
        Quaternion idleRot = baseLocalRot;
        Vector3 idleScale = baseLocalScale;

        Vector3 hiddenPos = idlePos + hiddenLocalOffset;
        Vector3 pushedPos = idlePos + dealPushOffset;

        Quaternion pushedRot = idleRot * Quaternion.Euler(0f, 0f, grabTiltAngle);
        Vector3 pushedScale = idleScale * grabScaleMultiplier;

        // 1. Появление снизу
        transform.localPosition = hiddenPos;
        transform.localRotation = idleRot;
        transform.localScale = idleScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / appearDuration;
            transform.localPosition = Vector3.Lerp(hiddenPos, idlePos, t);
            yield return null;
        }

        transform.localPosition = idlePos;

        // 2. Быстрый хват вперёд
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / grabForwardDuration;
            transform.localPosition = Vector3.Lerp(idlePos, pushedPos, t);
            transform.localRotation = Quaternion.Lerp(idleRot, pushedRot, t);
            transform.localScale = Vector3.Lerp(idleScale, pushedScale, t);
            yield return null;
        }

        transform.localPosition = pushedPos;
        transform.localRotation = pushedRot;
        transform.localScale = pushedScale;

        // 3. Пауза
        yield return new WaitForSeconds(grabHoldDuration);

        // 4. Возврат обратно
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / grabBackDuration;
            transform.localPosition = Vector3.Lerp(pushedPos, idlePos, t);
            transform.localRotation = Quaternion.Lerp(pushedRot, idleRot, t);
            transform.localScale = Vector3.Lerp(pushedScale, idleScale, t);
            yield return null;
        }

        transform.localPosition = idlePos;
        transform.localRotation = idleRot;
        transform.localScale = idleScale;

        isPlayingGrab = false;
        grabRoutine = null;
    }
}