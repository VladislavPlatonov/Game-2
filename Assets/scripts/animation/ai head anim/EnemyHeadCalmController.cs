using UnityEngine;

public class EnemyHeadCalmController : MonoBehaviour
{
    public enum EmotionState
    {
        Calm,
        Panic,
        Focus,
        Suspicious,
        Greedy,
        Aggressive
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

    [Header("Greedy / Aggressive Look Target")]
    [SerializeField] private Transform greedyPlayerTarget;

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
    private float panicWeight = 0f;
    private float focusWeight = 0f;
    private float suspiciousWeight = 0f;
    private float greedyWeight = 0f;
    private float aggressiveWeight = 0f;

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
    }

    private void Update()
    {
        UpdateStateWeights();
        AnimateHead();
        AnimateEyes();
    }

    private void UpdateStateWeights()
    {
        float targetCalm = currentState == EmotionState.Calm ? 1f : 0f;
        float targetPanic = currentState == EmotionState.Panic ? 1f : 0f;
        float targetFocus = currentState == EmotionState.Focus ? 1f : 0f;
        float targetSuspicious = currentState == EmotionState.Suspicious ? 1f : 0f;
        float targetGreedy = currentState == EmotionState.Greedy ? 1f : 0f;
        float targetAggressive = currentState == EmotionState.Aggressive ? 1f : 0f;

        calmWeight = Mathf.Lerp(calmWeight, targetCalm, Time.deltaTime * stateBlendSpeed);
        panicWeight = Mathf.Lerp(panicWeight, targetPanic, Time.deltaTime * stateBlendSpeed);
        focusWeight = Mathf.Lerp(focusWeight, targetFocus, Time.deltaTime * stateBlendSpeed);
        suspiciousWeight = Mathf.Lerp(suspiciousWeight, targetSuspicious, Time.deltaTime * stateBlendSpeed);
        greedyWeight = Mathf.Lerp(greedyWeight, targetGreedy, Time.deltaTime * stateBlendSpeed);
        aggressiveWeight = Mathf.Lerp(aggressiveWeight, targetAggressive, Time.deltaTime * stateBlendSpeed);

        float total = calmWeight + panicWeight + focusWeight + suspiciousWeight + greedyWeight + aggressiveWeight;
        if (total > 0.0001f)
        {
            calmWeight /= total;
            panicWeight /= total;
            focusWeight /= total;
            suspiciousWeight /= total;
            greedyWeight /= total;
            aggressiveWeight /= total;
        }
    }

    private void AnimateHead()
    {
        float t = Time.time;

        float calmMoveY = Mathf.Sin(t * calmMoveSpeed) * calmMoveAmountY;
        float calmMoveX = Mathf.Sin(t * calmMoveSpeed * 0.7f) * calmMoveAmountX;
        float calmRotZ = Mathf.Sin(t * calmRotSpeed) * calmRotAmountZ;

        float panicVibeX =
            (Mathf.PerlinNoise(t * panicNoiseSpeed, 0.17f) - 0.5f) * 2f * panicNoiseAmountX;

        float panicVibeY =
            (Mathf.PerlinNoise(0.41f, t * panicNoiseSpeed * 1.11f) - 0.5f) * 2f * panicNoiseAmountY;

        Vector3 focusOffsetPos = Vector3.zero;
        Quaternion focusOffsetRot = Quaternion.identity;

        Transform activeFocusTarget = GetCurrentFocusTarget();
        if (activeFocusTarget != null)
        {
            Vector3 toTargetWorld = activeFocusTarget.position - headMotionPivot.position;
            Vector3 toTargetLocal = headMotionPivot.parent != null
                ? headMotionPivot.parent.InverseTransformDirection(toTargetWorld.normalized)
                : transform.InverseTransformDirection(toTargetWorld.normalized);

            float posX = focusHeadOffsetX + Mathf.Clamp(
                toTargetLocal.x * focusHeadPositionInfluenceX,
                -focusMaxHeadOffsetX,
                focusMaxHeadOffsetX
            );

            float posY = focusHeadOffsetY + Mathf.Clamp(
                toTargetLocal.y * focusHeadPositionInfluenceY,
                -focusMaxHeadOffsetY,
                focusMaxHeadOffsetY
            );

            float pitch = -toTargetLocal.y * 35f * focusPitchMultiplier;
            float yaw = toTargetLocal.x * 30f * focusYawMultiplier;
            float roll = -toTargetLocal.x * 10f * focusRollMultiplier;

            focusOffsetPos = new Vector3(posX, posY, 0f);
            focusOffsetRot =
                Quaternion.Euler(focusHeadBaseRotation) *
                Quaternion.Euler(pitch, yaw, roll);
        }
        else
        {
            focusOffsetPos = new Vector3(focusHeadOffsetX, focusHeadOffsetY, 0f);
            focusOffsetRot = Quaternion.Euler(focusHeadBaseRotation);
        }

        float suspiciousRotZ = Mathf.Sin(t * suspiciousTiltSpeed) * suspiciousTiltAmountZ;
        Quaternion suspiciousOffsetRot = Quaternion.Euler(
            suspiciousHeadBaseRotation + new Vector3(0f, 0f, suspiciousRotZ)
        );

        Vector3 greedyOffsetPos = Vector3.zero;
        Quaternion greedyOffsetRot = Quaternion.Euler(greedyHeadBaseRotation);

        if (greedyPlayerTarget != null)
        {
            Vector3 toTargetWorld = greedyPlayerTarget.position - headMotionPivot.position;
            Vector3 toTargetLocal = headMotionPivot.parent != null
                ? headMotionPivot.parent.InverseTransformDirection(toTargetWorld.normalized)
                : transform.InverseTransformDirection(toTargetWorld.normalized);

            float pitch = -toTargetLocal.y * 35f * greedyPitchMultiplier;
            float yaw = toTargetLocal.x * 35f * greedyYawMultiplier;
            float roll = -toTargetLocal.x * 8f * greedyRollMultiplier;

            greedyOffsetPos = new Vector3(greedyOffsetX, greedyOffsetY, greedyOffsetZ);
            greedyOffsetRot =
                Quaternion.Euler(greedyHeadBaseRotation) *
                Quaternion.Euler(pitch, yaw, roll);
        }
        else
        {
            greedyOffsetPos = new Vector3(greedyOffsetX, greedyOffsetY, greedyOffsetZ);
        }

        Vector3 aggressiveOffsetPos = Vector3.zero;
        Quaternion aggressiveOffsetRot = Quaternion.Euler(aggressiveHeadBaseRotation);

        if (greedyPlayerTarget != null)
        {
            Vector3 toTargetWorld = greedyPlayerTarget.position - headMotionPivot.position;
            Vector3 toTargetLocal = headMotionPivot.parent != null
                ? headMotionPivot.parent.InverseTransformDirection(toTargetWorld.normalized)
                : transform.InverseTransformDirection(toTargetWorld.normalized);

            float pitch = -toTargetLocal.y * 35f * aggressivePitchMultiplier;
            float yaw = toTargetLocal.x * 35f * aggressiveYawMultiplier;
            float roll = -toTargetLocal.x * 8f * aggressiveRollMultiplier;

            float punch = Mathf.Sin(t * aggressivePunchSpeed) * aggressivePunchAngle * aggressivePunchAmount;

            aggressiveOffsetPos = new Vector3(0f, 0f, aggressiveOffsetZ);
            aggressiveOffsetRot =
                Quaternion.Euler(aggressiveHeadBaseRotation) *
                Quaternion.Euler(pitch + punch, yaw, roll);
        }
        else
        {
            float punch = Mathf.Sin(t * aggressivePunchSpeed) * aggressivePunchAngle * aggressivePunchAmount;
            aggressiveOffsetPos = new Vector3(0f, 0f, aggressiveOffsetZ);
            aggressiveOffsetRot = Quaternion.Euler(aggressiveHeadBaseRotation + new Vector3(punch, 0f, 0f));
        }

        Vector3 blendedPos =
            new Vector3(calmMoveX, calmMoveY, 0f) * calmWeight +
            new Vector3(panicVibeX, panicVibeY, 0f) * panicWeight +
            focusOffsetPos * focusWeight +
            greedyOffsetPos * greedyWeight +
            aggressiveOffsetPos * aggressiveWeight;

        float blendedRotZ = calmRotZ * calmWeight;

        Vector3 targetPos = headBaseLocalPos + blendedPos;

        Quaternion targetRot =
            headBaseLocalRot *
            Quaternion.Euler(0f, 0f, blendedRotZ) *
            Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(panicHeadBaseRotation), panicWeight) *
            Quaternion.Slerp(Quaternion.identity, focusOffsetRot, focusWeight) *
            Quaternion.Slerp(Quaternion.identity, suspiciousOffsetRot, suspiciousWeight) *
            Quaternion.Slerp(Quaternion.identity, greedyOffsetRot, greedyWeight) *
            Quaternion.Slerp(Quaternion.identity, aggressiveOffsetRot, aggressiveWeight);

        float posLerpSpeed = Mathf.Lerp(8f, focusHeadMoveLerpSpeed, focusWeight);
        posLerpSpeed = Mathf.Lerp(posLerpSpeed, greedyHeadMoveLerpSpeed, greedyWeight);
        posLerpSpeed = Mathf.Lerp(posLerpSpeed, aggressiveHeadMoveLerpSpeed, aggressiveWeight);

        float rotLerpSpeed = Mathf.Lerp(8f, focusHeadRotLerpSpeed, focusWeight);
        rotLerpSpeed = Mathf.Lerp(rotLerpSpeed, suspiciousHeadLerpSpeed, suspiciousWeight);
        rotLerpSpeed = Mathf.Lerp(rotLerpSpeed, greedyHeadRotLerpSpeed, greedyWeight);
        rotLerpSpeed = Mathf.Lerp(rotLerpSpeed, aggressiveHeadRotLerpSpeed, aggressiveWeight);

        headMotionPivot.localPosition = Vector3.Lerp(
            headMotionPivot.localPosition,
            targetPos,
            Time.deltaTime * posLerpSpeed
        );

        headMotionPivot.localRotation = Quaternion.Slerp(
            headMotionPivot.localRotation,
            targetRot,
            Time.deltaTime * rotLerpSpeed
        );
    }

    private void AnimateEyes()
    {
        AnimateSingleEye(
            leftEye,
            leftEyeBaseRot,
            leftEyeBaseScale,
            leftEyeBasePos,
            leftEyeAxis,
            leftEyeClockwise,
            ref leftEyeAngle
        );

        AnimateSingleEye(
            rightEye,
            rightEyeBaseRot,
            rightEyeBaseScale,
            rightEyeBasePos,
            rightEyeAxis,
            rightEyeClockwise,
            ref rightEyeAngle
        );
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

        if (aggressiveWeight > 0.0001f && greedyPlayerTarget != null)
        {
            Vector3 toTargetWorld = greedyPlayerTarget.position - eye.position;
            Vector3 toTargetLocal = eye.parent != null
                ? eye.parent.InverseTransformDirection(toTargetWorld.normalized)
                : transform.InverseTransformDirection(toTargetWorld.normalized);

            float extraYaw = Mathf.Sin(Time.time * aggressiveEyeMoveSpeed) * aggressiveEyeMoveYaw;
            float extraPitch = Mathf.Sin(Time.time * aggressiveEyeMoveSpeed * 0.8f) * aggressiveEyeMovePitch;

            float pitch = -toTargetLocal.y * 35f * 1.0f;
            float yaw = toTargetLocal.x * 35f * 1.0f;

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

        float rotSpeed = Mathf.Lerp(eyeRotationLerpSpeed, focusEyeLookLerpSpeed, focusWeight);
        rotSpeed = Mathf.Lerp(rotSpeed, suspiciousEyeLerpSpeed, suspiciousWeight);
        rotSpeed = Mathf.Lerp(rotSpeed, greedyEyeLerpSpeed, greedyWeight);
        rotSpeed = Mathf.Lerp(rotSpeed, aggressiveEyeLerpSpeed, aggressiveWeight);

        float scaleSpeed = Mathf.Lerp(eyeRotationLerpSpeed, focusEyeScaleLerpSpeed, focusWeight);
        scaleSpeed = Mathf.Lerp(scaleSpeed, suspiciousEyeLerpSpeed, suspiciousWeight);
        scaleSpeed = Mathf.Lerp(scaleSpeed, greedyEyeLerpSpeed, greedyWeight);
        scaleSpeed = Mathf.Lerp(scaleSpeed, aggressiveEyeLerpSpeed, aggressiveWeight);

        float posSpeed = Mathf.Lerp(eyePositionLerpSpeed, focusEyeLookLerpSpeed, focusWeight);
        posSpeed = Mathf.Lerp(posSpeed, suspiciousEyeLerpSpeed, suspiciousWeight);
        posSpeed = Mathf.Lerp(posSpeed, greedyEyeLerpSpeed, greedyWeight);
        posSpeed = Mathf.Lerp(posSpeed, aggressiveEyeLerpSpeed, aggressiveWeight);

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

        if (focusTableTarget != null)
            return focusTableTarget;

        return focusCardsTarget;
    }

    private float GetCurrentEyeSpeed()
    {
        return calmEyeSpinSpeed * calmWeight + panicEyeSpinSpeed * panicWeight;
    }

    private Quaternion MakeAxisRotation(float angle, EyeSpinAxis axis)
    {
        switch (axis)
        {
            case EyeSpinAxis.X:
                return Quaternion.AngleAxis(angle, Vector3.right);
            case EyeSpinAxis.Y:
                return Quaternion.AngleAxis(angle, Vector3.up);
            default:
                return Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void SetCalm() => currentState = EmotionState.Calm;
    public void SetPanic() => currentState = EmotionState.Panic;
    public void SetFocus() => currentState = EmotionState.Focus;
    public void SetSuspicious() => currentState = EmotionState.Suspicious;
    public void SetGreedy() => currentState = EmotionState.Greedy;
    public void SetAggressive() => currentState = EmotionState.Aggressive;
    public void SetEmotion(EmotionState newState) => currentState = newState;

    public void SetEmotionByIndex(int index)
    {
        if (index < 0 || index > 5) return;
        currentState = (EmotionState)index;
    }

    public EmotionState GetCurrentEmotion()
    {
        return currentState;
    }
}
