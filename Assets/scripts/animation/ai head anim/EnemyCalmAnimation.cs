using UnityEngine;

public class EnemyCalmAnimation : MonoBehaviour
{
    public enum EyeSpinAxis
    {
        X,
        Y,
        Z
    }

    [Header("Refs")]
    [SerializeField] private Transform headMotionPivot;
    [SerializeField] private Transform leftEyeSpinPivot;
    [SerializeField] private Transform rightEyeSpinPivot;

    [Header("Head Calm Motion")]
    [SerializeField] private float headMoveAmountY = 0.03f;
    [SerializeField] private float headMoveAmountX = 0.01f;
    [SerializeField] private float headMoveSpeed = 0.8f;

    [SerializeField] private float headRotAmountZ = 2f;
    [SerializeField] private float headRotSpeed = 0.5f;

    [Header("Eye Spin")]
    [SerializeField] private float eyeSpinSpeed = 60f;
    [SerializeField] private EyeSpinAxis eyeSpinAxis = EyeSpinAxis.Z;
    [SerializeField] private bool invertSpin = false;
    [SerializeField] private float eyeLerpSpeed = 12f;

    private Vector3 headBaseLocalPos;
    private Quaternion headBaseLocalRot;

    private Quaternion leftEyePivotBaseRot;
    private Quaternion rightEyePivotBaseRot;

    private float eyeSpinAngle;

    private void Start()
    {
        if (headMotionPivot == null)
            headMotionPivot = transform;

        headBaseLocalPos = headMotionPivot.localPosition;
        headBaseLocalRot = headMotionPivot.localRotation;

        if (leftEyeSpinPivot != null)
            leftEyePivotBaseRot = leftEyeSpinPivot.localRotation;

        if (rightEyeSpinPivot != null)
            rightEyePivotBaseRot = rightEyeSpinPivot.localRotation;
    }

    private void Update()
    {
        AnimateHead();
        AnimateEyes();
    }

    private void AnimateHead()
    {
        float t = Time.time;

        float moveY = Mathf.Sin(t * headMoveSpeed) * headMoveAmountY;
        float moveX = Mathf.Sin(t * headMoveSpeed * 0.65f) * headMoveAmountX;

        headMotionPivot.localPosition = headBaseLocalPos + new Vector3(moveX, moveY, 0f);

        float rotZ = Mathf.Sin(t * headRotSpeed) * headRotAmountZ;
        headMotionPivot.localRotation = headBaseLocalRot * Quaternion.Euler(0f, 0f, rotZ);
    }

    private void AnimateEyes()
    {
        if (leftEyeSpinPivot == null || rightEyeSpinPivot == null) return;

        float dir = invertSpin ? -1f : 1f;
        eyeSpinAngle += eyeSpinSpeed * dir * Time.deltaTime;

        Quaternion spinOffset = GetSpinOffset(eyeSpinAngle);

        Quaternion leftTarget = leftEyePivotBaseRot * spinOffset;
        Quaternion rightTarget = rightEyePivotBaseRot * spinOffset;

        leftEyeSpinPivot.localRotation = Quaternion.Slerp(
            leftEyeSpinPivot.localRotation,
            leftTarget,
            Time.deltaTime * eyeLerpSpeed
        );

        rightEyeSpinPivot.localRotation = Quaternion.Slerp(
            rightEyeSpinPivot.localRotation,
            rightTarget,
            Time.deltaTime * eyeLerpSpeed
        );
    }

    private Quaternion GetSpinOffset(float angle)
    {
        switch (eyeSpinAxis)
        {
            case EyeSpinAxis.X:
                return Quaternion.AngleAxis(angle, Vector3.right);
            case EyeSpinAxis.Y:
                return Quaternion.AngleAxis(angle, Vector3.up);
            default:
                return Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}