using UnityEngine;

public class EnemyHeadCalmController : MonoBehaviour
{
    public enum EmotionState
    {
        Calm,
        Nervous,
        Panic
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

    [Header("Future States Placeholder")]
    [SerializeField] private float nervousMultiplier = 1.35f;
    [SerializeField] private float panicMultiplier = 1.8f;

    [Header("Smoothing")]
    [SerializeField] private float eyeRotationLerpSpeed = 12f;

    private Vector3 headBaseLocalPos;
    private Quaternion headBaseLocalRot;

    private Quaternion leftEyeBaseRot;
    private Quaternion rightEyeBaseRot;

    private float leftEyeAngle;
    private float rightEyeAngle;

    private float calmWeight = 1f;
    private float nervousWeight = 0f;
    private float panicWeight = 0f;

    private void Awake()
    {
        if (headMotionPivot == null)
            headMotionPivot = transform;

        headBaseLocalPos = headMotionPivot.localPosition;
        headBaseLocalRot = headMotionPivot.localRotation;

        if (leftEye != null)
            leftEyeBaseRot = leftEye.localRotation;

        if (rightEye != null)
            rightEyeBaseRot = rightEye.localRotation;
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
        float targetNervous = currentState == EmotionState.Nervous ? 1f : 0f;
        float targetPanic = currentState == EmotionState.Panic ? 1f : 0f;

        calmWeight = Mathf.Lerp(calmWeight, targetCalm, Time.deltaTime * stateBlendSpeed);
        nervousWeight = Mathf.Lerp(nervousWeight, targetNervous, Time.deltaTime * stateBlendSpeed);
        panicWeight = Mathf.Lerp(panicWeight, targetPanic, Time.deltaTime * stateBlendSpeed);
    }

    private void AnimateHead()
    {
        float t = Time.time;

        float stateMultiplier =
            1f +
            nervousWeight * (nervousMultiplier - 1f) +
            panicWeight * (panicMultiplier - 1f);

        float moveY = Mathf.Sin(t * calmMoveSpeed * stateMultiplier) * calmMoveAmountY * stateMultiplier;
        float moveX = Mathf.Sin(t * calmMoveSpeed * 0.7f * stateMultiplier) * calmMoveAmountX * stateMultiplier;

        float rotZ = Mathf.Sin(t * calmRotSpeed * stateMultiplier) * calmRotAmountZ * stateMultiplier;

        headMotionPivot.localPosition = headBaseLocalPos + new Vector3(moveX, moveY, 0f);
        headMotionPivot.localRotation = headBaseLocalRot * Quaternion.Euler(0f, 0f, rotZ);
    }

    private void AnimateEyes()
    {
        if (leftEye != null)
        {
            float speed = GetCurrentEyeSpeed();
            float dir = leftEyeClockwise ? -1f : 1f;

            leftEyeAngle += speed * dir * Time.deltaTime;

            Quaternion offset = MakeAxisRotation(leftEyeAngle, leftEyeAxis);
            Quaternion targetRot = leftEyeBaseRot * offset;

            leftEye.localRotation = Quaternion.Slerp(
                leftEye.localRotation,
                targetRot,
                Time.deltaTime * eyeRotationLerpSpeed
            );
        }

        if (rightEye != null)
        {
            float speed = GetCurrentEyeSpeed();
            float dir = rightEyeClockwise ? -1f : 1f;

            rightEyeAngle += speed * dir * Time.deltaTime;

            Quaternion offset = MakeAxisRotation(rightEyeAngle, rightEyeAxis);
            Quaternion targetRot = rightEyeBaseRot * offset;

            rightEye.localRotation = Quaternion.Slerp(
                rightEye.localRotation,
                targetRot,
                Time.deltaTime * eyeRotationLerpSpeed
            );
        }
    }

    private float GetCurrentEyeSpeed()
    {
        float speed = calmEyeSpinSpeed;

        if (nervousWeight > 0.001f)
            speed *= Mathf.Lerp(1f, nervousMultiplier, nervousWeight);

        if (panicWeight > 0.001f)
            speed *= Mathf.Lerp(1f, panicMultiplier, panicWeight);

        return speed;
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

    // ---- Public API for future AI / neural logic ----

    public void SetCalm()
    {
        currentState = EmotionState.Calm;
    }

    public void SetNervous()
    {
        currentState = EmotionState.Nervous;
    }

    public void SetPanic()
    {
        currentState = EmotionState.Panic;
    }

    public void SetEmotion(EmotionState newState)
    {
        currentState = newState;
    }

    public void SetEmotionByIndex(int index)
    {
        if (index < 0 || index > 2) return;
        currentState = (EmotionState)index;
    }

    public EmotionState GetCurrentEmotion()
    {
        return currentState;
    }
}