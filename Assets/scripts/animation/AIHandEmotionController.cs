using System.Collections;
using UnityEngine;

public class AIHandEmotionController : MonoBehaviour
{
    public enum AIHandState
    {
        Calm,
        Nervous,
        Panic,
        Focus,
        Suspicious,
        Greedy,
        Aggressive,
        BluffCalm,
        BluffNervous,
        Unhinged
    }

    [Header("Refs")]
    [SerializeField] private EnemyHeadCalmController headController;
    [SerializeField] private Transform handVisual;
    [SerializeField] private Transform unhingedKnifeTarget;

    [Header("State Control")]
    [SerializeField] private AIHandState currentState = AIHandState.Calm;
    [SerializeField] private bool syncStateFromHead = false;

    [Header("Calm Motion")]
    [SerializeField] private float calmFollowAmountY = 0.018f;
    [SerializeField] private float calmFollowAmountX = 0.0025f;
    [SerializeField] private float calmFollowSpeed = 0.55f;
    [SerializeField] private float calmRotZ = 0.6f;
    [SerializeField] private float calmRotSpeed = 0.45f;
    [SerializeField] private float calmLerpSpeed = 5f;
    [SerializeField] private Vector3 calmRotation = Vector3.zero;

    [Header("Nervous Motion")]
    [SerializeField] private float nervousBaseOffsetX = 0.010f;
    [SerializeField] private float nervousBaseOffsetY = -0.003f;
    [SerializeField] private float nervousBaseOffsetZ = 0.008f;
    [SerializeField] private float nervousShakeSpeed = 9f;
    [SerializeField] private float nervousShakeAmountX = 0.004f;
    [SerializeField] private float nervousShakeAmountY = 0.006f;
    [SerializeField] private float nervousShakeAmountZ = 0.0025f;
    [SerializeField] private float nervousNoiseSpeed = 4.8f;
    [SerializeField] private float nervousNoiseAmountX = 0.003f;
    [SerializeField] private float nervousNoiseAmountY = 0.004f;
    [SerializeField] private float nervousNoiseAmountZ = 0.002f;
    [SerializeField] private Vector3 nervousRotation = new Vector3(0f, 0f, -2.5f);
    [SerializeField] private float nervousLerpSpeed = 10f;

    [Header("Panic Motion")]
    [SerializeField] private float panicNoiseSpeed = 26f;
    [SerializeField] private float panicNoiseAmountX = 0.010f;
    [SerializeField] private float panicNoiseAmountY = 0.014f;
    [SerializeField] private float panicLerpSpeed = 16f;
    [SerializeField] private Vector3 panicRotation = Vector3.zero;

    [Header("Focus Pose")]
    [SerializeField] private float focusOffsetX = 0.05f;
    [SerializeField] private float focusOffsetY = 0.025f;
    [SerializeField] private float focusOffsetZ = 0f;
    [SerializeField] private Vector3 focusRotation = new Vector3(0f, 0f, -9f);
    [SerializeField] private float focusLerpSpeed = 7f;

    [Header("Suspicious Motion")]
    [SerializeField] private float suspiciousBaseOffsetX = 0.015f;
    [SerializeField] private float suspiciousBaseOffsetY = 0.008f;
    [SerializeField] private float suspiciousBaseOffsetZ = 0.012f;
    [SerializeField] private float suspiciousMoveAmountX = 0.014f;
    [SerializeField] private float suspiciousMoveAmountY = 0.010f;
    [SerializeField] private float suspiciousMoveAmountZ = 0.008f;
    [SerializeField] private float suspiciousMoveSpeed = 0.95f;
    [SerializeField] private float suspiciousNoiseSpeed = 1.6f;
    [SerializeField] private float suspiciousNoiseAmountX = 0.007f;
    [SerializeField] private float suspiciousNoiseAmountY = 0.005f;
    [SerializeField] private float suspiciousNoiseAmountZ = 0.004f;
    [SerializeField] private Vector3 suspiciousRotation = new Vector3(0f, 0f, -4f);
    [SerializeField] private float suspiciousLerpSpeed = 5.5f;

    [Header("Greedy Motion")]
    [SerializeField] private float greedyOffsetX = 0.008f;
    [SerializeField] private float greedyOffsetY = 0.004f;
    [SerializeField] private float greedyOffsetZ = 0.014f;
    [SerializeField] private Vector3 greedyRotation = new Vector3(0f, 0f, -1.5f);
    [SerializeField] private float greedyLerpSpeed = 6f;

    [Header("Aggressive Motion")]
    [SerializeField] private float aggressiveBaseOffsetX = 0.018f;
    [SerializeField] private float aggressiveBaseOffsetY = -0.010f;
    [SerializeField] private float aggressiveBaseOffsetZ = 0.020f;
    [SerializeField] private float aggressiveMoveAmountX = 0.020f;
    [SerializeField] private float aggressiveMoveAmountY = 0.018f;
    [SerializeField] private float aggressiveMoveAmountZ = 0.016f;
    [SerializeField] private float aggressiveMoveSpeed = 4.2f;
    [SerializeField] private float aggressiveNoiseSpeed = 2.8f;
    [SerializeField] private float aggressiveNoiseAmountX = 0.006f;
    [SerializeField] private float aggressiveNoiseAmountY = 0.008f;
    [SerializeField] private float aggressiveNoiseAmountZ = 0.004f;
    [SerializeField] private Vector3 aggressiveRotation = new Vector3(0f, 0f, -10f);
    [SerializeField] private float aggressiveLerpSpeed = 11f;

    [Header("Bluff Calm Motion")]
    [SerializeField] private float bluffCalmBaseOffsetX = 0.010f;
    [SerializeField] private float bluffCalmBaseOffsetY = -0.002f;
    [SerializeField] private float bluffCalmBaseOffsetZ = 0.010f;
    [SerializeField] private float bluffCalmNoiseSpeed = 1.8f;
    [SerializeField] private float bluffCalmNoiseAmountX = 0.0025f;
    [SerializeField] private float bluffCalmNoiseAmountY = 0.0035f;
    [SerializeField] private float bluffCalmNoiseAmountZ = 0.002f;
    [SerializeField] private float bluffCalmMicroWaveSpeed = 1.25f;
    [SerializeField] private float bluffCalmMicroWaveAmountX = 0.0015f;
    [SerializeField] private float bluffCalmMicroWaveAmountY = 0.002f;
    [SerializeField] private Vector3 bluffCalmRotation = new Vector3(0f, 0f, -2.5f);
    [SerializeField] private float bluffCalmLerpSpeed = 8f;

    [Header("Bluff Nervous Motion")]
    [SerializeField] private float bluffNervousBaseOffsetX = 0.014f;
    [SerializeField] private float bluffNervousBaseOffsetY = -0.004f;
    [SerializeField] private float bluffNervousBaseOffsetZ = 0.012f;
    [SerializeField] private float bluffNervousShakeSpeed = 12f;
    [SerializeField] private float bluffNervousShakeAmountX = 0.0045f;
    [SerializeField] private float bluffNervousShakeAmountY = 0.006f;
    [SerializeField] private float bluffNervousShakeAmountZ = 0.0035f;
    [SerializeField] private float bluffNervousNoiseSpeed = 6f;
    [SerializeField] private float bluffNervousNoiseAmountX = 0.004f;
    [SerializeField] private float bluffNervousNoiseAmountY = 0.005f;
    [SerializeField] private float bluffNervousNoiseAmountZ = 0.003f;
    [SerializeField] private Vector3 bluffNervousRotation = new Vector3(0f, 0f, -3.5f);
    [SerializeField] private float bluffNervousLerpSpeed = 13f;

    [Header("Unhinged Motion")]
    [SerializeField] private float unhingedBaseOffsetX = 0.012f;
    [SerializeField] private float unhingedBaseOffsetY = -0.006f;
    [SerializeField] private float unhingedBaseOffsetZ = 0.013f;
    [SerializeField] private float unhingedNoiseSpeed = 3.8f;
    [SerializeField] private float unhingedNoiseAmountX = 0.004f;
    [SerializeField] private float unhingedNoiseAmountY = 0.0045f;
    [SerializeField] private float unhingedNoiseAmountZ = 0.003f;
    [SerializeField] private float unhingedWaveSpeed = 2.1f;
    [SerializeField] private float unhingedWaveAmountX = 0.003f;
    [SerializeField] private float unhingedWaveAmountY = 0.002f;
    [SerializeField] private Vector3 unhingedRotation = new Vector3(0f, 0f, -5.5f);
    [SerializeField] private float unhingedLerpSpeed = 10f;
    [SerializeField] private float unhingedKnifeReachAmount = 0.032f;
    [SerializeField] private float unhingedKnifeReachRotZ = -7f;

    [Header("Table Slam")]
    [SerializeField] private Vector3 slamOffset = new Vector3(0.05f, -0.06f, 0.03f);
    [SerializeField] private Vector3 slamRotation = new Vector3(0f, 0f, -18f);
    [SerializeField] private float slamForwardDuration = 0.07f;
    [SerializeField] private float slamBackDuration = 0.14f;
    [SerializeField] private float slamHoldDuration = 0.03f;
    [SerializeField] private float slamScaleMultiplier = 1.05f;

    [Header("Deal / Grab Animation")]
    [SerializeField] private Vector3 hiddenLocalOffset = new Vector3(0f, -0.7f, 0f);
    [SerializeField] private Vector3 dealPushOffset = new Vector3(0.02f, -0.03f, 0.01f);
    [SerializeField] private float appearDuration = 0.18f;
    [SerializeField] private float grabForwardDuration = 0.08f;
    [SerializeField] private float grabHoldDuration = 0.05f;
    [SerializeField] private float grabBackDuration = 0.12f;
    [SerializeField] private float grabTiltAngle = 4f;
    [SerializeField] private float grabScaleMultiplier = 1.03f;

    private Transform animatedTarget;

    private Vector3 baseLocalPos;
    private Quaternion baseLocalRot;
    private Vector3 baseLocalScale;

    private Coroutine grabRoutine;
    private Coroutine slamRoutine;
    private bool isPlayingGrab;
    private bool isPlayingSlam;

    private void Awake()
    {
        if (headController == null)
            headController = FindFirstObjectByType<EnemyHeadCalmController>();

        animatedTarget = handVisual != null ? handVisual : transform;

        baseLocalPos = animatedTarget.localPosition;
        baseLocalRot = animatedTarget.localRotation;
        baseLocalScale = animatedTarget.localScale;
    }

    private void Update()
    {
        SyncStateFromHeadIfNeeded();

        if (isPlayingGrab || isPlayingSlam)
            return;

        ApplyCurrentStatePose();
    }

    private void SyncStateFromHeadIfNeeded()
    {
        if (!syncStateFromHead || headController == null)
            return;

        switch (headController.GetCurrentEmotion())
        {
            case EnemyHeadCalmController.EmotionState.Calm:
                currentState = AIHandState.Calm;
                break;
            case EnemyHeadCalmController.EmotionState.Nervous:
                currentState = AIHandState.Nervous;
                break;
            case EnemyHeadCalmController.EmotionState.Panic:
                currentState = AIHandState.Panic;
                break;
            case EnemyHeadCalmController.EmotionState.Focus:
                currentState = AIHandState.Focus;
                break;
            case EnemyHeadCalmController.EmotionState.Suspicious:
                currentState = AIHandState.Suspicious;
                break;
            case EnemyHeadCalmController.EmotionState.Greedy:
                currentState = AIHandState.Greedy;
                break;
            case EnemyHeadCalmController.EmotionState.Aggressive:
                currentState = AIHandState.Aggressive;
                break;
            case EnemyHeadCalmController.EmotionState.BluffCalm:
                currentState = AIHandState.BluffCalm;
                break;
            case EnemyHeadCalmController.EmotionState.BluffNervous:
                currentState = AIHandState.BluffNervous;
                break;
            case EnemyHeadCalmController.EmotionState.Unhinged:
                currentState = AIHandState.Unhinged;
                break;
        }
    }

    private void ApplyCurrentStatePose()
    {
        switch (currentState)
        {
            case AIHandState.Calm:
                ApplyCalmPose();
                break;
            case AIHandState.Nervous:
                ApplyNervousPose();
                break;
            case AIHandState.Panic:
                ApplyPanicPose();
                break;
            case AIHandState.Focus:
                ApplyFocusPose();
                break;
            case AIHandState.Suspicious:
                ApplySuspiciousPose();
                break;
            case AIHandState.Greedy:
                ApplyGreedyPose();
                break;
            case AIHandState.Aggressive:
                ApplyAggressivePose();
                break;
            case AIHandState.BluffCalm:
                ApplyBluffCalmPose();
                break;
            case AIHandState.BluffNervous:
                ApplyBluffNervousPose();
                break;
            case AIHandState.Unhinged:
                ApplyUnhingedPose();
                break;
        }
    }

    private void ApplyCalmPose()
    {
        float t = Time.time;

        float moveY = Mathf.Sin(t * calmFollowSpeed) * calmFollowAmountY;
        float moveX = Mathf.Sin(t * calmFollowSpeed * 0.7f) * calmFollowAmountX;
        float rotZ = Mathf.Sin(t * calmRotSpeed) * calmRotZ;

        Vector3 targetPos = baseLocalPos + new Vector3(moveX, moveY, 0f);
        Quaternion targetRot = baseLocalRot * Quaternion.Euler(calmRotation + new Vector3(0f, 0f, rotZ));

        animatedTarget.localPosition = Vector3.Lerp(animatedTarget.localPosition, targetPos, Time.deltaTime * calmLerpSpeed);
        animatedTarget.localRotation = Quaternion.Slerp(animatedTarget.localRotation, targetRot, Time.deltaTime * calmLerpSpeed);
        animatedTarget.localScale = Vector3.Lerp(animatedTarget.localScale, baseLocalScale, Time.deltaTime * calmLerpSpeed);
    }

    private void ApplyNervousPose()
    {
        float t = Time.time;

        float shakeX = Mathf.Sin(t * nervousShakeSpeed) * nervousShakeAmountX;
        float shakeY = Mathf.Sin(t * nervousShakeSpeed * 1.27f) * nervousShakeAmountY;
        float shakeZ = Mathf.Sin(t * nervousShakeSpeed * 0.89f) * nervousShakeAmountZ;

        float noiseX =
            (Mathf.PerlinNoise(t * nervousNoiseSpeed, 0.26f) - 0.5f) * 2f * nervousNoiseAmountX;

        float noiseY =
            (Mathf.PerlinNoise(0.68f, t * nervousNoiseSpeed) - 0.5f) * 2f * nervousNoiseAmountY;

        float noiseZ =
            (Mathf.PerlinNoise(t * nervousNoiseSpeed, 0.91f) - 0.5f) * 2f * nervousNoiseAmountZ;

        Vector3 targetPos = baseLocalPos + new Vector3(
            nervousBaseOffsetX + shakeX + noiseX,
            nervousBaseOffsetY + shakeY + noiseY,
            nervousBaseOffsetZ + shakeZ + noiseZ
        );

        Quaternion targetRot = baseLocalRot * Quaternion.Euler(nervousRotation);

        animatedTarget.localPosition = Vector3.Lerp(animatedTarget.localPosition, targetPos, Time.deltaTime * nervousLerpSpeed);
        animatedTarget.localRotation = Quaternion.Slerp(animatedTarget.localRotation, targetRot, Time.deltaTime * nervousLerpSpeed);
        animatedTarget.localScale = Vector3.Lerp(animatedTarget.localScale, baseLocalScale, Time.deltaTime * nervousLerpSpeed);
    }

    private void ApplyPanicPose()
    {
        float t = Time.time;

        float vibeX =
            (Mathf.PerlinNoise(t * panicNoiseSpeed, 0.17f) - 0.5f) * 2f * panicNoiseAmountX;

        float vibeY =
            (Mathf.PerlinNoise(0.41f, t * panicNoiseSpeed * 1.11f) - 0.5f) * 2f * panicNoiseAmountY;

        Vector3 targetPos = baseLocalPos + new Vector3(vibeX, vibeY, 0f);
        Quaternion targetRot = baseLocalRot * Quaternion.Euler(panicRotation);

        animatedTarget.localPosition = Vector3.Lerp(animatedTarget.localPosition, targetPos, Time.deltaTime * panicLerpSpeed);
        animatedTarget.localRotation = Quaternion.Slerp(animatedTarget.localRotation, targetRot, Time.deltaTime * panicLerpSpeed);
        animatedTarget.localScale = Vector3.Lerp(animatedTarget.localScale, baseLocalScale, Time.deltaTime * panicLerpSpeed);
    }

    private void ApplyFocusPose()
    {
        Vector3 targetPos = baseLocalPos + new Vector3(focusOffsetX, focusOffsetY, focusOffsetZ);
        Quaternion targetRot = baseLocalRot * Quaternion.Euler(focusRotation);

        animatedTarget.localPosition = Vector3.Lerp(animatedTarget.localPosition, targetPos, Time.deltaTime * focusLerpSpeed);
        animatedTarget.localRotation = Quaternion.Slerp(animatedTarget.localRotation, targetRot, Time.deltaTime * focusLerpSpeed);
        animatedTarget.localScale = Vector3.Lerp(animatedTarget.localScale, baseLocalScale, Time.deltaTime * focusLerpSpeed);
    }

    private void ApplySuspiciousPose()
    {
        float t = Time.time;

        float waveX = Mathf.Sin(t * suspiciousMoveSpeed) * suspiciousMoveAmountX;
        float waveY = Mathf.Sin(t * suspiciousMoveSpeed * 0.83f) * suspiciousMoveAmountY;
        float waveZ = Mathf.Sin(t * suspiciousMoveSpeed * 1.19f) * suspiciousMoveAmountZ;

        float noiseX =
            (Mathf.PerlinNoise(t * suspiciousNoiseSpeed, 0.23f) - 0.5f) * 2f * suspiciousNoiseAmountX;

        float noiseY =
            (Mathf.PerlinNoise(0.61f, t * suspiciousNoiseSpeed) - 0.5f) * 2f * suspiciousNoiseAmountY;

        float noiseZ =
            (Mathf.PerlinNoise(t * suspiciousNoiseSpeed, 0.87f) - 0.5f) * 2f * suspiciousNoiseAmountZ;

        Vector3 targetPos = baseLocalPos + new Vector3(
            suspiciousBaseOffsetX + waveX + noiseX,
            suspiciousBaseOffsetY + waveY + noiseY,
            suspiciousBaseOffsetZ + waveZ + noiseZ
        );

        Quaternion targetRot = baseLocalRot * Quaternion.Euler(suspiciousRotation);

        animatedTarget.localPosition = Vector3.Lerp(animatedTarget.localPosition, targetPos, Time.deltaTime * suspiciousLerpSpeed);
        animatedTarget.localRotation = Quaternion.Slerp(animatedTarget.localRotation, targetRot, Time.deltaTime * suspiciousLerpSpeed);
        animatedTarget.localScale = Vector3.Lerp(animatedTarget.localScale, baseLocalScale, Time.deltaTime * suspiciousLerpSpeed);
    }

    private void ApplyGreedyPose()
    {
        Vector3 targetPos = baseLocalPos + new Vector3(greedyOffsetX, greedyOffsetY, greedyOffsetZ);
        Quaternion targetRot = baseLocalRot * Quaternion.Euler(greedyRotation);

        animatedTarget.localPosition = Vector3.Lerp(animatedTarget.localPosition, targetPos, Time.deltaTime * greedyLerpSpeed);
        animatedTarget.localRotation = Quaternion.Slerp(animatedTarget.localRotation, targetRot, Time.deltaTime * greedyLerpSpeed);
        animatedTarget.localScale = Vector3.Lerp(animatedTarget.localScale, baseLocalScale, Time.deltaTime * greedyLerpSpeed);
    }

    private void ApplyAggressivePose()
    {
        float t = Time.time;

        float waveX = Mathf.Sin(t * aggressiveMoveSpeed) * aggressiveMoveAmountX;
        float waveY = Mathf.Sin(t * aggressiveMoveSpeed * 1.21f) * aggressiveMoveAmountY;
        float waveZ = Mathf.Sin(t * aggressiveMoveSpeed * 0.93f) * aggressiveMoveAmountZ;

        float noiseX =
            (Mathf.PerlinNoise(t * aggressiveNoiseSpeed, 0.31f) - 0.5f) * 2f * aggressiveNoiseAmountX;

        float noiseY =
            (Mathf.PerlinNoise(0.57f, t * aggressiveNoiseSpeed) - 0.5f) * 2f * aggressiveNoiseAmountY;

        float noiseZ =
            (Mathf.PerlinNoise(t * aggressiveNoiseSpeed, 0.91f) - 0.5f) * 2f * aggressiveNoiseAmountZ;

        Vector3 targetPos = baseLocalPos + new Vector3(
            aggressiveBaseOffsetX + waveX + noiseX,
            aggressiveBaseOffsetY + waveY + noiseY,
            aggressiveBaseOffsetZ + waveZ + noiseZ
        );

        Quaternion targetRot = baseLocalRot * Quaternion.Euler(aggressiveRotation);

        animatedTarget.localPosition = Vector3.Lerp(animatedTarget.localPosition, targetPos, Time.deltaTime * aggressiveLerpSpeed);
        animatedTarget.localRotation = Quaternion.Slerp(animatedTarget.localRotation, targetRot, Time.deltaTime * aggressiveLerpSpeed);
        animatedTarget.localScale = Vector3.Lerp(animatedTarget.localScale, baseLocalScale, Time.deltaTime * aggressiveLerpSpeed);
    }

    private void ApplyBluffCalmPose()
    {
        float t = Time.time;

        float waveX = Mathf.Sin(t * bluffCalmMicroWaveSpeed) * bluffCalmMicroWaveAmountX;
        float waveY = Mathf.Sin(t * bluffCalmMicroWaveSpeed * 1.37f) * bluffCalmMicroWaveAmountY;

        float noiseX =
            (Mathf.PerlinNoise(t * bluffCalmNoiseSpeed, 0.19f) - 0.5f) * 2f * bluffCalmNoiseAmountX;

        float noiseY =
            (Mathf.PerlinNoise(0.73f, t * bluffCalmNoiseSpeed) - 0.5f) * 2f * bluffCalmNoiseAmountY;

        float noiseZ =
            (Mathf.PerlinNoise(t * bluffCalmNoiseSpeed, 0.49f) - 0.5f) * 2f * bluffCalmNoiseAmountZ;

        Vector3 targetPos = baseLocalPos + new Vector3(
            bluffCalmBaseOffsetX + waveX + noiseX,
            bluffCalmBaseOffsetY + waveY + noiseY,
            bluffCalmBaseOffsetZ + noiseZ
        );

        Quaternion targetRot = baseLocalRot * Quaternion.Euler(bluffCalmRotation);

        animatedTarget.localPosition = Vector3.Lerp(animatedTarget.localPosition, targetPos, Time.deltaTime * bluffCalmLerpSpeed);
        animatedTarget.localRotation = Quaternion.Slerp(animatedTarget.localRotation, targetRot, Time.deltaTime * bluffCalmLerpSpeed);
        animatedTarget.localScale = Vector3.Lerp(animatedTarget.localScale, baseLocalScale, Time.deltaTime * bluffCalmLerpSpeed);
    }

    private void ApplyBluffNervousPose()
    {
        float t = Time.time;

        float shakeX = Mathf.Sin(t * bluffNervousShakeSpeed) * bluffNervousShakeAmountX;
        float shakeY = Mathf.Sin(t * bluffNervousShakeSpeed * 1.31f) * bluffNervousShakeAmountY;
        float shakeZ = Mathf.Sin(t * bluffNervousShakeSpeed * 0.91f) * bluffNervousShakeAmountZ;

        float noiseX =
            (Mathf.PerlinNoise(t * bluffNervousNoiseSpeed, 0.28f) - 0.5f) * 2f * bluffNervousNoiseAmountX;

        float noiseY =
            (Mathf.PerlinNoise(0.67f, t * bluffNervousNoiseSpeed) - 0.5f) * 2f * bluffNervousNoiseAmountY;

        float noiseZ =
            (Mathf.PerlinNoise(t * bluffNervousNoiseSpeed, 0.93f) - 0.5f) * 2f * bluffNervousNoiseAmountZ;

        Vector3 targetPos = baseLocalPos + new Vector3(
            bluffNervousBaseOffsetX + shakeX + noiseX,
            bluffNervousBaseOffsetY + shakeY + noiseY,
            bluffNervousBaseOffsetZ + shakeZ + noiseZ
        );

        Quaternion targetRot = baseLocalRot * Quaternion.Euler(bluffNervousRotation);

        animatedTarget.localPosition = Vector3.Lerp(animatedTarget.localPosition, targetPos, Time.deltaTime * bluffNervousLerpSpeed);
        animatedTarget.localRotation = Quaternion.Slerp(animatedTarget.localRotation, targetRot, Time.deltaTime * bluffNervousLerpSpeed);
        animatedTarget.localScale = Vector3.Lerp(animatedTarget.localScale, baseLocalScale, Time.deltaTime * bluffNervousLerpSpeed);
    }

    private void ApplyUnhingedPose()
    {
        float t = Time.time;

        float waveX = Mathf.Sin(t * unhingedWaveSpeed) * unhingedWaveAmountX;
        float waveY = Mathf.Sin(t * unhingedWaveSpeed * 1.41f) * unhingedWaveAmountY;

        float noiseX =
            (Mathf.PerlinNoise(t * unhingedNoiseSpeed, 0.18f) - 0.5f) * 2f * unhingedNoiseAmountX;

        float noiseY =
            (Mathf.PerlinNoise(0.66f, t * unhingedNoiseSpeed) - 0.5f) * 2f * unhingedNoiseAmountY;

        float noiseZ =
            (Mathf.PerlinNoise(t * unhingedNoiseSpeed, 0.92f) - 0.5f) * 2f * unhingedNoiseAmountZ;

        Vector3 targetPos = baseLocalPos + new Vector3(
            unhingedBaseOffsetX + waveX + noiseX,
            unhingedBaseOffsetY + waveY + noiseY,
            unhingedBaseOffsetZ + noiseZ
        );

        Quaternion targetRot = baseLocalRot * Quaternion.Euler(unhingedRotation);

        float knifeGlanceWeight = headController != null ? headController.GetUnhingedKnifeGlanceWeight() : 0f;
        Transform knifeTarget = GetKnifeTarget();

        if (knifeTarget != null && knifeGlanceWeight > 0.0001f)
        {
            Vector3 toKnifeWorld = knifeTarget.position - animatedTarget.position;
            Vector3 toKnifeLocal = animatedTarget.parent != null
                ? animatedTarget.parent.InverseTransformDirection(toKnifeWorld.normalized)
                : transform.InverseTransformDirection(toKnifeWorld.normalized);

            Vector3 reachOffset = new Vector3(toKnifeLocal.x, toKnifeLocal.y, 0f) * unhingedKnifeReachAmount;
            Quaternion reachRot = baseLocalRot * Quaternion.Euler(unhingedRotation + new Vector3(0f, 0f, unhingedKnifeReachRotZ));

            targetPos = Vector3.Lerp(targetPos, targetPos + reachOffset, knifeGlanceWeight);
            targetRot = Quaternion.Slerp(targetRot, reachRot, knifeGlanceWeight);
        }

        animatedTarget.localPosition = Vector3.Lerp(animatedTarget.localPosition, targetPos, Time.deltaTime * unhingedLerpSpeed);
        animatedTarget.localRotation = Quaternion.Slerp(animatedTarget.localRotation, targetRot, Time.deltaTime * unhingedLerpSpeed);
        animatedTarget.localScale = Vector3.Lerp(animatedTarget.localScale, baseLocalScale, Time.deltaTime * unhingedLerpSpeed);
    }

    private Transform GetKnifeTarget()
    {
        if (unhingedKnifeTarget != null)
            return unhingedKnifeTarget;

        if (headController != null)
            return headController.GetUnhingedKnifeTarget();

        return null;
    }

    public void PlayDealGrab()
    {
        if (grabRoutine != null)
            StopCoroutine(grabRoutine);

        grabRoutine = StartCoroutine(DealGrabRoutine());
    }

    public void PlayTableSlam()
    {
        if (slamRoutine != null)
            StopCoroutine(slamRoutine);

        slamRoutine = StartCoroutine(TableSlamRoutine());
    }

    public void SnapToCurrentState()
    {
        if (grabRoutine != null)
            StopCoroutine(grabRoutine);

        if (slamRoutine != null)
            StopCoroutine(slamRoutine);

        isPlayingGrab = false;
        isPlayingSlam = false;
        grabRoutine = null;
        slamRoutine = null;
        ApplyCurrentStatePose();
    }

    private IEnumerator TableSlamRoutine()
    {
        isPlayingSlam = true;

        Vector3 startPos = animatedTarget.localPosition;
        Quaternion startRot = animatedTarget.localRotation;
        Vector3 startScale = animatedTarget.localScale;

        Vector3 slamPos = startPos + slamOffset;
        Quaternion slamRot = startRot * Quaternion.Euler(slamRotation);
        Vector3 slamScale = startScale * slamScaleMultiplier;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / slamForwardDuration;
            animatedTarget.localPosition = Vector3.Lerp(startPos, slamPos, t);
            animatedTarget.localRotation = Quaternion.Lerp(startRot, slamRot, t);
            animatedTarget.localScale = Vector3.Lerp(startScale, slamScale, t);
            yield return null;
        }

        animatedTarget.localPosition = slamPos;
        animatedTarget.localRotation = slamRot;
        animatedTarget.localScale = slamScale;

        yield return new WaitForSeconds(slamHoldDuration);

        Vector3 stateTargetPos = GetStateTargetPosition();
        Quaternion stateTargetRot = GetStateTargetRotation();
        Vector3 stateTargetScale = baseLocalScale;

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / slamBackDuration;
            animatedTarget.localPosition = Vector3.Lerp(slamPos, stateTargetPos, t);
            animatedTarget.localRotation = Quaternion.Lerp(slamRot, stateTargetRot, t);
            animatedTarget.localScale = Vector3.Lerp(slamScale, stateTargetScale, t);
            yield return null;
        }

        animatedTarget.localPosition = stateTargetPos;
        animatedTarget.localRotation = stateTargetRot;
        animatedTarget.localScale = stateTargetScale;

        isPlayingSlam = false;
        slamRoutine = null;
    }

    private IEnumerator DealGrabRoutine()
    {
        isPlayingGrab = true;

        Vector3 idlePos = animatedTarget.localPosition;
        Quaternion idleRot = animatedTarget.localRotation;
        Vector3 idleScale = animatedTarget.localScale;

        Vector3 hiddenPos = baseLocalPos + hiddenLocalOffset;
        Vector3 pushedPos = idlePos + dealPushOffset;

        Quaternion pushedRot = idleRot * Quaternion.Euler(0f, 0f, grabTiltAngle);
        Vector3 pushedScale = idleScale * grabScaleMultiplier;

        animatedTarget.localPosition = hiddenPos;
        animatedTarget.localRotation = idleRot;
        animatedTarget.localScale = idleScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / appearDuration;
            animatedTarget.localPosition = Vector3.Lerp(hiddenPos, idlePos, t);
            yield return null;
        }

        animatedTarget.localPosition = idlePos;

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / grabForwardDuration;
            animatedTarget.localPosition = Vector3.Lerp(idlePos, pushedPos, t);
            animatedTarget.localRotation = Quaternion.Lerp(idleRot, pushedRot, t);
            animatedTarget.localScale = Vector3.Lerp(idleScale, pushedScale, t);
            yield return null;
        }

        animatedTarget.localPosition = pushedPos;
        animatedTarget.localRotation = pushedRot;
        animatedTarget.localScale = pushedScale;

        yield return new WaitForSeconds(grabHoldDuration);

        t = 0f;

        Vector3 stateTargetPos = GetStateTargetPosition();
        Quaternion stateTargetRot = GetStateTargetRotation();
        Vector3 stateTargetScale = baseLocalScale;

        while (t < 1f)
        {
            t += Time.deltaTime / grabBackDuration;
            animatedTarget.localPosition = Vector3.Lerp(pushedPos, stateTargetPos, t);
            animatedTarget.localRotation = Quaternion.Lerp(pushedRot, stateTargetRot, t);
            animatedTarget.localScale = Vector3.Lerp(pushedScale, stateTargetScale, t);
            yield return null;
        }

        animatedTarget.localPosition = stateTargetPos;
        animatedTarget.localRotation = stateTargetRot;
        animatedTarget.localScale = stateTargetScale;

        isPlayingGrab = false;
        grabRoutine = null;
    }

    private Vector3 GetStateTargetPosition()
    {
        switch (currentState)
        {
            case AIHandState.Focus:
                return baseLocalPos + new Vector3(focusOffsetX, focusOffsetY, focusOffsetZ);

            case AIHandState.Nervous:
                {
                    float t = Time.time;

                    float shakeX = Mathf.Sin(t * nervousShakeSpeed) * nervousShakeAmountX;
                    float shakeY = Mathf.Sin(t * nervousShakeSpeed * 1.27f) * nervousShakeAmountY;
                    float shakeZ = Mathf.Sin(t * nervousShakeSpeed * 0.89f) * nervousShakeAmountZ;

                    float noiseX =
                        (Mathf.PerlinNoise(t * nervousNoiseSpeed, 0.26f) - 0.5f) * 2f * nervousNoiseAmountX;

                    float noiseY =
                        (Mathf.PerlinNoise(0.68f, t * nervousNoiseSpeed) - 0.5f) * 2f * nervousNoiseAmountY;

                    float noiseZ =
                        (Mathf.PerlinNoise(t * nervousNoiseSpeed, 0.91f) - 0.5f) * 2f * nervousNoiseAmountZ;

                    return baseLocalPos + new Vector3(
                        nervousBaseOffsetX + shakeX + noiseX,
                        nervousBaseOffsetY + shakeY + noiseY,
                        nervousBaseOffsetZ + shakeZ + noiseZ
                    );
                }

            case AIHandState.Suspicious:
                {
                    float t = Time.time;

                    float waveX = Mathf.Sin(t * suspiciousMoveSpeed) * suspiciousMoveAmountX;
                    float waveY = Mathf.Sin(t * suspiciousMoveSpeed * 0.83f) * suspiciousMoveAmountY;
                    float waveZ = Mathf.Sin(t * suspiciousMoveSpeed * 1.19f) * suspiciousMoveAmountZ;

                    float noiseX =
                        (Mathf.PerlinNoise(t * suspiciousNoiseSpeed, 0.23f) - 0.5f) * 2f * suspiciousNoiseAmountX;

                    float noiseY =
                        (Mathf.PerlinNoise(0.61f, t * suspiciousNoiseSpeed) - 0.5f) * 2f * suspiciousNoiseAmountY;

                    float noiseZ =
                        (Mathf.PerlinNoise(t * suspiciousNoiseSpeed, 0.87f) - 0.5f) * 2f * suspiciousNoiseAmountZ;

                    return baseLocalPos + new Vector3(
                        suspiciousBaseOffsetX + waveX + noiseX,
                        suspiciousBaseOffsetY + waveY + noiseY,
                        suspiciousBaseOffsetZ + waveZ + noiseZ
                    );
                }

            case AIHandState.Greedy:
                return baseLocalPos + new Vector3(greedyOffsetX, greedyOffsetY, greedyOffsetZ);

            case AIHandState.Aggressive:
                {
                    float t = Time.time;

                    float waveX = Mathf.Sin(t * aggressiveMoveSpeed) * aggressiveMoveAmountX;
                    float waveY = Mathf.Sin(t * aggressiveMoveSpeed * 1.21f) * aggressiveMoveAmountY;
                    float waveZ = Mathf.Sin(t * aggressiveMoveSpeed * 0.93f) * aggressiveMoveAmountZ;

                    float noiseX =
                        (Mathf.PerlinNoise(t * aggressiveNoiseSpeed, 0.31f) - 0.5f) * 2f * aggressiveNoiseAmountX;

                    float noiseY =
                        (Mathf.PerlinNoise(0.57f, t * aggressiveNoiseSpeed) - 0.5f) * 2f * aggressiveNoiseAmountY;

                    float noiseZ =
                        (Mathf.PerlinNoise(t * aggressiveNoiseSpeed, 0.91f) - 0.5f) * 2f * aggressiveNoiseAmountZ;

                    return baseLocalPos + new Vector3(
                        aggressiveBaseOffsetX + waveX + noiseX,
                        aggressiveBaseOffsetY + waveY + noiseY,
                        aggressiveBaseOffsetZ + waveZ + noiseZ
                    );
                }

            case AIHandState.BluffCalm:
                {
                    float t = Time.time;

                    float waveX = Mathf.Sin(t * bluffCalmMicroWaveSpeed) * bluffCalmMicroWaveAmountX;
                    float waveY = Mathf.Sin(t * bluffCalmMicroWaveSpeed * 1.37f) * bluffCalmMicroWaveAmountY;

                    float noiseX =
                        (Mathf.PerlinNoise(t * bluffCalmNoiseSpeed, 0.19f) - 0.5f) * 2f * bluffCalmNoiseAmountX;

                    float noiseY =
                        (Mathf.PerlinNoise(0.73f, t * bluffCalmNoiseSpeed) - 0.5f) * 2f * bluffCalmNoiseAmountY;

                    float noiseZ =
                        (Mathf.PerlinNoise(t * bluffCalmNoiseSpeed, 0.49f) - 0.5f) * 2f * bluffCalmNoiseAmountZ;

                    return baseLocalPos + new Vector3(
                        bluffCalmBaseOffsetX + waveX + noiseX,
                        bluffCalmBaseOffsetY + waveY + noiseY,
                        bluffCalmBaseOffsetZ + noiseZ
                    );
                }

            case AIHandState.BluffNervous:
                {
                    float t = Time.time;

                    float shakeX = Mathf.Sin(t * bluffNervousShakeSpeed) * bluffNervousShakeAmountX;
                    float shakeY = Mathf.Sin(t * bluffNervousShakeSpeed * 1.31f) * bluffNervousShakeAmountY;
                    float shakeZ = Mathf.Sin(t * bluffNervousShakeSpeed * 0.91f) * bluffNervousShakeAmountZ;

                    float noiseX =
                        (Mathf.PerlinNoise(t * bluffNervousNoiseSpeed, 0.28f) - 0.5f) * 2f * bluffNervousNoiseAmountX;

                    float noiseY =
                        (Mathf.PerlinNoise(0.67f, t * bluffNervousNoiseSpeed) - 0.5f) * 2f * bluffNervousNoiseAmountY;

                    float noiseZ =
                        (Mathf.PerlinNoise(t * bluffNervousNoiseSpeed, 0.93f) - 0.5f) * 2f * bluffNervousNoiseAmountZ;

                    return baseLocalPos + new Vector3(
                        bluffNervousBaseOffsetX + shakeX + noiseX,
                        bluffNervousBaseOffsetY + shakeY + noiseY,
                        bluffNervousBaseOffsetZ + shakeZ + noiseZ
                    );
                }

            case AIHandState.Unhinged:
                {
                    float t = Time.time;

                    float waveX = Mathf.Sin(t * unhingedWaveSpeed) * unhingedWaveAmountX;
                    float waveY = Mathf.Sin(t * unhingedWaveSpeed * 1.41f) * unhingedWaveAmountY;

                    float noiseX =
                        (Mathf.PerlinNoise(t * unhingedNoiseSpeed, 0.18f) - 0.5f) * 2f * unhingedNoiseAmountX;

                    float noiseY =
                        (Mathf.PerlinNoise(0.66f, t * unhingedNoiseSpeed) - 0.5f) * 2f * unhingedNoiseAmountY;

                    float noiseZ =
                        (Mathf.PerlinNoise(t * unhingedNoiseSpeed, 0.92f) - 0.5f) * 2f * unhingedNoiseAmountZ;

                    Vector3 pos = baseLocalPos + new Vector3(
                        unhingedBaseOffsetX + waveX + noiseX,
                        unhingedBaseOffsetY + waveY + noiseY,
                        unhingedBaseOffsetZ + noiseZ
                    );

                    float knifeGlanceWeight = headController != null ? headController.GetUnhingedKnifeGlanceWeight() : 0f;
                    Transform knifeTarget = GetKnifeTarget();

                    if (knifeTarget != null && knifeGlanceWeight > 0.0001f)
                    {
                        Vector3 toKnifeWorld = knifeTarget.position - animatedTarget.position;
                        Vector3 toKnifeLocal = animatedTarget.parent != null
                            ? animatedTarget.parent.InverseTransformDirection(toKnifeWorld.normalized)
                            : transform.InverseTransformDirection(toKnifeWorld.normalized);

                        pos += new Vector3(toKnifeLocal.x, toKnifeLocal.y, 0f) * unhingedKnifeReachAmount * knifeGlanceWeight;
                    }

                    return pos;
                }

            case AIHandState.Panic:
                return animatedTarget.localPosition;

            default:
                {
                    float t = Time.time;
                    float moveY = Mathf.Sin(t * calmFollowSpeed) * calmFollowAmountY;
                    float moveX = Mathf.Sin(t * calmFollowSpeed * 0.7f) * calmFollowAmountX;
                    return baseLocalPos + new Vector3(moveX, moveY, 0f);
                }
        }
    }

    private Quaternion GetStateTargetRotation()
    {
        switch (currentState)
        {
            case AIHandState.Focus:
                return baseLocalRot * Quaternion.Euler(focusRotation);

            case AIHandState.Nervous:
                return baseLocalRot * Quaternion.Euler(nervousRotation);

            case AIHandState.Suspicious:
                return baseLocalRot * Quaternion.Euler(suspiciousRotation);

            case AIHandState.Greedy:
                return baseLocalRot * Quaternion.Euler(greedyRotation);

            case AIHandState.Aggressive:
                return baseLocalRot * Quaternion.Euler(aggressiveRotation);

            case AIHandState.BluffCalm:
                return baseLocalRot * Quaternion.Euler(bluffCalmRotation);

            case AIHandState.BluffNervous:
                return baseLocalRot * Quaternion.Euler(bluffNervousRotation);

            case AIHandState.Unhinged:
                {
                    float knifeGlanceWeight = headController != null ? headController.GetUnhingedKnifeGlanceWeight() : 0f;
                    Quaternion baseUnhingedRot = baseLocalRot * Quaternion.Euler(unhingedRotation);
                    Quaternion knifeRot = baseLocalRot * Quaternion.Euler(unhingedRotation + new Vector3(0f, 0f, unhingedKnifeReachRotZ));
                    return Quaternion.Slerp(baseUnhingedRot, knifeRot, knifeGlanceWeight);
                }

            case AIHandState.Panic:
                return baseLocalRot * Quaternion.Euler(panicRotation);

            default:
                {
                    float t = Time.time;
                    float rotZ = Mathf.Sin(t * calmRotSpeed) * calmRotZ;
                    return baseLocalRot * Quaternion.Euler(calmRotation + new Vector3(0f, 0f, rotZ));
                }
        }
    }

    public void SetStateCalm() => currentState = AIHandState.Calm;
    public void SetStateNervous() => currentState = AIHandState.Nervous;
    public void SetStatePanic() => currentState = AIHandState.Panic;
    public void SetStateFocus() => currentState = AIHandState.Focus;
    public void SetStateSuspicious() => currentState = AIHandState.Suspicious;
    public void SetStateGreedy() => currentState = AIHandState.Greedy;
    public void SetStateAggressive() => currentState = AIHandState.Aggressive;
    public void SetStateBluffCalm() => currentState = AIHandState.BluffCalm;
    public void SetStateBluffNervous() => currentState = AIHandState.BluffNervous;
    public void SetStateUnhinged() => currentState = AIHandState.Unhinged;
    public void SetState(AIHandState newState) => currentState = newState;

    public AIHandState GetCurrentState()
    {
        return currentState;
    }
}