using System.Collections;
using UnityEngine;
using Poker;

public class AIHandEmotionController : MonoBehaviour
{
    public enum AIHandState
    {
        Calm,
        Stressed,
        Panic
    }

    [Header("Refs")]
    [SerializeField] private SoulManager soulManager;
    [SerializeField] private Transform handVisual;

    [Header("Auto state from AI HP")]
    [SerializeField] private bool useAutoStateFromAIHp = true;
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int stressedHpThreshold = 75;
    [SerializeField] private int panicHpThreshold = 40;

    [Header("Manual / future AI emotion input")]
    [SerializeField] private AIHandState currentState = AIHandState.Calm;

    [Header("Calm Tremor")]
    [SerializeField] private float calmAmplitudeY = 0.0015f;
    [SerializeField] private float calmAmplitudeX = 0.0004f;
    [SerializeField] private float calmAmplitudeZ = 0.0002f;
    [SerializeField] private float calmRotZ = 0.08f;
    [SerializeField] private float calmSpeed = 18f;

    [Header("Stressed Tremor")]
    [SerializeField] private float stressedAmplitudeY = 0.003f;
    [SerializeField] private float stressedAmplitudeX = 0.0007f;
    [SerializeField] private float stressedAmplitudeZ = 0.0003f;
    [SerializeField] private float stressedRotZ = 0.18f;
    [SerializeField] private float stressedSpeed = 28f;

    [Header("Panic Tremor")]
    [SerializeField] private float panicAmplitudeY = 0.006f;
    [SerializeField] private float panicAmplitudeX = 0.0012f;
    [SerializeField] private float panicAmplitudeZ = 0.0005f;
    [SerializeField] private float panicRotZ = 0.35f;
    [SerializeField] private float panicSpeed = 42f;

    [Header("Deal / Grab Animation")]
    [SerializeField] private Vector3 hiddenLocalOffset = new Vector3(0f, -0.7f, 0f);
    [SerializeField] private Vector3 dealPushOffset = new Vector3(0.02f, -0.03f, 0.01f);
    [SerializeField] private float appearDuration = 0.18f;
    [SerializeField] private float grabForwardDuration = 0.08f;
    [SerializeField] private float grabHoldDuration = 0.05f;
    [SerializeField] private float grabBackDuration = 0.12f;
    [SerializeField] private float grabTiltAngle = 4f;
    [SerializeField] private float grabScaleMultiplier = 1.03f;

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

    private void LateUpdate()
    {
        if (isPlayingGrab) return;
        ApplyIdleTremor();
    }

    private void ApplyIdleTremor()
    {
        AIHandState stateToUse = currentState;

        if (useAutoStateFromAIHp && soulManager != null)
        {
            int hp = Mathf.Clamp(soulManager.GetAISouls(), 0, maxHp);

            if (hp > stressedHpThreshold)
                stateToUse = AIHandState.Calm;
            else if (hp > panicHpThreshold)
                stateToUse = AIHandState.Stressed;
            else
                stateToUse = AIHandState.Panic;
        }

        float ampY;
        float ampX;
        float ampZ;
        float rotZ;
        float speed;

        switch (stateToUse)
        {
            case AIHandState.Stressed:
                ampY = stressedAmplitudeY;
                ampX = stressedAmplitudeX;
                ampZ = stressedAmplitudeZ;
                rotZ = stressedRotZ;
                speed = stressedSpeed;
                break;

            case AIHandState.Panic:
                ampY = panicAmplitudeY;
                ampX = panicAmplitudeX;
                ampZ = panicAmplitudeZ;
                rotZ = panicRotZ;
                speed = panicSpeed;
                break;

            default:
                ampY = calmAmplitudeY;
                ampX = calmAmplitudeX;
                ampZ = calmAmplitudeZ;
                rotZ = calmRotZ;
                speed = calmSpeed;
                break;
        }

        float t = Time.time * speed;

        // Главное — вертикальная дрожь
        float shakeY =
            (Mathf.PerlinNoise(t, 0.21f) - 0.5f) * 2f * ampY +
            Mathf.Sin(t * 1.7f) * ampY * 0.25f;

        // Очень слабое горизонтальное движение
        float shakeX =
            (Mathf.PerlinNoise(0.37f, t) - 0.5f) * 2f * ampX;

        // Совсем легкое смещение по Z
        float shakeZ =
            (Mathf.PerlinNoise(t, 0.73f) - 0.5f) * 2f * ampZ;

        float z =
            (Mathf.PerlinNoise(t, 0.91f) - 0.5f) * 2f * rotZ;

        transform.localPosition = baseLocalPos + new Vector3(shakeX, shakeY, shakeZ);
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

        // 1. Появление / подача руки
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

        // 2. Подхват карт
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

        // 3. Короткая пауза
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

    public void SetStateCalm()
    {
        currentState = AIHandState.Calm;
    }

    public void SetStateStressed()
    {
        currentState = AIHandState.Stressed;
    }

    public void SetStatePanic()
    {
        currentState = AIHandState.Panic;
    }

    public void SetState(AIHandState newState)
    {
        currentState = newState;
    }

    public void SetExternalStress01(float stress01)
    {
        stress01 = Mathf.Clamp01(stress01);

        if (stress01 < 0.33f)
            currentState = AIHandState.Calm;
        else if (stress01 < 0.66f)
            currentState = AIHandState.Stressed;
        else
            currentState = AIHandState.Panic;
    }
}