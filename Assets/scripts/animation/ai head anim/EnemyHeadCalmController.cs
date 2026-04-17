using UnityEngine;

public class EnemyHeadCalmController : MonoBehaviour
{
    public enum EmotionState
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

    public enum EyeSpinAxis
    {
        X,
        Y,
        Z
    }

    [Header("Refs")]
    [SerializeField] private Transform headMotionPivot;
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;

    [Header("Focus Look Targets")]
    [SerializeField] private Transform focusTableTarget;
    [SerializeField] private Transform focusCardsTarget;

    [Header("Player Look Target")]
    [SerializeField] private Transform greedyPlayerTarget;

    [Header("Unhinged Look Target")]
    [SerializeField] private Transform unhingedKnifeTarget;

    [Header("Current State")]
    [SerializeField] private EmotionState currentState = EmotionState.Calm;

    [Header("Blend")]
    [SerializeField] private float stateBlendSpeed = 3f;

    [Header("Calm Head Motion")]
    [SerializeField] private float calmMoveAmountY = 0.02f;
    [SerializeField] private float calmMoveAmountX = 0.006f;
    [SerializeField] private float calmMoveSpeed = 0.55f;
    [SerializeField] private float calmRotAmountZ = 1.5f;
    [SerializeField] private float calmRotSpeed = 0.45f;

    [Header("Calm Eye Spin")]
    [SerializeField] private float calmEyeSpinSpeed = 25f;
    [SerializeField] private EyeSpinAxis leftEyeAxis = EyeSpinAxis.X;
    [SerializeField] private EyeSpinAxis rightEyeAxis = EyeSpinAxis.X;
    [SerializeField] private bool leftEyeClockwise = true;
    [SerializeField] private bool rightEyeClockwise = true;

    [Header("Nervous Head")]
    [SerializeField] private Vector3 nervousHeadBaseRotation = new Vector3(-2f, 0f, 0f);
    [SerializeField] private float nervousOffsetX = 0.002f;
    [SerializeField] private float nervousOffsetY = -0.004f;
    [SerializeField] private float nervousOffsetZ = 0.010f;
    [SerializeField] private float nervousShakeSpeed = 5f;
    [SerializeField] private float nervousShakeAmountX = 0.0025f;
    [SerializeField] private float nervousShakeAmountY = 0.003f;
    [SerializeField] private float nervousNoiseSpeed = 3f;
    [SerializeField] private float nervousRotNoiseZ = 0.6f;
    [SerializeField] private float nervousHeadMoveLerpSpeed = 8f;
    [SerializeField] private float nervousHeadRotLerpSpeed = 9f;
    [SerializeField] private float nervousJerkIntervalMin = 5f;
    [SerializeField] private float nervousJerkIntervalMax = 6f;
    [SerializeField] private float nervousJerkDuration = 0.14f;
    [SerializeField] private Vector3 nervousJerkRotation = new Vector3(-8f, 5f, 3f);

    [Header("Nervous Eyes")]
    [SerializeField] private Vector3 nervousEyeBasePosition = Vector3.zero;
    [SerializeField] private Vector3 nervousEyeBaseRotation = new Vector3(-1.5f, 0f, 0f);
    [SerializeField] private float nervousEyeMoveAmountX = 0.008f;
    [SerializeField] private float nervousEyeMoveAmountY = 0.006f;
    [SerializeField] private float nervousEyeMoveSpeed = 9f;
    [SerializeField] private float nervousEyeSpinSpeed = 6f;
    [SerializeField] private float nervousEyeVerticalScale = 0.91f;
    [SerializeField] private float nervousEyeHorizontalScale = 1.0f;
    [SerializeField] private float nervousEyeLerpSpeed = 11f;
    [SerializeField] private float nervousEyeYawJitter = 1.1f;
    [SerializeField] private float nervousEyePitchJitter = 0.8f;
    [SerializeField] private float nervousEyeTargetYawMultiplier = 0.8f;
    [SerializeField] private float nervousEyeTargetPitchMultiplier = 0.8f;

    [Header("Panic Head Motion")]
    [SerializeField] private float panicNoiseSpeed = 28f;
    [SerializeField] private float panicNoiseAmountX = 0.018f;
    [SerializeField] private float panicNoiseAmountY = 0.014f;
    [SerializeField] private Vector3 panicHeadBaseRotation = Vector3.zero;

    [Header("Panic Eye Spin")]
    [SerializeField] private float panicEyeSpinSpeed = 75f;

    [Header("Focus Head")]
    [SerializeField] private float focusHeadMoveLerpSpeed = 6f;
    [SerializeField] private float focusHeadRotLerpSpeed = 6f;
    [SerializeField] private float focusLookSwitchSpeed = 1.4f;
    [SerializeField] private float focusLookBias = 0.5f;
    [SerializeField] private float focusHeadOffsetX = 0.02f;
    [SerializeField] private float focusHeadOffsetY = -0.015f;
    [SerializeField] private float focusHeadPositionInfluenceX = 0.03f;
    [SerializeField] private float focusHeadPositionInfluenceY = 0.03f;
    [SerializeField] private float focusMaxHeadOffsetX = 0.04f;
    [SerializeField] private float focusMaxHeadOffsetY = 0.04f;
    [SerializeField] private float focusPitchMultiplier = 0.7f;
    [SerializeField] private float focusYawMultiplier = 0.45f;
    [SerializeField] private float focusRollMultiplier = 0.08f;
    [SerializeField] private Vector3 focusHeadBaseRotation = Vector3.zero;

    [Header("Focus Eyes")]
    [SerializeField] private float focusEyeLookLerpSpeed = 10f;
    [SerializeField] private Vector3 focusEyeBasePosition = Vector3.zero;
    [SerializeField] private Vector3 focusEyeBaseRotation = Vector3.zero;
    [SerializeField] private float focusEyeVerticalScale = 0.72f;
    [SerializeField] private float focusEyeHorizontalScale = 1.02f;
    [SerializeField] private float focusEyeScaleLerpSpeed = 8f;
    [SerializeField] private float focusEyeYawMultiplier = 1.0f;
    [SerializeField] private float focusEyePitchMultiplier = 1.0f;

    [Header("Suspicious Head")]
    [SerializeField] private float suspiciousTiltAmountZ = 5.5f;
    [SerializeField] private float suspiciousTiltSpeed = 0.70f;
    [SerializeField] private float suspiciousHeadLerpSpeed = 5f;
    [SerializeField] private Vector3 suspiciousHeadBaseRotation = new Vector3(-5f, 8f, 0f);

    [Header("Suspicious Eyes")]
    [SerializeField] private Vector3 suspiciousEyeBasePosition = Vector3.zero;
    [SerializeField] private Vector3 suspiciousEyeBaseRotation = new Vector3(-6f, 10f, 0f);
    [SerializeField] private float suspiciousEyeMoveYaw = 4f;
    [SerializeField] private float suspiciousEyeMovePitch = 2f;
    [SerializeField] private float suspiciousEyeMoveSpeed = 0.85f;
    [SerializeField] private float suspiciousEyeSpinSpeed = 5f;
    [SerializeField] private float suspiciousEyeVerticalScale = 0.95f;
    [SerializeField] private float suspiciousEyeHorizontalScale = 1.0f;
    [SerializeField] private float suspiciousEyeLerpSpeed = 7f;

    [Header("Greedy Head")]
    [SerializeField] private float greedyOffsetX = 0.0f;
    [SerializeField] private float greedyOffsetY = 0.0f;
    [SerializeField] private float greedyOffsetZ = 0.02f;
    [SerializeField] private float greedyPitchMultiplier = 0.45f;
    [SerializeField] private float greedyYawMultiplier = 0.35f;
    [SerializeField] private float greedyRollMultiplier = 0.03f;
    [SerializeField] private float greedyHeadMoveLerpSpeed = 6f;
    [SerializeField] private float greedyHeadRotLerpSpeed = 6f;
    [SerializeField] private Vector3 greedyHeadBaseRotation = Vector3.zero;

    [Header("Greedy Eyes")]
    [SerializeField] private Vector3 greedyEyeBasePosition = Vector3.zero;
    [SerializeField] private Vector3 greedyEyeBaseRotation = Vector3.zero;
    [SerializeField] private float greedyEyeYawMultiplier = 0.9f;
    [SerializeField] private float greedyEyePitchMultiplier = 0.9f;
    [SerializeField] private float greedyEyeSpinSpeed = 4f;
    [SerializeField] private float greedyEyeVerticalScale = 0.96f;
    [SerializeField] private float greedyEyeHorizontalScale = 1.0f;
    [SerializeField] private float greedyEyeLerpSpeed = 7f;

    [Header("Aggressive Head")]
    [SerializeField] private Vector3 aggressiveHeadBaseRotation = new Vector3(-6f, 0f, 0f);
    [SerializeField] private float aggressiveOffsetZ = 0.03f;
    [SerializeField] private float aggressivePitchMultiplier = 0.75f;
    [SerializeField] private float aggressiveYawMultiplier = 0.65f;
    [SerializeField] private float aggressiveRollMultiplier = 0.05f;
    [SerializeField] private float aggressiveHeadMoveLerpSpeed = 10f;
    [SerializeField] private float aggressiveHeadRotLerpSpeed = 12f;
    [SerializeField] private float aggressivePunchAngle = 8f;
    [SerializeField] private float aggressivePunchSpeed = 8f;
    [SerializeField] private float aggressivePunchAmount = 0.35f;

    [Header("Aggressive Eyes")]
    [SerializeField] private Vector3 aggressiveEyeBasePosition = Vector3.zero;
    [SerializeField] private Vector3 aggressiveEyeBaseRotation = new Vector3(-4f, 0f, 0f);
    [SerializeField] private float aggressiveEyeMoveYaw = 1.5f;
    [SerializeField] private float aggressiveEyeMovePitch = 1f;
    [SerializeField] private float aggressiveEyeMoveSpeed = 1.2f;
    [SerializeField] private float aggressiveEyeVerticalScale = 0.86f;
    [SerializeField] private float aggressiveEyeHorizontalScale = 1.02f;
    [SerializeField] private float aggressiveEyeLerpSpeed = 12f;

    [Header("Bluff Calm Head")]
    [SerializeField] private Vector3 bluffCalmHeadBaseRotation = new Vector3(-2f, 0f, 0f);
    [SerializeField] private float bluffCalmOffsetX = 0.0f;
    [SerializeField] private float bluffCalmOffsetY = -0.003f;
    [SerializeField] private float bluffCalmOffsetZ = 0.012f;
    [SerializeField] private float bluffCalmNoiseSpeed = 1.8f;
    [SerializeField] private float bluffCalmNoiseAmountX = 0.0025f;
    [SerializeField] private float bluffCalmNoiseAmountY = 0.003f;
    [SerializeField] private float bluffCalmNoiseRotZ = 0.8f;
    [SerializeField] private float bluffCalmHeadMoveLerpSpeed = 8f;
    [SerializeField] private float bluffCalmHeadRotLerpSpeed = 9f;

    [Header("Bluff Calm Eyes")]
    [SerializeField] private Vector3 bluffCalmEyeBasePosition = Vector3.zero;
    [SerializeField] private Vector3 bluffCalmEyeBaseRotation = new Vector3(-1f, 0f, 0f);
    [SerializeField] private float bluffCalmEyeSpinSpeed = 18f;
    [SerializeField] private float bluffCalmEyeVerticalScale = 0.90f;
    [SerializeField] private float bluffCalmEyeHorizontalScale = 1.01f;
    [SerializeField] private float bluffCalmEyeLerpSpeed = 9f;
    [SerializeField] private float bluffCalmEyeMicroYaw = 0.8f;
    [SerializeField] private float bluffCalmEyeMicroPitch = 0.5f;
    [SerializeField] private float bluffCalmEyeMicroSpeed = 0.9f;

    [Header("Bluff Nervous Head")]
    [SerializeField] private Vector3 bluffNervousHeadBaseRotation = new Vector3(-1.5f, 1.5f, 0f);
    [SerializeField] private float bluffNervousOffsetX = 0.002f;
    [SerializeField] private float bluffNervousOffsetY = -0.004f;
    [SerializeField] private float bluffNervousOffsetZ = 0.016f;
    [SerializeField] private float bluffNervousShakeSpeed = 10f;
    [SerializeField] private float bluffNervousShakeAmountX = 0.004f;
    [SerializeField] private float bluffNervousShakeAmountY = 0.005f;
    [SerializeField] private float bluffNervousRotNoiseSpeed = 7f;
    [SerializeField] private float bluffNervousRotNoiseZ = 1.4f;
    [SerializeField] private float bluffNervousHeadMoveLerpSpeed = 12f;
    [SerializeField] private float bluffNervousHeadRotLerpSpeed = 13f;

    [Header("Bluff Nervous Eyes")]
    [SerializeField] private Vector3 bluffNervousEyeBasePosition = Vector3.zero;
    [SerializeField] private Vector3 bluffNervousEyeBaseRotation = new Vector3(-2f, 0f, 0f);
    [SerializeField] private float bluffNervousEyeMoveAmountX = 0.018f;
    [SerializeField] private float bluffNervousEyeMoveAmountY = 0.014f;
    [SerializeField] private float bluffNervousEyeMoveSpeed = 16f;
    [SerializeField] private float bluffNervousEyeSpinSpeed = 8f;
    [SerializeField] private float bluffNervousEyeVerticalScale = 0.88f;
    [SerializeField] private float bluffNervousEyeHorizontalScale = 1.02f;
    [SerializeField] private float bluffNervousEyeLerpSpeed = 16f;
    [SerializeField] private float bluffNervousEyeYaw = 2f;
    [SerializeField] private float bluffNervousEyePitch = 1.2f;

    [Header("Unhinged Head")]
    [SerializeField] private Vector3 unhingedHeadBaseRotation = new Vector3(-3f, 0f, 0f);
    [SerializeField] private float unhingedOffsetX = 0.003f;
    [SerializeField] private float unhingedOffsetY = -0.002f;
    [SerializeField] private float unhingedOffsetZ = 0.020f;
    [SerializeField] private float unhingedHeadMoveLerpSpeed = 11f;
    [SerializeField] private float unhingedHeadRotLerpSpeed = 13f;
    [SerializeField] private float unhingedPitchMultiplier = 0.9f;
    [SerializeField] private float unhingedYawMultiplier = 0.9f;
    [SerializeField] private float unhingedRollMultiplier = 0.12f;
    [SerializeField] private float unhingedNoiseSpeed = 2.4f;
    [SerializeField] private float unhingedNoiseAmountX = 0.002f;
    [SerializeField] private float unhingedNoiseAmountY = 0.003f;
    [SerializeField] private float unhingedNoiseRotZ = 1.2f;
    [SerializeField] private float unhingedKnifeLookIntervalMin = 2.5f;
    [SerializeField] private float unhingedKnifeLookIntervalMax = 5.5f;
    [SerializeField] private float unhingedKnifeLookDuration = 0.45f;

    [Header("Unhinged Eyes")]
    [SerializeField] private Vector3 unhingedEyeBasePosition = Vector3.zero;
    [SerializeField] private Vector3 unhingedEyeBaseRotation = new Vector3(-2f, 0f, 0f);
    [SerializeField] private float unhingedEyeMoveAmountX = 0.012f;
    [SerializeField] private float unhingedEyeMoveAmountY = 0.008f;
    [SerializeField] private float unhingedEyeMoveSpeed = 6.5f;
    [SerializeField] private float unhingedEyeSpinSpeed = 3f;
    [SerializeField] private float unhingedEyeVerticalScale = 0.84f;
    [SerializeField] private float unhingedEyeHorizontalScale = 1.03f;
    [SerializeField] private float unhingedEyeLerpSpeed = 14f;
    [SerializeField] private float unhingedEyeYawMultiplier = 1.05f;
    [SerializeField] private float unhingedEyePitchMultiplier = 1.05f;

    [Header("Smoothing")]
    [SerializeField] private float eyeRotationLerpSpeed = 12f;
    [SerializeField] private float eyePositionLerpSpeed = 12f;

    private Vector3 headBaseLocalPos;
    private Quaternion headBaseLocalRot;

    private Quaternion leftEyeBaseRot;
    private Quaternion rightEyeBaseRot;

    private Vector3 leftEyeBaseScale;
    private Vector3 rightEyeBaseScale;

    private Vector3 leftEyeBasePos;
    private Vector3 rightEyeBasePos;

    private float leftEyeAngle;
    private float rightEyeAngle;

    private float calmWeight = 1f;
    private float nervousWeight = 0f;
    private float panicWeight = 0f;
    private float focusWeight = 0f;
    private float suspiciousWeight = 0f;
    private float greedyWeight = 0f;
    private float aggressiveWeight = 0f;
    private float bluffCalmWeight = 0f;
    private float bluffNervousWeight = 0f;
    private float unhingedWeight = 0f;

    private float nextNervousJerkTime;
    private float nervousJerkTimer;

    private float nextUnhingedKnifeLookTime;
    private float unhingedKnifeLookTimer;

    private void Awake()
    {
        if (headMotionPivot == null)
            headMotionPivot = transform;

        headBaseLocalPos = headMotionPivot.localPosition;
        headBaseLocalRot = headMotionPivot.localRotation;

        if (leftEye != null)
        {
            leftEyeBaseRot = leftEye.localRotation;
            leftEyeBaseScale = leftEye.localScale;
            leftEyeBasePos = leftEye.localPosition;
        }

        if (rightEye != null)
        {
            rightEyeBaseRot = rightEye.localRotation;
            rightEyeBaseScale = rightEye.localScale;
            rightEyeBasePos = rightEye.localPosition;
        }

        ScheduleNextNervousJerk();
        ScheduleNextUnhingedKnifeLook();
    }

    private void Update()
    {
        UpdateStateWeights();
        UpdateNervousJerk();
        UpdateUnhingedKnifeLook();
        AnimateHead();
        AnimateEyes();
    }

    private void UpdateStateWeights()
    {
        float targetCalm = currentState == EmotionState.Calm ? 1f : 0f;
        float targetNervous = currentState == EmotionState.Nervous ? 1f : 0f;
        float targetPanic = currentState == EmotionState.Panic ? 1f : 0f;
        float targetFocus = currentState == EmotionState.Focus ? 1f : 0f;
        float targetSuspicious = currentState == EmotionState.Suspicious ? 1f : 0f;
        float targetGreedy = currentState == EmotionState.Greedy ? 1f : 0f;
        float targetAggressive = currentState == EmotionState.Aggressive ? 1f : 0f;
        float targetBluffCalm = currentState == EmotionState.BluffCalm ? 1f : 0f;
        float targetBluffNervous = currentState == EmotionState.BluffNervous ? 1f : 0f;
        float targetUnhinged = currentState == EmotionState.Unhinged ? 1f : 0f;

        calmWeight = Mathf.Lerp(calmWeight, targetCalm, Time.deltaTime * stateBlendSpeed);
        nervousWeight = Mathf.Lerp(nervousWeight, targetNervous, Time.deltaTime * stateBlendSpeed);
        panicWeight = Mathf.Lerp(panicWeight, targetPanic, Time.deltaTime * stateBlendSpeed);
        focusWeight = Mathf.Lerp(focusWeight, targetFocus, Time.deltaTime * stateBlendSpeed);
        suspiciousWeight = Mathf.Lerp(suspiciousWeight, targetSuspicious, Time.deltaTime * stateBlendSpeed);
        greedyWeight = Mathf.Lerp(greedyWeight, targetGreedy, Time.deltaTime * stateBlendSpeed);
        aggressiveWeight = Mathf.Lerp(aggressiveWeight, targetAggressive, Time.deltaTime * stateBlendSpeed);
        bluffCalmWeight = Mathf.Lerp(bluffCalmWeight, targetBluffCalm, Time.deltaTime * stateBlendSpeed);
        bluffNervousWeight = Mathf.Lerp(bluffNervousWeight, targetBluffNervous, Time.deltaTime * stateBlendSpeed);
        unhingedWeight = Mathf.Lerp(unhingedWeight, targetUnhinged, Time.deltaTime * stateBlendSpeed);

        float total =
            calmWeight +
            nervousWeight +
            panicWeight +
            focusWeight +
            suspiciousWeight +
            greedyWeight +
            aggressiveWeight +
            bluffCalmWeight +
            bluffNervousWeight +
            unhingedWeight;

        if (total > 0.0001f)
        {
            calmWeight /= total;
            nervousWeight /= total;
            panicWeight /= total;
            focusWeight /= total;
            suspiciousWeight /= total;
            greedyWeight /= total;
            aggressiveWeight /= total;
            bluffCalmWeight /= total;
            bluffNervousWeight /= total;
            unhingedWeight /= total;
        }
    }

    private void UpdateNervousJerk()
    {
        if (currentState != EmotionState.Nervous)
        {
            nervousJerkTimer = 0f;
            return;
        }

        if (Time.time >= nextNervousJerkTime && nervousJerkTimer <= 0f)
        {
            nervousJerkTimer = nervousJerkDuration;
            ScheduleNextNervousJerk();
        }

        if (nervousJerkTimer > 0f)
            nervousJerkTimer -= Time.deltaTime;
    }

    private void UpdateUnhingedKnifeLook()
    {
        if (currentState != EmotionState.Unhinged)
        {
            unhingedKnifeLookTimer = 0f;
            return;
        }

        if (Time.time >= nextUnhingedKnifeLookTime && unhingedKnifeLookTimer <= 0f)
        {
            unhingedKnifeLookTimer = unhingedKnifeLookDuration;
            ScheduleNextUnhingedKnifeLook();
        }

        if (unhingedKnifeLookTimer > 0f)
            unhingedKnifeLookTimer -= Time.deltaTime;
    }

    private void ScheduleNextNervousJerk()
    {
        nextNervousJerkTime = Time.time + Random.Range(nervousJerkIntervalMin, nervousJerkIntervalMax);
    }

    private void ScheduleNextUnhingedKnifeLook()
    {
        nextUnhingedKnifeLookTime = Time.time + Random.Range(unhingedKnifeLookIntervalMin, unhingedKnifeLookIntervalMax);
    }

    private float GetNervousJerkWeight()
    {
        if (nervousJerkDuration <= 0.0001f || nervousJerkTimer <= 0f)
            return 0f;

        float normalized = 1f - (nervousJerkTimer / nervousJerkDuration);
        return Mathf.Sin(normalized * Mathf.PI);
    }

    public float GetUnhingedKnifeGlanceWeight()
    {
        if (unhingedKnifeLookDuration <= 0.0001f || unhingedKnifeLookTimer <= 0f)
            return 0f;

        float normalized = 1f - (unhingedKnifeLookTimer / unhingedKnifeLookDuration);
        return Mathf.Sin(normalized * Mathf.PI);
    }

    public Transform GetUnhingedKnifeTarget()
    {
        return unhingedKnifeTarget;
    }
    private void AnimateHead()
    {
        float t = Time.time;

        float calmMoveY = Mathf.Sin(t * calmMoveSpeed) * calmMoveAmountY;
        float calmMoveX = Mathf.Sin(t * calmMoveSpeed * 0.7f) * calmMoveAmountX;
        float calmRotZ = Mathf.Sin(t * calmRotSpeed) * calmRotAmountZ;

        float nervousPosX = nervousOffsetX + Mathf.Sin(t * nervousShakeSpeed) * nervousShakeAmountX;
        float nervousPosY = nervousOffsetY + Mathf.Sin(t * nervousShakeSpeed * 1.19f) * nervousShakeAmountY;
        float nervousRotZ = (Mathf.PerlinNoise(t * nervousNoiseSpeed, 0.82f) - 0.5f) * 2f * nervousRotNoiseZ;

        Vector3 nervousOffsetPos = new Vector3(nervousPosX, nervousPosY, nervousOffsetZ);
        Quaternion nervousOffsetRot = Quaternion.Euler(nervousHeadBaseRotation + new Vector3(0f, 0f, nervousRotZ));
        Quaternion nervousJerkRot = Quaternion.Euler(nervousJerkRotation * GetNervousJerkWeight());

        float panicVibeX = (Mathf.PerlinNoise(t * panicNoiseSpeed, 0.17f) - 0.5f) * 2f * panicNoiseAmountX;
        float panicVibeY = (Mathf.PerlinNoise(0.41f, t * panicNoiseSpeed * 1.11f) - 0.5f) * 2f * panicNoiseAmountY;

        Vector3 focusOffsetPos = Vector3.zero;
        Quaternion focusOffsetRot = Quaternion.identity;

        Transform focusTarget = GetCurrentFocusTarget();
        if (focusTarget != null)
        {
            Vector3 toTarget = (focusTarget.position - headMotionPivot.position).normalized;
            Vector3 localDir = transform.InverseTransformDirection(toTarget);

            float pitch = -localDir.y * 35f * focusPitchMultiplier;
            float yaw = localDir.x * 30f * focusYawMultiplier;
            float roll = -localDir.x * 10f * focusRollMultiplier;

            focusOffsetPos = new Vector3(focusHeadOffsetX, focusHeadOffsetY, 0f);
            focusOffsetRot = Quaternion.Euler(focusHeadBaseRotation) * Quaternion.Euler(pitch, yaw, roll);
        }

        float suspiciousRotZ = Mathf.Sin(t * suspiciousTiltSpeed) * suspiciousTiltAmountZ;
        Quaternion suspiciousOffsetRot = Quaternion.Euler(suspiciousHeadBaseRotation + new Vector3(0f, 0f, suspiciousRotZ));

        Vector3 greedyOffsetPos = new Vector3(greedyOffsetX, greedyOffsetY, greedyOffsetZ);
        Quaternion greedyOffsetRot = Quaternion.Euler(greedyHeadBaseRotation);

        if (greedyPlayerTarget != null)
        {
            Vector3 dir = (greedyPlayerTarget.position - headMotionPivot.position).normalized;
            Vector3 local = transform.InverseTransformDirection(dir);

            float pitch = -local.y * 35f * greedyPitchMultiplier;
            float yaw = local.x * 35f * greedyYawMultiplier;
            float roll = -local.x * 8f * greedyRollMultiplier;

            greedyOffsetRot = Quaternion.Euler(greedyHeadBaseRotation) * Quaternion.Euler(pitch, yaw, roll);
        }

        Vector3 aggressiveOffsetPos = new Vector3(0f, 0f, aggressiveOffsetZ);
        Quaternion aggressiveOffsetRot = Quaternion.Euler(aggressiveHeadBaseRotation);

        if (greedyPlayerTarget != null)
        {
            Vector3 dir = (greedyPlayerTarget.position - headMotionPivot.position).normalized;
            Vector3 local = transform.InverseTransformDirection(dir);

            float pitch = -local.y * 35f * aggressivePitchMultiplier;
            float yaw = local.x * 35f * aggressiveYawMultiplier;
            float roll = -local.x * 8f * aggressiveRollMultiplier;

            float punch = Mathf.Sin(t * aggressivePunchSpeed) * aggressivePunchAngle * aggressivePunchAmount;

            aggressiveOffsetRot =
                Quaternion.Euler(aggressiveHeadBaseRotation) *
                Quaternion.Euler(pitch + punch, yaw, roll);
        }

        // ===== UNHINGED =====
        Vector3 unhingedOffsetPos = new Vector3(unhingedOffsetX, unhingedOffsetY, unhingedOffsetZ);
        Quaternion unhingedOffsetRot = Quaternion.Euler(unhingedHeadBaseRotation);

        if (unhingedWeight > 0.0001f)
        {
            float knifeWeight = GetUnhingedKnifeGlanceWeight();

            Transform target = greedyPlayerTarget;

            if (unhingedKnifeTarget != null && knifeWeight > 0f)
                target = unhingedKnifeTarget;

            if (target != null)
            {
                Vector3 dir = (target.position - headMotionPivot.position).normalized;
                Vector3 local = transform.InverseTransformDirection(dir);

                float pitch = local.y * 35f * unhingedPitchMultiplier;
                float yaw = -local.x * 35f * unhingedYawMultiplier;
                float roll = -local.x * 10f * unhingedRollMultiplier;

                float noiseX = (Mathf.PerlinNoise(t * unhingedNoiseSpeed, 0.17f) - 0.5f) * 2f * unhingedNoiseAmountX;
                float noiseY = (Mathf.PerlinNoise(0.53f, t * unhingedNoiseSpeed) - 0.5f) * 2f * unhingedNoiseAmountY;
                float noiseRot = (Mathf.PerlinNoise(t * unhingedNoiseSpeed, 0.91f) - 0.5f) * 2f * unhingedNoiseRotZ;

                unhingedOffsetPos += new Vector3(noiseX, noiseY, 0f);
                unhingedOffsetRot =
                    Quaternion.Euler(unhingedHeadBaseRotation) *
                    Quaternion.Euler(pitch, yaw, roll + noiseRot);
            }
        }

        Vector3 blendedPos =
            new Vector3(calmMoveX, calmMoveY, 0f) * calmWeight +
            nervousOffsetPos * nervousWeight +
            new Vector3(panicVibeX, panicVibeY, 0f) * panicWeight +
            focusOffsetPos * focusWeight +
            greedyOffsetPos * greedyWeight +
            aggressiveOffsetPos * aggressiveWeight +
            unhingedOffsetPos * unhingedWeight;

        Quaternion targetRot =
            headBaseLocalRot *
            Quaternion.Slerp(Quaternion.identity, nervousOffsetRot * nervousJerkRot, nervousWeight) *
            Quaternion.Slerp(Quaternion.identity, focusOffsetRot, focusWeight) *
            Quaternion.Slerp(Quaternion.identity, suspiciousOffsetRot, suspiciousWeight) *
            Quaternion.Slerp(Quaternion.identity, greedyOffsetRot, greedyWeight) *
            Quaternion.Slerp(Quaternion.identity, aggressiveOffsetRot, aggressiveWeight) *
            Quaternion.Slerp(Quaternion.identity, unhingedOffsetRot, unhingedWeight);

        Vector3 targetPos = headBaseLocalPos + blendedPos;

        float posSpeed = Mathf.Lerp(8f, unhingedHeadMoveLerpSpeed, unhingedWeight);
        float rotSpeed = Mathf.Lerp(8f, unhingedHeadRotLerpSpeed, unhingedWeight);

        headMotionPivot.localPosition = Vector3.Lerp(headMotionPivot.localPosition, targetPos, Time.deltaTime * posSpeed);
        headMotionPivot.localRotation = Quaternion.Slerp(headMotionPivot.localRotation, targetRot, Time.deltaTime * rotSpeed);
    }

    private void AnimateEyes()
    {
        AnimateSingleEye(leftEye, leftEyeBaseRot, leftEyeBaseScale, leftEyeBasePos, leftEyeAxis, leftEyeClockwise, ref leftEyeAngle);
        AnimateSingleEye(rightEye, rightEyeBaseRot, rightEyeBaseScale, rightEyeBasePos, rightEyeAxis, rightEyeClockwise, ref rightEyeAngle);
    }

    private void AnimateSingleEye(
    Transform eye,
    Quaternion baseRot,
    Vector3 baseScale,
    Vector3 basePos,
    EyeSpinAxis axis,
    bool clockwise,
    ref float eyeAngle)
    {
        if (eye == null)
            return;

        Quaternion targetRot = baseRot;
        Vector3 targetScale = baseScale;
        Vector3 targetPos = basePos;

        // ===== Calm / Panic spin =====
        float spinWeight = calmWeight + panicWeight;
        if (spinWeight > 0.0001f)
        {
            float speed = GetCurrentEyeSpeed();
            float dir = clockwise ? -1f : 1f;

            eyeAngle += speed * dir * Time.deltaTime;

            Quaternion spinOffset = MakeAxisRotation(eyeAngle, axis);
            Quaternion spinTarget = baseRot * spinOffset;

            targetRot = Quaternion.Slerp(targetRot, spinTarget, spinWeight);
        }

        // ===== Nervous =====
        if (nervousWeight > 0.0001f)
        {
            float dir = clockwise ? -1f : 1f;
            eyeAngle += nervousEyeSpinSpeed * dir * Time.deltaTime;

            float posX = Mathf.Sin(Time.time * nervousEyeMoveSpeed) * nervousEyeMoveAmountX;
            float posY = Mathf.Sin(Time.time * nervousEyeMoveSpeed * 1.33f) * nervousEyeMoveAmountY;

            float extraYaw = Mathf.Sin(Time.time * nervousEyeMoveSpeed * 0.87f) * nervousEyeYawJitter;
            float extraPitch = Mathf.Sin(Time.time * nervousEyeMoveSpeed * 1.11f) * nervousEyePitchJitter;

            Quaternion nervousSpinOffset = MakeAxisRotation(eyeAngle, axis);
            Quaternion nervousLookRot = baseRot * Quaternion.Euler(nervousEyeBaseRotation);

            if (greedyPlayerTarget != null)
            {
                Vector3 toTargetWorld = greedyPlayerTarget.position - eye.position;
                Vector3 toTargetLocal = eye.parent != null
                    ? eye.parent.InverseTransformDirection(toTargetWorld.normalized)
                    : transform.InverseTransformDirection(toTargetWorld.normalized);

                float pitch = -toTargetLocal.y * 35f * nervousEyeTargetPitchMultiplier;
                float yaw = toTargetLocal.x * 35f * nervousEyeTargetYawMultiplier;

                nervousLookRot =
                    baseRot *
                    Quaternion.Euler(nervousEyeBaseRotation) *
                    Quaternion.Euler(pitch + extraPitch, yaw + extraYaw, 0f);
            }
            else
            {
                nervousLookRot =
                    baseRot *
                    Quaternion.Euler(nervousEyeBaseRotation + new Vector3(extraPitch, extraYaw, 0f));
            }

            Quaternion nervousTarget = nervousLookRot * nervousSpinOffset;

            targetRot = Quaternion.Slerp(targetRot, nervousTarget, nervousWeight);
            targetPos = Vector3.Lerp(
                targetPos,
                basePos + nervousEyeBasePosition + new Vector3(posX, posY, 0f),
                nervousWeight
            );

            Vector3 nervousScale = new Vector3(
                baseScale.x * nervousEyeHorizontalScale,
                baseScale.y * nervousEyeVerticalScale,
                baseScale.z
            );

            targetScale = Vector3.Lerp(targetScale, nervousScale, nervousWeight);
        }

        // ===== Focus =====
        Transform activeFocusTarget = GetCurrentFocusTarget();
        if (activeFocusTarget != null && focusWeight > 0.0001f)
        {
            Vector3 toTargetWorld = activeFocusTarget.position - eye.position;
            Vector3 toTargetLocal = eye.parent != null
                ? eye.parent.InverseTransformDirection(toTargetWorld.normalized)
                : transform.InverseTransformDirection(toTargetWorld.normalized);

            float pitch = -toTargetLocal.y * 35f * focusEyePitchMultiplier;
            float yaw = toTargetLocal.x * 35f * focusEyeYawMultiplier;

            Quaternion focusRot =
                baseRot *
                Quaternion.Euler(focusEyeBaseRotation) *
                Quaternion.Euler(pitch, yaw, 0f);

            targetRot = Quaternion.Slerp(targetRot, focusRot, focusWeight);
            targetPos = Vector3.Lerp(targetPos, basePos + focusEyeBasePosition, focusWeight);

            targetScale = new Vector3(
                baseScale.x * focusEyeHorizontalScale,
                baseScale.y * focusEyeVerticalScale,
                baseScale.z
            );
        }

        // ===== Suspicious =====
        if (suspiciousWeight > 0.0001f)
        {
            float dir = clockwise ? -1f : 1f;
            eyeAngle += suspiciousEyeSpinSpeed * dir * Time.deltaTime;

            float extraYaw = Mathf.Sin(Time.time * suspiciousEyeMoveSpeed) * suspiciousEyeMoveYaw;
            float extraPitch = Mathf.Sin(Time.time * suspiciousEyeMoveSpeed * 0.8f) * suspiciousEyeMovePitch;

            Quaternion suspiciousSpinOffset = MakeAxisRotation(eyeAngle, axis);
            Quaternion suspiciousLookRot =
                baseRot *
                Quaternion.Euler(suspiciousEyeBaseRotation + new Vector3(extraPitch, extraYaw, 0f));

            Quaternion suspiciousTarget = suspiciousLookRot * suspiciousSpinOffset;

            targetRot = Quaternion.Slerp(targetRot, suspiciousTarget, suspiciousWeight);
            targetPos = Vector3.Lerp(targetPos, basePos + suspiciousEyeBasePosition, suspiciousWeight);

            Vector3 suspiciousScale = new Vector3(
                baseScale.x * suspiciousEyeHorizontalScale,
                baseScale.y * suspiciousEyeVerticalScale,
                baseScale.z
            );

            targetScale = Vector3.Lerp(targetScale, suspiciousScale, suspiciousWeight);
        }

        // ===== Greedy =====
        if (greedyWeight > 0.0001f && greedyPlayerTarget != null)
        {
            float dir = clockwise ? -1f : 1f;
            eyeAngle += greedyEyeSpinSpeed * dir * Time.deltaTime;

            Vector3 toTargetWorld = greedyPlayerTarget.position - eye.position;
            Vector3 toTargetLocal = eye.parent != null
                ? eye.parent.InverseTransformDirection(toTargetWorld.normalized)
                : transform.InverseTransformDirection(toTargetWorld.normalized);

            float pitch = -toTargetLocal.y * 35f * greedyEyePitchMultiplier;
            float yaw = toTargetLocal.x * 35f * greedyEyeYawMultiplier;

            Quaternion greedyLookRot =
                baseRot *
                Quaternion.Euler(greedyEyeBaseRotation) *
                Quaternion.Euler(pitch, yaw, 0f);

            Quaternion greedySpinOffset = MakeAxisRotation(eyeAngle, axis);
            Quaternion greedyTarget = greedyLookRot * greedySpinOffset;

            targetRot = Quaternion.Slerp(targetRot, greedyTarget, greedyWeight);
            targetPos = Vector3.Lerp(targetPos, basePos + greedyEyeBasePosition, greedyWeight);

            Vector3 greedyScale = new Vector3(
                baseScale.x * greedyEyeHorizontalScale,
                baseScale.y * greedyEyeVerticalScale,
                baseScale.z
            );

            targetScale = Vector3.Lerp(targetScale, greedyScale, greedyWeight);
        }

        // ===== Aggressive =====
        if (aggressiveWeight > 0.0001f && greedyPlayerTarget != null)
        {
            Vector3 toTargetWorld = greedyPlayerTarget.position - eye.position;
            Vector3 toTargetLocal = eye.parent != null
                ? eye.parent.InverseTransformDirection(toTargetWorld.normalized)
                : transform.InverseTransformDirection(toTargetWorld.normalized);

            float extraYaw = Mathf.Sin(Time.time * aggressiveEyeMoveSpeed) * aggressiveEyeMoveYaw;
            float extraPitch = Mathf.Sin(Time.time * aggressiveEyeMoveSpeed * 0.8f) * aggressiveEyeMovePitch;

            float pitch = -toTargetLocal.y * 35f;
            float yaw = toTargetLocal.x * 35f;

            Quaternion aggressiveLookRot =
                baseRot *
                Quaternion.Euler(aggressiveEyeBaseRotation) *
                Quaternion.Euler(pitch + extraPitch, yaw + extraYaw, 0f);

            targetRot = Quaternion.Slerp(targetRot, aggressiveLookRot, aggressiveWeight);
            targetPos = Vector3.Lerp(targetPos, basePos + aggressiveEyeBasePosition, aggressiveWeight);

            Vector3 aggressiveScale = new Vector3(
                baseScale.x * aggressiveEyeHorizontalScale,
                baseScale.y * aggressiveEyeVerticalScale,
                baseScale.z
            );

            targetScale = Vector3.Lerp(targetScale, aggressiveScale, aggressiveWeight);
        }

        // ===== Bluff Calm =====
        if (bluffCalmWeight > 0.0001f)
        {
            float reverseDir = clockwise ? 1f : -1f;
            eyeAngle += bluffCalmEyeSpinSpeed * reverseDir * Time.deltaTime;

            float extraYaw = Mathf.Sin(Time.time * bluffCalmEyeMicroSpeed) * bluffCalmEyeMicroYaw;
            float extraPitch = Mathf.Sin(Time.time * bluffCalmEyeMicroSpeed * 0.77f) * bluffCalmEyeMicroPitch;

            Quaternion bluffSpinOffset = MakeAxisRotation(eyeAngle, axis);
            Quaternion bluffLookRot =
                baseRot *
                Quaternion.Euler(bluffCalmEyeBaseRotation + new Vector3(extraPitch, extraYaw, 0f));

            Quaternion bluffTarget = bluffLookRot * bluffSpinOffset;

            targetRot = Quaternion.Slerp(targetRot, bluffTarget, bluffCalmWeight);
            targetPos = Vector3.Lerp(targetPos, basePos + bluffCalmEyeBasePosition, bluffCalmWeight);

            Vector3 bluffScale = new Vector3(
                baseScale.x * bluffCalmEyeHorizontalScale,
                baseScale.y * bluffCalmEyeVerticalScale,
                baseScale.z
            );

            targetScale = Vector3.Lerp(targetScale, bluffScale, bluffCalmWeight);
        }

        // ===== Bluff Nervous =====
        if (bluffNervousWeight > 0.0001f)
        {
            float dir = clockwise ? -1f : 1f;
            eyeAngle += bluffNervousEyeSpinSpeed * dir * Time.deltaTime;

            float posX = Mathf.Sin(Time.time * bluffNervousEyeMoveSpeed) * bluffNervousEyeMoveAmountX;
            float posY = Mathf.Sin(Time.time * bluffNervousEyeMoveSpeed * 1.37f) * bluffNervousEyeMoveAmountY;

            float extraYaw = Mathf.Sin(Time.time * bluffNervousEyeMoveSpeed * 0.91f) * bluffNervousEyeYaw;
            float extraPitch = Mathf.Sin(Time.time * bluffNervousEyeMoveSpeed * 1.19f) * bluffNervousEyePitch;

            Quaternion bluffNervousSpinOffset = MakeAxisRotation(eyeAngle, axis);
            Quaternion bluffNervousLookRot =
                baseRot *
                Quaternion.Euler(bluffNervousEyeBaseRotation + new Vector3(extraPitch, extraYaw, 0f));

            Quaternion bluffNervousTarget = bluffNervousLookRot * bluffNervousSpinOffset;

            targetRot = Quaternion.Slerp(targetRot, bluffNervousTarget, bluffNervousWeight);
            targetPos = Vector3.Lerp(
                targetPos,
                basePos + bluffNervousEyeBasePosition + new Vector3(posX, posY, 0f),
                bluffNervousWeight
            );

            Vector3 bluffNervousScale = new Vector3(
                baseScale.x * bluffNervousEyeHorizontalScale,
                baseScale.y * bluffNervousEyeVerticalScale,
                baseScale.z
            );

            targetScale = Vector3.Lerp(targetScale, bluffNervousScale, bluffNervousWeight);
        }

        // ===== Unhinged =====
        if (unhingedWeight > 0.0001f)
        {
            float dir = clockwise ? -1f : 1f;
            eyeAngle += unhingedEyeSpinSpeed * dir * Time.deltaTime;

            float knifeWeight = GetUnhingedKnifeGlanceWeight();
            Transform target = greedyPlayerTarget;

            if (unhingedKnifeTarget != null && knifeWeight > 0f)
                target = unhingedKnifeTarget;

            if (target != null)
            {
                Vector3 toTargetWorld = target.position - eye.position;
                Vector3 toTargetLocal = eye.parent != null
                    ? eye.parent.InverseTransformDirection(toTargetWorld.normalized)
                    : transform.InverseTransformDirection(toTargetWorld.normalized);

                float pitch = -toTargetLocal.y * 35f * unhingedEyePitchMultiplier;
                float yaw = toTargetLocal.x * 35f * unhingedEyeYawMultiplier;

                float extraPosX = Mathf.Sin(Time.time * unhingedEyeMoveSpeed) * unhingedEyeMoveAmountX;
                float extraPosY = Mathf.Sin(Time.time * unhingedEyeMoveSpeed * 1.17f) * unhingedEyeMoveAmountY;

                Quaternion unhingedSpinOffset = MakeAxisRotation(eyeAngle, axis);
                Quaternion unhingedLookRot =
                    baseRot *
                    Quaternion.Euler(unhingedEyeBaseRotation) *
                    Quaternion.Euler(pitch, yaw, 0f);

                Quaternion unhingedTarget = unhingedLookRot * unhingedSpinOffset;

                targetRot = Quaternion.Slerp(targetRot, unhingedTarget, unhingedWeight);
                targetPos = Vector3.Lerp(
                    targetPos,
                    basePos + unhingedEyeBasePosition + new Vector3(extraPosX, extraPosY, 0f),
                    unhingedWeight
                );

                Vector3 unhingedScale = new Vector3(
                    baseScale.x * unhingedEyeHorizontalScale,
                    baseScale.y * unhingedEyeVerticalScale,
                    baseScale.z
                );

                targetScale = Vector3.Lerp(targetScale, unhingedScale, unhingedWeight);
            }
        }

        float rotSpeed = Mathf.Lerp(eyeRotationLerpSpeed, nervousEyeLerpSpeed, nervousWeight);
        rotSpeed = Mathf.Lerp(rotSpeed, focusEyeLookLerpSpeed, focusWeight);
        rotSpeed = Mathf.Lerp(rotSpeed, suspiciousEyeLerpSpeed, suspiciousWeight);
        rotSpeed = Mathf.Lerp(rotSpeed, greedyEyeLerpSpeed, greedyWeight);
        rotSpeed = Mathf.Lerp(rotSpeed, aggressiveEyeLerpSpeed, aggressiveWeight);
        rotSpeed = Mathf.Lerp(rotSpeed, bluffCalmEyeLerpSpeed, bluffCalmWeight);
        rotSpeed = Mathf.Lerp(rotSpeed, bluffNervousEyeLerpSpeed, bluffNervousWeight);
        rotSpeed = Mathf.Lerp(rotSpeed, unhingedEyeLerpSpeed, unhingedWeight);

        float scaleSpeed = Mathf.Lerp(eyeRotationLerpSpeed, nervousEyeLerpSpeed, nervousWeight);
        scaleSpeed = Mathf.Lerp(scaleSpeed, focusEyeScaleLerpSpeed, focusWeight);
        scaleSpeed = Mathf.Lerp(scaleSpeed, suspiciousEyeLerpSpeed, suspiciousWeight);
        scaleSpeed = Mathf.Lerp(scaleSpeed, greedyEyeLerpSpeed, greedyWeight);
        scaleSpeed = Mathf.Lerp(scaleSpeed, aggressiveEyeLerpSpeed, aggressiveWeight);
        scaleSpeed = Mathf.Lerp(scaleSpeed, bluffCalmEyeLerpSpeed, bluffCalmWeight);
        scaleSpeed = Mathf.Lerp(scaleSpeed, bluffNervousEyeLerpSpeed, bluffNervousWeight);
        scaleSpeed = Mathf.Lerp(scaleSpeed, unhingedEyeLerpSpeed, unhingedWeight);

        float posSpeed = Mathf.Lerp(eyePositionLerpSpeed, nervousEyeLerpSpeed, nervousWeight);
        posSpeed = Mathf.Lerp(posSpeed, focusEyeLookLerpSpeed, focusWeight);
        posSpeed = Mathf.Lerp(posSpeed, suspiciousEyeLerpSpeed, suspiciousWeight);
        posSpeed = Mathf.Lerp(posSpeed, greedyEyeLerpSpeed, greedyWeight);
        posSpeed = Mathf.Lerp(posSpeed, aggressiveEyeLerpSpeed, aggressiveWeight);
        posSpeed = Mathf.Lerp(posSpeed, bluffCalmEyeLerpSpeed, bluffCalmWeight);
        posSpeed = Mathf.Lerp(posSpeed, bluffNervousEyeLerpSpeed, bluffNervousWeight);
        posSpeed = Mathf.Lerp(posSpeed, unhingedEyeLerpSpeed, unhingedWeight);

        eye.localRotation = Quaternion.Slerp(
            eye.localRotation,
            targetRot,
            Time.deltaTime * rotSpeed
        );

        eye.localScale = Vector3.Lerp(
            eye.localScale,
            targetScale,
            Time.deltaTime * scaleSpeed
        );

        eye.localPosition = Vector3.Lerp(
            eye.localPosition,
            targetPos,
            Time.deltaTime * posSpeed
        );
    }

    private Transform GetCurrentFocusTarget()
    {
        if (focusTableTarget == null && focusCardsTarget == null)
            return null;

        if (focusTableTarget != null && focusCardsTarget != null)
        {
            float wave = Mathf.Sin(Time.time * focusLookSwitchSpeed) * 0.5f + 0.5f;
            return wave > focusLookBias ? focusTableTarget : focusCardsTarget;
        }

        return focusTableTarget != null ? focusTableTarget : focusCardsTarget;
    }

    private float GetCurrentEyeSpeed()
    {
        return calmEyeSpinSpeed * calmWeight + panicEyeSpinSpeed * panicWeight;
    }

    private Quaternion MakeAxisRotation(float angle, EyeSpinAxis axis)
    {
        switch (axis)
        {
            case EyeSpinAxis.X: return Quaternion.AngleAxis(angle, Vector3.right);
            case EyeSpinAxis.Y: return Quaternion.AngleAxis(angle, Vector3.up);
            default: return Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void SetEmotion(EmotionState newState) => currentState = newState;
    public EmotionState GetCurrentEmotion() => currentState;
}