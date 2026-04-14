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

    [Header("Nervous Multipliers")]
    [SerializeField] private float nervousMotionMultiplier = 1.35f;
    [SerializeField] private float nervousEyeMultiplier = 1.35f;

    [Header("Panic Head Motion")]
    [SerializeField] private float panicMoveAmountX = 0.02f;
    [SerializeField] private float panicMoveAmountY = 0.03f;
    [SerializeField] private float panicMoveSpeed = 8f;
    [SerializeField] private float panicRotAmountZ = 4f;
    [SerializeField] private float panicRotSpeed = 10f;
    [SerializeField] private float panicNoiseSpeed = 18f;
    [SerializeField] private float panicNoiseAmountX = 0.015f;
    [SerializeField] private float panicNoiseAmountY = 0.012f;
    [SerializeField] private float panicNoiseRotZ = 2.5f;

    [Header("Panic Eye Spin")]
    [SerializeField] private float panicEyeSpinSpeed = 75f;

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

        float total = calmWeight + nervousWeight + panicWeight;
        if (total > 0.0001f)
        {
            calmWeight /= total;
            nervousWeight /= total;
            panicWeight /= total;
        }
    }

    private void AnimateHead()
    {
        float t = Time.time;

        // -----------------------------
        // CALM / NERVOUS BASE
        // -----------------------------
        float calmStateMultiplier =
            1f +
            nervousWeight * (nervousMotionMultiplier - 1f);

        float calmMoveY =
            Mathf.Sin(t * calmMoveSpeed * calmStateMultiplier) *
            calmMoveAmountY *
            calmStateMultiplier;

        float calmMoveX =
            Mathf.Sin(t * calmMoveSpeed * 0.7f * calmStateMultiplier) *
            calmMoveAmountX *
            calmStateMultiplier;

        float calmRotZ =
            Mathf.Sin(t * calmRotSpeed * calmStateMultiplier) *
            calmRotAmountZ *
            calmStateMultiplier;

        // -----------------------------
        // PANIC = PURE VIBRATION
        // -----------------------------
        float panicVibeX =
            (Mathf.PerlinNoise(t * panicNoiseSpeed, 0.17f) - 0.5f) * 2f * panicNoiseAmountX;

        float panicVibeY =
            (Mathf.PerlinNoise(0.41f, t * panicNoiseSpeed * 1.11f) - 0.5f) * 2f * panicNoiseAmountY;

        // Никакого rotation по Z в panic
        float panicRotZ = 0f;

        // -----------------------------
        // BLEND
        // -----------------------------
        float finalMoveX =
            calmMoveX * calmWeight +
            calmMoveX * nervousWeight +
            panicVibeX * panicWeight;

        float finalMoveY =
            calmMoveY * calmWeight +
            calmMoveY * nervousWeight +
            panicVibeY * panicWeight;

        float finalRotZ =
            calmRotZ * calmWeight +
            calmRotZ * nervousWeight +
            panicRotZ * panicWeight;

        headMotionPivot.localPosition = headBaseLocalPos + new Vector3(finalMoveX, finalMoveY, 0f);
        headMotionPivot.localRotation = headBaseLocalRot * Quaternion.Euler(0f, 0f, finalRotZ);
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
        float speed =
            calmEyeSpinSpeed * calmWeight +
            (calmEyeSpinSpeed * nervousEyeMultiplier) * nervousWeight +
            panicEyeSpinSpeed * panicWeight;

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

    // ---- Public API for AI / neural logic ----

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
