using System.Collections;
using UnityEngine;

public class AIHandEmotionController : MonoBehaviour
{
    public enum AIHandState
    {
        Calm,
        Panic,
        Focus,
        Suspicious,
        Greedy,
        Aggressive
    }

    [Header("Refs")]
    [SerializeField] private EnemyHeadCalmController headController;
    [SerializeField] private Transform handVisual;

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
    [SerializeField] private float aggressiveOffsetX = 0.018f;
    [SerializeField] private float aggressiveOffsetY = -0.010f;
    [SerializeField] private float aggressiveOffsetZ = 0.020f;
    [SerializeField] private float aggressiveMoveAmountX = 0.020f;
    [SerializeField] private float aggressiveMoveAmountY = 0.018f;
    [SerializeField] private float aggressiveMoveAmountZ = 0.016f;
    [SerializeField] private float aggressiveMoveSpeed = 4f;
    [SerializeField] private Vector3 aggressiveRotation = new Vector3(0f, 0f, -10f);
    [SerializeField] private float aggressiveLerpSpeed = 11f;

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

        if (handVisual == null)
            handVisual = transform;

        baseLocalPos = transform.localPosition;
        baseLocalRot = transform.localRotation;
        baseLocalScale = transform.localScale;
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
        }
    }

    private void ApplyCurrentStatePose()
    {
        switch (currentState)
        {
            case AIHandState.Calm:
                ApplyCalmPose();
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

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * calmLerpSpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * calmLerpSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, baseLocalScale, Time.deltaTime * calmLerpSpeed);
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

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * panicLerpSpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * panicLerpSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, baseLocalScale, Time.deltaTime * panicLerpSpeed);
    }

    private void ApplyFocusPose()
    {
        Vector3 targetPos = baseLocalPos + new Vector3(focusOffsetX, focusOffsetY, focusOffsetZ);
        Quaternion targetRot = baseLocalRot * Quaternion.Euler(focusRotation);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * focusLerpSpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * focusLerpSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, baseLocalScale, Time.deltaTime * focusLerpSpeed);
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

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * suspiciousLerpSpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * suspiciousLerpSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, baseLocalScale, Time.deltaTime * suspiciousLerpSpeed);
    }

    private void ApplyGreedyPose()
    {
        Vector3 targetPos = baseLocalPos + new Vector3(greedyOffsetX, greedyOffsetY, greedyOffsetZ);
        Quaternion targetRot = baseLocalRot * Quaternion.Euler(greedyRotation);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * greedyLerpSpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * greedyLerpSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, baseLocalScale, Time.deltaTime * greedyLerpSpeed);
    }

    private void ApplyAggressivePose()
    {
        float t = Time.time;

        float punchX = Mathf.Sin(t * aggressiveMoveSpeed) * aggressiveMoveAmountX;
        float punchY = Mathf.Sin(t * aggressiveMoveSpeed * 1.21f) * aggressiveMoveAmountY;
        float punchZ = Mathf.Sin(t * aggressiveMoveSpeed * 0.93f) * aggressiveMoveAmountZ;

        Vector3 targetPos = baseLocalPos + new Vector3(
            aggressiveOffsetX + punchX,
            aggressiveOffsetY + punchY,
            aggressiveOffsetZ + punchZ
        );

        Quaternion targetRot = baseLocalRot * Quaternion.Euler(aggressiveRotation);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * aggressiveLerpSpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * aggressiveLerpSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, baseLocalScale, Time.deltaTime * aggressiveLerpSpeed);
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

        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;
        Vector3 startScale = transform.localScale;

        Vector3 slamPos = startPos + slamOffset;
        Quaternion slamRot = startRot * Quaternion.Euler(slamRotation);
        Vector3 slamScale = startScale * slamScaleMultiplier;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / slamForwardDuration;
            transform.localPosition = Vector3.Lerp(startPos, slamPos, t);
            transform.localRotation = Quaternion.Lerp(startRot, slamRot, t);
            transform.localScale = Vector3.Lerp(startScale, slamScale, t);
            yield return null;
        }

        transform.localPosition = slamPos;
        transform.localRotation = slamRot;
        transform.localScale = slamScale;

        yield return new WaitForSeconds(slamHoldDuration);

        Vector3 stateTargetPos = GetStateTargetPosition();
        Quaternion stateTargetRot = GetStateTargetRotation();
        Vector3 stateTargetScale = baseLocalScale;

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / slamBackDuration;
            transform.localPosition = Vector3.Lerp(slamPos, stateTargetPos, t);
            transform.localRotation = Quaternion.Lerp(slamRot, stateTargetRot, t);
            transform.localScale = Vector3.Lerp(slamScale, stateTargetScale, t);
            yield return null;
        }

        transform.localPosition = stateTargetPos;
        transform.localRotation = stateTargetRot;
        transform.localScale = stateTargetScale;

        isPlayingSlam = false;
        slamRoutine = null;
    }

    private IEnumerator DealGrabRoutine()
    {
        isPlayingGrab = true;

        Vector3 idlePos = transform.localPosition;
        Quaternion idleRot = transform.localRotation;
        Vector3 idleScale = transform.localScale;

        Vector3 hiddenPos = baseLocalPos + hiddenLocalOffset;
        Vector3 pushedPos = idlePos + dealPushOffset;

        Quaternion pushedRot = idleRot * Quaternion.Euler(0f, 0f, grabTiltAngle);
        Vector3 pushedScale = idleScale * grabScaleMultiplier;

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

        yield return new WaitForSeconds(grabHoldDuration);

        t = 0f;

        Vector3 stateTargetPos = GetStateTargetPosition();
        Quaternion stateTargetRot = GetStateTargetRotation();
        Vector3 stateTargetScale = baseLocalScale;

        while (t < 1f)
        {
            t += Time.deltaTime / grabBackDuration;
            transform.localPosition = Vector3.Lerp(pushedPos, stateTargetPos, t);
            transform.localRotation = Quaternion.Lerp(pushedRot, stateTargetRot, t);
            transform.localScale = Vector3.Lerp(pushedScale, stateTargetScale, t);
            yield return null;
        }

        transform.localPosition = stateTargetPos;
        transform.localRotation = stateTargetRot;
        transform.localScale = stateTargetScale;

        isPlayingGrab = false;
        grabRoutine = null;
    }

    private Vector3 GetStateTargetPosition()
    {
        switch (currentState)
        {
            case AIHandState.Focus:
                return baseLocalPos + new Vector3(focusOffsetX, focusOffsetY, focusOffsetZ);

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
                    float punchX = Mathf.Sin(t * aggressiveMoveSpeed) * aggressiveMoveAmountX;
                    float punchY = Mathf.Sin(t * aggressiveMoveSpeed * 1.21f) * aggressiveMoveAmountY;
                    float punchZ = Mathf.Sin(t * aggressiveMoveSpeed * 0.93f) * aggressiveMoveAmountZ;

                    return baseLocalPos + new Vector3(
                        aggressiveOffsetX + punchX,
                        aggressiveOffsetY + punchY,
                        aggressiveOffsetZ + punchZ
                    );
                }

            case AIHandState.Panic:
                return transform.localPosition;

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

            case AIHandState.Suspicious:
                return baseLocalRot * Quaternion.Euler(suspiciousRotation);

            case AIHandState.Greedy:
                return baseLocalRot * Quaternion.Euler(greedyRotation);

            case AIHandState.Aggressive:
                return baseLocalRot * Quaternion.Euler(aggressiveRotation);

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
    public void SetStatePanic() => currentState = AIHandState.Panic;
    public void SetStateFocus() => currentState = AIHandState.Focus;
    public void SetStateSuspicious() => currentState = AIHandState.Suspicious;
    public void SetStateGreedy() => currentState = AIHandState.Greedy;
    public void SetStateAggressive() => currentState = AIHandState.Aggressive;
    public void SetState(AIHandState newState) => currentState = newState;

    public AIHandState GetCurrentState()
    {
        return currentState;
    }
}
