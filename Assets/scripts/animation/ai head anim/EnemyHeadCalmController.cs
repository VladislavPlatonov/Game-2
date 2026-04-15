using UnityEngine;

public class EnemyHeadCalmController : MonoBehaviour
{
    public enum EmotionState
    {
        Calm,
        Panic,
        Focus,
        Suspicious
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

    [Header("Focus Eyes")]
    [SerializeField] private float focusEyeLookLerpSpeed = 10f;
    [SerializeField] private float focusEyeVerticalScale = 0.72f;
    [SerializeField] private float focusEyeHorizontalScale = 1.02f;
    [SerializeField] private float focusEyeScaleLerpSpeed = 8f;
    [SerializeField] private float focusEyeYawMultiplier = 1.0f;
    [SerializeField] private float focusEyePitchMultiplier = 1.0f;

    [Header("Suspicious Head")]
    [SerializeField] private float suspiciousTiltAmountZ = 5.5f;
    [SerializeField] private float suspiciousTiltSpeed = 0.70f;
    [SerializeField] private float suspiciousYaw = 8f;
    [SerializeField] private float suspiciousPitch = -5f;
    [SerializeField] private float suspiciousHeadLerpSpeed = 5f;

    [Header("Suspicious Eyes")]
    [SerializeField] private float suspiciousEyeBaseYaw = 10f;
    [SerializeField] private float suspiciousEyeBasePitch = -6f;
    [SerializeField] private float suspiciousEyeMoveYaw = 4f;
    [SerializeField] private float suspiciousEyeMovePitch = 2f;
    [SerializeField] private float suspiciousEyeMoveSpeed = 0.85f;
    [SerializeField] private float suspiciousEyeSpinSpeed = 5f;
    [SerializeField] private float suspiciousEyeVerticalScale = 0.95f;
    [SerializeField] private float suspiciousEyeHorizontalScale = 1.0f;
    [SerializeField] private float suspiciousEyeLerpSpeed = 7f;

    [Header("Smoothing")]
    [SerializeField] private float eyeRotationLerpSpeed = 12f;

    private Vector3 headBaseLocalPos;
    private Quaternion headBaseLocalRot;

    private Quaternion leftEyeBaseRot;
    private Quaternion rightEyeBaseRot;

    private Vector3 leftEyeBaseScale;
    private Vector3 rightEyeBaseScale;

    private float leftEyeAngle;
    private float rightEyeAngle;

    private float calmWeight = 1f;
    private float panicWeight = 0f;
    private float focusWeight = 0f;
    private float suspiciousWeight = 0f;

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
        }

        if (rightEye != null)
        {
            rightEyeBaseRot = rightEye.localRotation;
            rightEyeBaseScale = rightEye.localScale;
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

        calmWeight = Mathf.Lerp(calmWeight, targetCalm, Time.deltaTime * stateBlendSpeed);
        panicWeight = Mathf.Lerp(panicWeight, targetPanic, Time.deltaTime * stateBlendSpeed);
        focusWeight = Mathf.Lerp(focusWeight, targetFocus, Time.deltaTime * stateBlendSpeed);
        suspiciousWeight = Mathf.Lerp(suspiciousWeight, targetSuspicious, Time.deltaTime * stateBlendSpeed);

        float total = calmWeight + panicWeight + focusWeight + suspiciousWeight;
        if (total > 0.0001f)
        {
            calmWeight /= total;
            panicWeight /= total;
            focusWeight /= total;
            suspiciousWeight /= total;
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
            focusOffsetRot = Quaternion.Euler(pitch, yaw, roll);
        }
        else
        {
            focusOffsetPos = new Vector3(focusHeadOffsetX, focusHeadOffsetY, 0f);
        }

        float suspiciousRotZ = Mathf.Sin(t * suspiciousTiltSpeed) * suspiciousTiltAmountZ;
        Quaternion suspiciousOffsetRot = Quaternion.Euler(
            suspiciousPitch,
            suspiciousYaw,
            suspiciousRotZ
        );

        Vector3 blendedPos =
            new Vector3(calmMoveX, calmMoveY, 0f) * calmWeight +
            new Vector3(panicVibeX, panicVibeY, 0f) * panicWeight +
            focusOffsetPos * focusWeight;
        // Suspicious НЕ двигает голову по позиции

        float blendedRotZ = calmRotZ * calmWeight;

        Vector3 targetPos = headBaseLocalPos + blendedPos;

        Quaternion targetRot =
            headBaseLocalRot *
            Quaternion.Euler(0f, 0f, blendedRotZ) *
            Quaternion.Slerp(Quaternion.identity, focusOffsetRot, focusWeight) *
            Quaternion.Slerp(Quaternion.identity, suspiciousOffsetRot, suspiciousWeight);

        float posLerpSpeed = Mathf.Lerp(8f, focusHeadMoveLerpSpeed, focusWeight);
        float rotLerpSpeed = Mathf.Lerp(8f, focusHeadRotLerpSpeed, focusWeight);
        rotLerpSpeed = Mathf.Lerp(rotLerpSpeed, suspiciousHeadLerpSpeed, suspiciousWeight);

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
        AnimateSingleEye(leftEye, leftEyeBaseRot, leftEyeBaseScale, leftEyeAxis, leftEyeClockwise, ref leftEyeAngle);
        AnimateSingleEye(rightEye, rightEyeBaseRot, rightEyeBaseScale, rightEyeAxis, rightEyeClockwise, ref rightEyeAngle);
    }

    private void AnimateSingleEye(
        Transform eye,
        Quaternion baseRot,
        Vector3 baseScale,
        EyeSpinAxis axis,
        bool clockwise,
        ref float eyeAngle)
    {
        if (eye == null)
            return;

        Quaternion targetRot = baseRot;
        Vector3 targetScale = baseScale;

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

            Quaternion focusRot = baseRot * Quaternion.Euler(pitch, yaw, 0f);
            targetRot = Quaternion.Slerp(targetRot, focusRot, focusWeight);

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
            Quaternion suspiciousLookRot = baseRot * Quaternion.Euler(
                suspiciousEyeBasePitch + extraPitch,
                suspiciousEyeBaseYaw + extraYaw,
                0f
            );

            Quaternion suspiciousTarget = suspiciousLookRot * suspiciousSpinOffset;

            targetRot = Quaternion.Slerp(targetRot, suspiciousTarget, suspiciousWeight);

            Vector3 suspiciousScale = new Vector3(
                baseScale.x * suspiciousEyeHorizontalScale,
                baseScale.y * suspiciousEyeVerticalScale,
                baseScale.z
            );

            targetScale = Vector3.Lerp(targetScale, suspiciousScale, suspiciousWeight);
        }

        float rotSpeed = Mathf.Lerp(eyeRotationLerpSpeed, focusEyeLookLerpSpeed, focusWeight);
        rotSpeed = Mathf.Lerp(rotSpeed, suspiciousEyeLerpSpeed, suspiciousWeight);

        float scaleSpeed = Mathf.Lerp(eyeRotationLerpSpeed, focusEyeScaleLerpSpeed, focusWeight);
        scaleSpeed = Mathf.Lerp(scaleSpeed, suspiciousEyeLerpSpeed, suspiciousWeight);

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
    public void SetEmotion(EmotionState newState) => currentState = newState;

    public void SetEmotionByIndex(int index)
    {
        if (index < 0 || index > 3) return;
        currentState = (EmotionState)index;
    }

    public EmotionState GetCurrentEmotion()
    {
        return currentState;
    }
}
