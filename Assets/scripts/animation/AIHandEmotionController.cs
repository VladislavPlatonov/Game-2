using System.Collections;
using UnityEngine;

public class AIHandEmotionController : MonoBehaviour
{
    public enum AIHandState
    {
        Calm,
        Panic,
        Focus
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

    [Header("Panic Motion")]
    [SerializeField] private float panicNoiseSpeed = 26f;
    [SerializeField] private float panicNoiseAmountX = 0.010f;
    [SerializeField] private float panicNoiseAmountY = 0.014f;
    [SerializeField] private float panicLerpSpeed = 16f;

    [Header("Focus Pose")]
    [SerializeField] private float focusOffsetX = 0.05f;
    [SerializeField] private float focusOffsetY = 0.025f;
    [SerializeField] private float focusOffsetZ = 0f;
    [SerializeField] private Vector3 focusRotation = new Vector3(0f, 0f, -9f);
    [SerializeField] private float focusLerpSpeed = 7f;

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

        if (isPlayingGrab)
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
        }
    }

    private void ApplyCalmPose()
    {
        float t = Time.time;

        float moveY = Mathf.Sin(t * calmFollowSpeed) * calmFollowAmountY;
        float moveX = Mathf.Sin(t * calmFollowSpeed * 0.7f) * calmFollowAmountX;
        float rotZ = Mathf.Sin(t * calmRotSpeed) * calmRotZ;

        Vector3 targetPos = baseLocalPos + new Vector3(moveX, moveY, 0f);
        Quaternion targetRot = baseLocalRot * Quaternion.Euler(0f, 0f, rotZ);

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

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * panicLerpSpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, baseLocalRot, Time.deltaTime * panicLerpSpeed);
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

    public void PlayDealGrab()
    {
        if (grabRoutine != null)
            StopCoroutine(grabRoutine);

        grabRoutine = StartCoroutine(DealGrabRoutine());
    }

    public void SnapToCurrentState()
    {
        if (grabRoutine != null)
            StopCoroutine(grabRoutine);

        isPlayingGrab = false;
        grabRoutine = null;
        ApplyCurrentStatePose();
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

            case AIHandState.Panic:
                return baseLocalRot;

            default:
                {
                    float t = Time.time;
                    float rotZ = Mathf.Sin(t * calmRotSpeed) * calmRotZ;
                    return baseLocalRot * Quaternion.Euler(0f, 0f, rotZ);
                }
        }
    }

    public void SetStateCalm()
    {
        currentState = AIHandState.Calm;
    }

    public void SetStatePanic()
    {
        currentState = AIHandState.Panic;
    }

    public void SetStateFocus()
    {
        currentState = AIHandState.Focus;
    }

    public void SetState(AIHandState newState)
    {
        currentState = newState;
    }

    public AIHandState GetCurrentState()
    {
        return currentState;
    }
}
